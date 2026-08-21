using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VNyanInterface;
using static VRnyan.Functions;
using static VRnyan.Settings;

namespace VRnyan {
    public class VNyan_Handlers : IVNyanPluginManifest, IButtonClickedHandler, ITriggerHandler {
        private const string VersionString = "2.3-beta2";
        public string PluginName { get; } = SharedValues.PluginName;
        public string Version { get; } = VersionString;
        public string Title { get; } = SharedValues.PluginName + " " + VersionString;
        public string Author { get; } = SharedValues.Author;
        public string Website { get; } = SharedValues.Website;

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
                
                //Log("Spawning gameobject: VRnyan");
                //VRnyan.objVRnyan = new GameObject("VRnyan", typeof(VRnyan));
                //objVRnyan.SetActive(false);
                Log("Register trigger listener");
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);

                VRnyan.SetActive(false);
                LoadPluginSettings();
                VRnyan_GUI.SetActive(false);
                
                Log($"VNyanSettings: {VNyanSettings}");
                if ((VNyanSettings & SharedValues.CAMENABLED) != 0) {
                    Log("Starting VRnyan at launch");
                    VRnyan.SetActive(true);
                }
                    
                //objVRnyan.SetActive((VNyanSettings & SharedValues.CAMENABLED) != 0);
                //mmfAccess = MMF_Windows.InitialiseMMF();
                //Log("Window size set to to: " + Screen.width.ToString() + "," + Screen.height.ToString());
                //mmfAccess.Write(SharedValues.MMFPos_ResX, Screen.width);
                //mmfAccess.Write(SharedValues.MMFPos_ResY, Screen.height);
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }


        public void pluginButtonClicked() {
            Log("Plugin button clicked");
            VRnyan_GUI.ToggleActive();
            //VRnyan.SetActive(!VRnyan.IsActive);
            Log("Enabled: " + ((VNyanSettings & SharedValues.CAMENABLED) != 0).ToString());
            return;
        }

        public void triggerCalled(string name, int int1, int int2, int int3, string text1, string text2, string text3) {
            try {
                if (name == VRnyan_GUI.CloseTriggerName && text1 != VRnyan_GUI.CloseTriggerValue) { VRnyan_GUI.SetActive(false); }
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
                            VRnyan.SetActive(true);
                            break;
                        case "_disable":
                            VRnyan.SetActive(false);
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
            return;
        }
    }
}
