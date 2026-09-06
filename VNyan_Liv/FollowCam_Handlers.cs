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

        public static void LogAssemblyName() {
            Log($"Handler Assembly name: {typeof(FollowCam_Handlers).AssemblyQualifiedName}");
        }

    }
}


/*
        public static void EnableFollowCam() {
            VRnyan.FollowCamEnabled = true;
            Log("FollowCam enabled");
        }
        public static void DisableFollowCam() {
            VRnyan.FollowCamEnabled = false;
            Log("FollowCam disabled");
        }

        public static MemoryMappedViewAccessor GetMMF() {
            if (VRnyan.mmfAccess == null) {
                if (IsWine() && (Settings.LinuxRootDriveLetter >= 'a') && (Settings.LinuxRootDriveLetter <= 'z')) {
                    Log("Initialise MMF - Wine/Linux shared memory");
                    VRnyan.mmfAccess = MMF_Wine.InitialiseMMF();
                } else {
                    Log("Initialise MMF - Windows shared memory");
                    VRnyan.mmfAccess = MMF_Windows.InitialiseMMF();
                }
            }
            return VRnyan.mmfAccess;
        }
        private static System.Reflection.MethodInfo _GetFollowCamTransform;
        private static System.Reflection.MethodInfo _GetFollowCamRot;
        internal static void ConnectFollowCam() {
            Log("Looking for FollowCam");
            var type = Type.GetType("VNyan_FollowCam.FollowCam, VNyan-FollowCam", throwOnError: false);
            if (type != null) {
                Log("Found VNyan followcam, getting methods");
                System.Reflection.MethodInfo GetFollowCamTransform = type.GetMethod("GetFollowCamTransform");
                if (GetFollowCamTransform == null) {
                    Log("Couldn't find position methods");
                } else {
                    FollowCamTransform = (Transform)GetFollowCamTransform?.Invoke(null, new object[] { (int)0 });
                    Log("Got methods, testing...");
                    Log(FollowCamTransform.position.ToString());
                    Log(FollowCamTransform.rotation.eulerAngles.ToString());
                    FollowCamConnected = true;
                }
            } else {
                Log("Did not find followcam assembly");
            }
        }*/