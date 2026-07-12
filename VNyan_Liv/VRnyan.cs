using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using VNyanInterface;

namespace VRnyan {
    [DefaultExecutionOrder(15000)]
    public class CameraTransform {
        public Vector3 Position;
        public Quaternion Rotation;
        public DateTime TargetTime;

        public CameraTransform(Vector3 _Position, Quaternion _Rotation, DateTime _TargetTime) {
            Position = _Position;
            Rotation = _Rotation;
            TargetTime = _TargetTime;
        }
        public bool Ready {
            get { return (DateTime.UtcNow >= TargetTime); }
        }
        public void SetCam() {
            Camera.main.transform.position = Position;
            Camera.main.transform.rotation = Rotation;
        }
    }
    
    public class VRnyan : MonoBehaviour, IVNyanPluginManifest, IButtonClickedHandler, ITriggerHandler {
        private const string VersionString = "2.0-RC1";
        public string PluginName { get; } = SharedValues.PluginName;
        public string Version { get; } = VersionString;
        public string Title { get; } = SharedValues.PluginName+" "+VersionString;
        public string Author { get; } = SharedValues.Author;
        public string Website { get; } = SharedValues.Website;

        private readonly string OldSettingsFileName = VNyanInterface.VNyanInterface.VNyanSettings.getProfilePath()+"\\LIVnyan.cfg";
        private readonly string SettingsFileName = VNyanInterface.VNyanInterface.VNyanSettings.getProfilePath() + "\\VRnyan.cfg";

        private static float[] CamData = new float[9];
        private static MemoryMappedFile mmf = null;
        private static MemoryMappedViewAccessor mmfAccess;
        private static int VNyanSettings = 2;
        private static GameObject objVRnyan;
        private static uint CursedCameraDelay = 0;
        private static HumanBodyBones? BoneClip = HumanBodyBones.Hips;   //_lum_liv_BoneClip
        private static float BoneClipDistanceAdjust = 0;                 //_lum_liv_BoneClipDistanceAdjust
        private static bool BoneClipDistanceAdjust2DOnly = true;
        private static List<CameraTransform> CursedCamera = new List<CameraTransform>();


        private void ErrorHandler(Exception e) {
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString("_lum_liv_err", e.ToString());
            UnityEngine.Debug.Log("[VRnyan] ERR:" + e.ToString());
        }

        private void Log(string message) {
            if ((VNyanSettings & SharedValues.LOGENABLED) != 0) {
                UnityEngine.Debug.Log("[VRnyan] " + message);
            }
        }

        public void InitializePlugin() {
            try {
                Log("VRNyan version " + Version + " started");
                string OldDLLpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Items\\Assemblies\\VNyan_Liv.dll");
                Log("Checking for old DLL in: " + OldDLLpath);
                if (System.IO.File.Exists(OldDLLpath)) {
                    Log("ERROR: Old VNyan_LIV.dll detected, disabling VRnyan");
                    Log("ERROR: Please delete " + OldDLLpath);
                    VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton("VRnyan: Upgrade incomplete, please see https://lum.uk/VRN", null);
                    return;
                } else {
                    Log("Register plugin button");
                    VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton("VRnyan", this);
                }

                Log("Spawning gameobject: VRnyan");
                objVRnyan = new GameObject("VRnyan", typeof(VRnyan));
                objVRnyan.SetActive(false);
                Log("Register trigger listener");
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);
                
                
                LoadPluginSettings();
                objVRnyan.SetActive((VNyanSettings & SharedValues.CAMENABLED) != 0);
                InitialiseMMF();
                Log("Window size set to to: " + Screen.width.ToString() + "," + Screen.height.ToString());
                mmfAccess.Write(SharedValues.MMFPos_ResX, Screen.width);
                mmfAccess.Write(SharedValues.MMFPos_ResY, Screen.height);
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }

