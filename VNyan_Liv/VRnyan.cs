using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using VNyanInterface;
using static VRnyan.Functions;
using static VRnyan.Settings;

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
    
    public class VRnyan : MonoBehaviour {

        internal static bool FollowCamEnabled = false;
        private static float[] CamData = new float[9];
        
        internal static MemoryMappedViewAccessor mmfAccess = null;
        private static GameObject objVRnyan = new GameObject("VRnyan", typeof(VRnyan));

        private static List<CameraTransform> CursedCamera = new List<CameraTransform>();

        internal static bool IsActive => objVRnyan.activeSelf;

        internal static void SetActive(bool Active) {
            if (Active && !objVRnyan.activeSelf) {
                if (mmfAccess == null) {
                    if (IsWine() && (LinuxRootDriveLetter >= 'a') && (LinuxRootDriveLetter <= 'z')) {
                        Log("Initialise MMF - Wine/Linux shared memory");
                        mmfAccess = MMF_Wine.InitialiseMMF();
                    } else {
                        Log("Initialise MMF - Windows shared memory");
                        mmfAccess = MMF_Windows.InitialiseMMF();
                    }
                }
                Log("Update Settings");
                VNyanSettings = VNyanSettings | SharedValues.CAMENABLED;
                Log("Write settings to MMF");
                mmfAccess.Write(SharedValues.MMFPos_Settings, VNyanSettings);
                Log("Enable VRnyan GameObject");
                objVRnyan.SetActive(true);
                Log("Disable physical camera");
                Camera.main.usePhysicalProperties = false;
            } else if (!Active && objVRnyan.activeSelf) {
                VNyanSettings = (VNyanSettings | SharedValues.CAMENABLED) - SharedValues.CAMENABLED;
                objVRnyan.SetActive(false);
                CursedCamera.Clear();
                if (mmfAccess != null) { mmfAccess.Write(SharedValues.MMFPos_Settings, VNyanSettings); }
                Camera.main.usePhysicalProperties = true;
            }
        }
        
        public static void EnableFollowCam() {
            FollowCamEnabled = true;
            Log("FollowCam enabled");
        }
        public static void DisableFollowCam() {
            FollowCamEnabled = false;
            Log("FollowCam disabled");
        }

        public void OnRectTransformDimensionsChange() {
            Log("Window size changed to: " + Screen.width.ToString() + "," + Screen.height.ToString());
            mmfAccess.Write(SharedValues.MMFPos_ResX, Screen.width);
            mmfAccess.Write(SharedValues.MMFPos_ResY, Screen.height);
        }

        public void LateUpdate() {
            
            Vector3 CamPos;
            Quaternion CamRot;
            try {
                if (FollowCamEnabled) {
                    CamPos = VNyan_Handlers.GetFollowCamPos();
                    CamRot = VNyan_Handlers.GetFollowCamRot();
                } else {
                    CamPos = Camera.main.transform.position;
                    CamRot = Camera.main.transform.rotation;
                }

                // var camera = Camera.main;
                mmfAccess.Write(SharedValues.MMFPos_CamPosX, CamPos.x);
                mmfAccess.Write(SharedValues.MMFPos_CamPosY, CamPos.y);
                mmfAccess.Write(SharedValues.MMFPos_CamPosZ, CamPos.z);
                mmfAccess.Write(SharedValues.MMFPos_CamRotW, CamRot.w);
                mmfAccess.Write(SharedValues.MMFPos_CamRotX, CamRot.x);
                mmfAccess.Write(SharedValues.MMFPos_CamRotY, CamRot.y);
                mmfAccess.Write(SharedValues.MMFPos_CamRotZ, CamRot.z);
                mmfAccess.Write(SharedValues.MMFPos_CamFOV,  Camera.main.fieldOfView);
                
                // Only used by OnAirTap. Ignored by LIV_VNyan.dll
                mmfAccess.Write(SharedValues.MMFPos_ResX, Screen.width);
                mmfAccess.Write(SharedValues.MMFPos_ResY, Screen.height);

                if (BoneClip != null) {
                    GameObject AvatarObject = (GameObject)VNyanInterface.VNyanInterface.VNyanAvatar.getAvatarObject();
                    Animator AvatarAnimator = AvatarObject.GetComponent<Animator>();
                    Transform BoneTransform = AvatarAnimator.GetBoneTransform((HumanBodyBones)BoneClip);

                    if (BoneClipDistanceAdjust != 0) {
                        Vector3 AdjustmentVector3D = BoneTransform.position - CamPos;
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
                    CursedCamera.Add(new CameraTransform(CamPos, CamRot, DateTime.UtcNow.AddMilliseconds(CursedCameraDelay)));
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