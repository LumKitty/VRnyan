using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;
using UnityEngine;
using static VRnyan.Functions;

namespace VRnyan {
    public class FollowCam_Handlers {
        //public static bool FollowCamConnected = false;

        public static bool VRNyanControllingCamera = false;
        public static bool MainFollowCamActive = false;
        internal static Vector3 FollowCamPos;
        internal static Quaternion FollowCamRot;

        public static void UpdateCameraPos(Vector3 CamPos, Quaternion CamRot) { 
            if (Settings.CursedCameraDelay > 0) {
                VRnyan.UpdateCursedCamera(CamPos, CamRot, "FollowCam");
            } 
            FollowCamPos = CamPos;
            FollowCamRot = CamRot;
            VRnyan.UpdateMMF(CamPos, CamRot, "FollowCam");
        }

        public static bool GetVRNyanControllingCamera() { return VRNyanControllingCamera; }
        public static void SetMainFollowCamActive(bool Active) { MainFollowCamActive = Active; }
        public static bool GetMainFollowCamActive() { return MainFollowCamActive; }

        public static Action<Vector3, Quaternion> Get_UpdateCameraPos() { return UpdateCameraPos; }
        public static Func<bool> Get_GetVRNyanControllingCamera() { return GetVRNyanControllingCamera; }
        public static Action<bool> Get_SetMainFollowCamActive() { return SetMainFollowCamActive; }
        public static Func<bool> Get_GetMainFollowCamActive() { return GetMainFollowCamActive; }

        //public static void LogAssemblyName() {
        //    Log($"Handler Assembly name: {typeof(FollowCam_Handlers).AssemblyQualifiedName}");
        //}
    }
}