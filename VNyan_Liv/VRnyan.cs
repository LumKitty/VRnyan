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
        public ulong Sequence;
        public double TargetTime;

        public CameraTransform(Vector3 _Position, Quaternion _Rotation, ulong _Sequence, double _TargetTime) {
            Position = _Position;
            Rotation = _Rotation;
            Sequence = _Sequence;
            TargetTime = _TargetTime;
        }
        public bool Ready {
            get { return (UnityEngine.Time.realtimeSinceStartupAsDouble >= TargetTime); }
        }
        public void SetCam() {
            Camera.main.transform.position = Position;
            Camera.main.transform.rotation = Rotation;
            LogSpam($"Sequence counter: {Sequence}, Timestamp: {TargetTime}");
        }
    }
    
    public class VRnyan : MonoBehaviour {

        private static float[] CamData = new float[9];
        
        internal static MemoryMappedViewAccessor mmfAccess = null;
        private static GameObject objVRnyan = new GameObject("VRnyan", typeof(VRnyan));

        internal static Queue<CameraTransform> CursedCamera = new Queue<CameraTransform>();

        internal static bool IsActive => objVRnyan.activeSelf;

        public static void UpdateCursedCamera(Vector3 CamPos, Quaternion CamRot, ulong Sequence, double TimeStamp, string Source="Local") {
            // LogSpam($"Adding {CamPos.ToString()}, {CamRot.ToString()} to Cursed Camera from {Source}");
            CursedCamera.Enqueue(new CameraTransform(CamPos, CamRot, Sequence, TimeStamp + (Settings.CursedCameraDelay / 1000d)));
        }

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
                FollowCam_Handlers.VRNyanControllingCamera = false;
            }
        }
       

        public void OnRectTransformDimensionsChange() {
            Log("Window size changed to: " + Screen.width.ToString() + "," + Screen.height.ToString());
            mmfAccess.Write(SharedValues.MMFPos_ResX, Screen.width);
            mmfAccess.Write(SharedValues.MMFPos_ResY, Screen.height);
        }
        
        public static void UpdateMMF(Vector3 CamPos, Quaternion CamRot, string Source="Local") {
            // Log($"Local UpdateMMF called from {Source}");
            if (mmfAccess != null) {
                mmfAccess.Write(SharedValues.MMFPos_CamPosX, CamPos.x);
                mmfAccess.Write(SharedValues.MMFPos_CamPosY, CamPos.y);
                mmfAccess.Write(SharedValues.MMFPos_CamPosZ, CamPos.z);
                mmfAccess.Write(SharedValues.MMFPos_CamRotW, CamRot.w);
                mmfAccess.Write(SharedValues.MMFPos_CamRotX, CamRot.x);
                mmfAccess.Write(SharedValues.MMFPos_CamRotY, CamRot.y);
                mmfAccess.Write(SharedValues.MMFPos_CamRotZ, CamRot.z);
                mmfAccess.Write(SharedValues.MMFPos_CamFOV, Camera.main.fieldOfView);
            }
        }

        internal static CameraTransform DesiredPos;
        internal static CameraTransform TempPos;

        public void LateUpdate() {
            
            Vector3 CamPos;
            Quaternion CamRot;
            double Now = UnityEngine.Time.realtimeSinceStartupAsDouble;
            try {
                if (FollowCam_Handlers.MainFollowCamActive) {
                    CamPos = FollowCam_Handlers.FollowCamPos;
                    CamRot = FollowCam_Handlers.FollowCamRot;
                } else {
                    CamPos = Camera.main.transform.position;
                    CamRot = Camera.main.transform.rotation;
                    UpdateMMF(CamPos, CamRot);
                }
                
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
                        //if ((VNyanSettings & SharedValues.LOGSPAMENABLED) != 0) {
                        //    Log("Set Bone POS: " + ClipPos.ToString());
                        //}
                    } else {
                        mmfAccess.Write(SharedValues.MMFPos_ClipPosX, BoneTransform.position.x);
                        mmfAccess.Write(SharedValues.MMFPos_ClipPosY, BoneTransform.position.y);
                        mmfAccess.Write(SharedValues.MMFPos_ClipPosZ, BoneTransform.position.z);
                        //if ((VNyanSettings & SharedValues.LOGSPAMENABLED) != 0) {
                        //    Log("Set Bone POS: " + BoneTransform.position.ToString());
                        //}
                    }
                }

                FollowCam_Handlers.VRNyanControllingCamera = (CursedCameraDelay > 0);
                if (FollowCam_Handlers.VRNyanControllingCamera) {
                    if (!FollowCam_Handlers.MainFollowCamActive) { UpdateCursedCamera(CamPos, CamRot,0, Now); }

                    if (CursedCamera.Count >= 1) {
                        TempPos = CursedCamera.Peek();
                        if (TempPos.Ready) {
                            DesiredPos = CursedCamera.Dequeue();
                            TempPos = DesiredPos;

                            while (CursedCamera.TryPeek(out TempPos) && TempPos.Ready) {
                                DesiredPos = CursedCamera.Dequeue();
                            }
                            DesiredPos.SetCam();
                        } else {
                            TempPos.SetCam();
                            DesiredPos = TempPos;
                        }
                    } else {
                        if (FollowCam_Handlers.MainFollowCamActive) {
                            //Camera.main.transform.position = CamPos;
                            //Camera.main.transform.rotation = CamRot;
                            DesiredPos.SetCam();
                        }
                    }

                    //Log($"Queue before: {Count} Queue After: {CursedCamera.Count}");
                }
                // Log($"VRNyanControllingCamera: {FollowCam_Handlers.VRNyanControllingCamera}, MainFollowCamActive: {FollowCam_Handlers.MainFollowCamActive}");
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
    }
}