        private void LoadPluginSettings() {
            try {
                Dictionary<string, string> settings = null;
                bool SettingMissing = true;
                // Get settings in dictionary
                if (!System.IO.File.Exists(SettingsFileName)) {
                    if (System.IO.File.Exists(OldSettingsFileName)) {
                        Log("Old LIVnyan.cfg file found, renaming to VRnyan.cfg and reading");
                        System.IO.File.Move(OldSettingsFileName, SettingsFileName);
                        settings = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings(SettingsFileName);
                        SettingMissing = false;
                    }
                } else {
                    Log("Reading settings from: " + SettingsFileName);
                    settings = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings(SettingsFileName);
                    SettingMissing = false;
                }
                int tempVNyanSettings = 0;
                if (settings != null) {
                    // Read string value
                    string tempSetting;

                    if (settings.TryGetValue("ActiveOnStart", out tempSetting)) {
                        if (bool.Parse(tempSetting)) {
                            tempVNyanSettings += SharedValues.CAMENABLED;
                            Log("Camera sync enabled on startup");
                        } else {
                            Log("Camera sync disabled on startup");
                        }
                    } else {
                        Log("ActiveOnStart setting missing, defaulting to disabled");
                        SettingMissing = true;
                    }
                    if (settings.TryGetValue("LogEnabled", out tempSetting)) {
                        if (bool.Parse(tempSetting)) {
                            tempVNyanSettings += SharedValues.LOGENABLED;
                            Log("Logging enabled");
                        } else {
                            Log("Logging disabled");
                        }
                    } else {
                        Log("LogEnabled setting missing, defaulting to disabled");
                        SettingMissing = true;
                    }
                    if (settings.TryGetValue("LogSpam", out tempSetting)) {
                        if (bool.Parse(tempSetting)) {
                            tempVNyanSettings += SharedValues.LOGSPAMENABLED;
                            Log("Log spam enabled");
                        } else {
                            Log("Log spam disabled");
                        }
                    } else {
                        Log("ActiveOnStart setting missing, defaulting to disabled");
                        SettingMissing = true;
                    }
                    if (settings.TryGetValue("CursedCamera", out tempSetting)) {
                        if (uint.TryParse(tempSetting, out CursedCameraDelay)) {
                            Log("Cursed Camera delay set to: " + CursedCameraDelay.ToString());
                        } else {
                            Log("Cursed Camera disabled");
                            SettingMissing = true;
                        }
                    } else {
                        Log("Cursed Camera setting missing, defaulting to disabled");
                        SettingMissing = true;
                    }
                    if (settings.TryGetValue("BoneClip", out tempSetting)) {
                        HumanBodyBones TempBoneClip;

                        if (Enum.TryParse<HumanBodyBones>(tempSetting, out TempBoneClip)) {
                            BoneClip = TempBoneClip;
                            tempVNyanSettings += SharedValues.OATREADCLIPPLANEPOS;
                            Log("Clipping bone tracker set to: " + TempBoneClip.ToString());
                        } else {
                            Log("Clipping bone tracker setting invalid, defaulting to hips");
                            SettingMissing = true;
                        }
                    } else {
                        Log("Clipping bone tracker setting missing, defaulting to hips");
                        SettingMissing = true;
                    }
                    if (settings.TryGetValue("BoneClipDistanceAdjust", out tempSetting)) {
                        if (float.TryParse(tempSetting, out BoneClipDistanceAdjust)) {
                            Log("Bone Clip Distance Adjustment set to: " + BoneClipDistanceAdjust.ToString());
                        } else {
                            Log("Bone Clip Distance Adjustment setting invalid, defaulting to 0 ");
                            SettingMissing = true;
                        }
                    } else {
                        Log("Bone Clip Distance Adjustment setting missing, defaulting to 0 ");
                        SettingMissing = true;
                    }
                    if (settings.TryGetValue("BoneClipDistanceAdjust2DOnly", out tempSetting)) {
                        if (bool.Parse(tempSetting)) {
                            BoneClipDistanceAdjust2DOnly = true;
                            Log("Bone Clip Distance Adjustment: 2D");
                        } else {
                            Log("Bone Clip Distance Adjustment: 3D");
                        }
                    } else {
                        Log("Bone Clip Distance Adjustment (2D Only) setting missing, defaulting to 2D");
                        SettingMissing = true;
                    }


                } else {
                    Log("No settings file detected, using defaults");
                    SettingMissing = true;
                }
                if (SettingMissing) {
                    Log("Writing settings file");
                    SavePluginSettings();
                }
                VNyanSettings = tempVNyanSettings;
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }

        private void SavePluginSettings() {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["ActiveOnStart"] = ((VNyanSettings & SharedValues.CAMENABLED) != 0).ToString();
            settings["LogEnabled"]    = ((VNyanSettings & SharedValues.LOGENABLED) != 0).ToString();
            settings["LogSpam"]       = false.ToString();
            settings["CursedCamera"]  = CursedCameraDelay.ToString();
            settings["BoneClip"]      = BoneClip.ToString();
            settings["BoneClipDistanceAdjust"] = BoneClipDistanceAdjust.ToString();
            settings["BoneClipDistanceAdjust2DOnly"] = BoneClipDistanceAdjust2DOnly.ToString();
            VNyanInterface.VNyanInterface.VNyanSettings.saveSettings(SettingsFileName, settings);
        }

        public void pluginButtonClicked() {
            Log("Plugin button clicked");
            SetActive(!objVRnyan.activeInHierarchy);
            Log("Enabled: " + ((VNyanSettings & SharedValues.CAMENABLED) != 0).ToString());
        }
        
        private void InitialiseMMF() {
            if (mmf == null) {
                Log("Creating file");
                mmf = MemoryMappedFile.CreateOrOpen(SharedValues.MMFname, SharedValues.MMFSize);
                Log("Creating accessor");
                mmfAccess = mmf.CreateViewAccessor(0, SharedValues.MMFSize);
            }
        }

        private void SetActive(bool Active) {
            if (Active) {
                Log("Initialise MMF");
                InitialiseMMF();
                Log("Update Settings");
                VNyanSettings = VNyanSettings | SharedValues.CAMENABLED;
                Log("Write settings to MMF");
                mmfAccess.Write(SharedValues.MMFPos_Settings, VNyanSettings);
                Log("Enable VRnyan GameObject");
                objVRnyan.SetActive(true);
                Log("Disable physical camera");
                Camera.main.usePhysicalProperties = false;
            } else {
                VNyanSettings = (VNyanSettings | SharedValues.CAMENABLED) - SharedValues.CAMENABLED;
                objVRnyan.SetActive(false);
                CursedCamera.Clear();
                mmfAccess.Write(SharedValues.MMFPos_Settings, VNyanSettings);
                Camera.main.usePhysicalProperties = true;
            }
        }
        
        public void triggerCalled(string name, int int1, int int2, int int3, string text1, string text2, string text3) {
            try {
                if (name.Length > 10) {
                    name = name.ToLower();
                    if (name.Substring(0, 8) == "_lum_vr_") {
                        Log("Detected trigger: " + name);
                        name = name.Substring(7);
                    } else if (name.Substring(0, 9) == "_lum_liv_") {
                        Log("Detected trigger: " + name);
                        name = name.Substring(8);
                    } else {
                        return;
                    }
                    switch (name) {
                        case "_enable":
                            if (int1 > 0) {
                                CursedCameraDelay = (uint)int1;
                                Log("CursedCamera set to: " + CursedCameraDelay.ToString());
                            } else if (int1 < 0) {
                                CursedCameraDelay = 0;
                                Log("CursedCamera disabled");
                            }
                            SetActive(true);
                            break;
                        case "_disable":
                            SetActive(false);
                            break;
                        case "_setbone":
                            if (text1.Length > 0) {
                                HumanBodyBones TempBoneClip;
                                if (Enum.TryParse<HumanBodyBones>(text1, out TempBoneClip)) {
                                    if ((VNyanSettings & SharedValues.OATREADCLIPPLANEPOS) == 0) { VNyanSettings += SharedValues.OATREADCLIPPLANEPOS; }
                                    BoneClip = TempBoneClip;
                                    Log("Clipping bone tracker set to: " + BoneClip.ToString());
                                } else {
                                    //TODO: Talk with MilkyDelta about VNyan controlling the ReadClipPlaneLocation
                                    if ((VNyanSettings & SharedValues.OATREADCLIPPLANEPOS) != 0) { VNyanSettings -= SharedValues.OATREADCLIPPLANEPOS; }
                                    BoneClip = null;
                                }
                            }
                            if (text2.Length > 0) {
                                float TempBoneClipDist;
                                if (float.TryParse(text2, out TempBoneClipDist)) {
                                    BoneClipDistanceAdjust = TempBoneClipDist;
                                    Log("Bone Clip Distance Adjustment set to: " + BoneClipDistanceAdjust.ToString());
                                }
                            }
                            break;
                    }
                }
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }

        public void OnRectTransformDimensionsChange() {
            Log("Window size changed to: " + Screen.width.ToString() + "," + Screen.height.ToString());
            mmfAccess.Write(SharedValues.MMFPos_ResX, Screen.width);
            mmfAccess.Write(SharedValues.MMFPos_ResY, Screen.height);
        }

        public void LateUpdate() {
            try {
                // var camera = Camera.main;
                mmfAccess.Write(SharedValues.MMFPos_CamPosX, Camera.main.transform.position.x);
                mmfAccess.Write(SharedValues.MMFPos_CamPosY, Camera.main.transform.position.y);
                mmfAccess.Write(SharedValues.MMFPos_CamPosZ, Camera.main.transform.position.z);
                mmfAccess.Write(SharedValues.MMFPos_CamRotW, Camera.main.transform.rotation.w);
                mmfAccess.Write(SharedValues.MMFPos_CamRotX, Camera.main.transform.rotation.x);
                mmfAccess.Write(SharedValues.MMFPos_CamRotY, Camera.main.transform.rotation.y);
                mmfAccess.Write(SharedValues.MMFPos_CamRotZ, Camera.main.transform.rotation.z);
                mmfAccess.Write(SharedValues.MMFPos_CamFOV,  Camera.main.fieldOfView);
                
                // Only used by OnAirTap. Ignored by LIV_VNyan.dll
                mmfAccess.Write(SharedValues.MMFPos_ResX, Screen.width);
                mmfAccess.Write(SharedValues.MMFPos_ResY, Screen.height);

                if (BoneClip != null) {
                    GameObject AvatarObject = (GameObject)VNyanInterface.VNyanInterface.VNyanAvatar.getAvatarObject();
                    Animator AvatarAnimator = AvatarObject.GetComponent<Animator>();
                    Transform BoneTransform = AvatarAnimator.GetBoneTransform((HumanBodyBones)BoneClip);

                    if (BoneClipDistanceAdjust != 0) {
                        Vector3 AdjustmentVector3D = BoneTransform.position - Camera.main.transform.position;
                        if (BoneClipDistanceAdjust2DOnly) {
                            AdjustmentVector3D.y = 0;
                        }
                        Vector3 ClipPos = BoneTransform.position + (AdjustmentVector3D.normalized * BoneClipDistanceAdjust);
                        mmfAccess.Write(SharedValues.MMFPos_ClipPosX, ClipPos.x);
                        mmfAccess.Write(SharedValues.MMFPos_ClipPosY, ClipPos.y);
                        mmfAccess.Write(SharedValues.MMFPos_ClipPosZ, ClipPos.z);
                        if ((VNyanSettings & SharedValues.LOGSPAMENABLED) != 0) {
                            Log("Set Bone POS: " + ClipPos.ToString());
                        }
                    } else {
                        mmfAccess.Write(SharedValues.MMFPos_ClipPosX, BoneTransform.position.x);
                        mmfAccess.Write(SharedValues.MMFPos_ClipPosY, BoneTransform.position.y);
                        mmfAccess.Write(SharedValues.MMFPos_ClipPosZ, BoneTransform.position.z);
                        if ((VNyanSettings & SharedValues.LOGSPAMENABLED) != 0) {
                            Log("Set Bone POS: " + BoneTransform.position.ToString());
                        }
                    }
                }

                if ((VNyanSettings & SharedValues.LOGSPAMENABLED) !=0) {
                    //Log("Set POS: " + Camera.main.transform.position.ToString() + " ROT: " + Camera.main.transform.rotation.ToString() + " FOV: " + Camera.main.fieldOfView + " Settings: " + VNyanSettings);
                    
                    /*if (FramesElapsed >= 60) { FramesElapsed = 0; }
                    if (FramesElapsed == 0) {
                        Log("FOV                    : " + Camera.main.fieldOfView.ToString());
                        Log("Physical Camera Enabled: " + Camera.main.usePhysicalProperties.ToString());
                        Log("Focal Length           : " + Camera.main.focalLength.ToString());
                        Log("Orthograhpic           : " + Camera.main.orthographic.ToString());
                        Log("Sensor Size            : " + Camera.main.sensorSize.ToString());
                        Log("Lens Shift             : " + Camera.main.lensShift.ToString());
                        Log("Gate Fit               : " + Camera.main.gateFit.ToString());
                        Log("Height                 : " + Camera.main.pixelHeight.ToString());
                        Log("Width                  : " + Camera.main.pixelWidth.ToString());
                        Log("----------------------------------------------------");
                    }
                    FramesElapsed++;*/
                }
                if (CursedCameraDelay > 0) {
                    CursedCamera.Add(new CameraTransform(Camera.main.transform.position, Camera.main.transform.rotation, DateTime.UtcNow.AddMilliseconds(CursedCameraDelay)));
                    //Log("New Frame");
                    int Count = CursedCamera.Count;
                    //Log("0/" + Count.ToString());
                    
                    if (!CursedCamera[0].Ready) {
                        CursedCamera[0].SetCam();
                    } else {
                        int n = 1;
                        while (n < CursedCamera.Count && CursedCamera[n].Ready) {
                            //Log(n.ToString()+"/" + CursedCamera.Count.ToString());
                            n++;
                        }
                        CursedCamera[n - 1].SetCam();
                        CursedCamera.RemoveRange(0, n);
                    }
                    //Log ("Queue Len: "+CursedCamera.Count.ToString()+" Time: "+DateTime.UtcNow.ToString()+" Next trg time: " + CursedCamera[0].TargetTime);
                }
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
    }
}