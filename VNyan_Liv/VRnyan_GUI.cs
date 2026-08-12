using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VRnyan {
    public class VRnyan_GUI : MonoBehaviour {
        private const int DWidth = 213;
        private const int DHeight = 400;
        public Camera curCam;
        private static GameObject objVRnyan_GUI = new GameObject("VRnyan_GUI", typeof(VRnyan_GUI));

        private string CursedCameraDelay;
        private float BoneClipDistanceAdjust;

        internal static void SetActive(bool Active) { objVRnyan_GUI.SetActive(Active); }
        internal static void ToggleActive() { objVRnyan_GUI.SetActive(!objVRnyan_GUI.activeSelf); }
        
        void OnEnable() {
            CursedCameraDelay = Settings.CursedCameraDelay.ToString();
            BoneClipDistanceAdjust = Settings.BoneClipDistanceAdjust;
            VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger("____bottom_right_gui",0,0,0,"vrnyan","","");
        }

        void OnDisable() {
            Settings.SavePluginSettings();
        }

        void OnGUI() {
            GUILayout.BeginArea(new Rect(Screen.width - DWidth, Screen.height - DHeight, DWidth, DHeight));
            GUILayout.FlexibleSpace();
            if (VRnyan.IsActive) { GUILayout.Label("VRnyan - Active"); } else { GUILayout.Label("VRnyan - Inactive");  }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Activate")) { VRnyan.SetActive(true); }
            if (GUILayout.Button("Deactivate")) { VRnyan.SetActive(false); }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Cursed Camera Delay");
            CursedCameraDelay = GUILayout.TextField(CursedCameraDelay, 3);
            uint.TryParse(CursedCameraDelay, out Settings.CursedCameraDelay);
            GUILayout.EndHorizontal();
            
            GUILayout.Label($"BoneClip {BoneClipDistanceAdjust}");
            BoneClipDistanceAdjust = (float)System.Math.Round(GUILayout.HorizontalSlider(BoneClipDistanceAdjust, -1, 1),2);
            Settings.BoneClipDistanceAdjust = BoneClipDistanceAdjust;
            GUILayout.Space(50);
            GUILayout.EndArea();
        }
    }
}

