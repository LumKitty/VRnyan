using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

namespace VRnyan {
    public class VRnyan_GUI : MonoBehaviour {
        internal const string CloseTriggerName = "____bottom_right_gui";
        internal const string CloseTriggerValue = "uk.lum.vrnyan";
        private readonly HumanBodyBones[] BoneSelectorList = { 
            HumanBodyBones.Hips, 
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            HumanBodyBones.UpperChest,
            HumanBodyBones.Head,
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightHand,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightFoot,
        };
        private const int DWidth = 213;
        private const int DHeight = 400;
        private static GameObject objVRnyan_GUI = new GameObject("VRnyan_GUI", typeof(VRnyan_GUI));

        private bool BoneSelector;
        
        private string CursedCameraDelay;
        private float BoneClipDistanceAdjust;
        private bool BoneClipDistanceAdjust2DOnly;
        private string BoneClip;

        internal static void SetActive(bool Active) { objVRnyan_GUI.SetActive(Active); }
        internal static void ToggleActive() { objVRnyan_GUI.SetActive(!objVRnyan_GUI.activeSelf); }
        
        void OnEnable() {
            BoneSelector = false;
            CursedCameraDelay = Settings.CursedCameraDelay.ToString();
            BoneClipDistanceAdjust = Settings.BoneClipDistanceAdjust;
            BoneClipDistanceAdjust2DOnly = Settings.BoneClipDistanceAdjust2DOnly;
            if (Settings.BoneClip == null) {
                BoneClip = "OFF";
            } else {
                BoneClip = Settings.BoneClip.ToString();
            }
            VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger(CloseTriggerName,0,0,0, CloseTriggerValue, "","");
        }

        void OnDisable() {
            Settings.SavePluginSettings();
        }

        void OnGUI() {
            GUILayout.BeginArea(new Rect(Screen.width - DWidth, Screen.height - DHeight, DWidth, DHeight));
            GUILayout.FlexibleSpace(); // Force bottom alignment

            if (BoneSelector) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Select bone to track for clipping");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(" X ")) { BoneSelector = false; }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("OFF")) {
                    Settings.BoneClip = null;
                    BoneClip = "OFF";
                    BoneSelector = false;
                    if ((Settings.VNyanSettings & SharedValues.OATREADCLIPPLANEPOS) != 0) { 
                        Settings.VNyanSettings -= SharedValues.OATREADCLIPPLANEPOS;
                        VRnyan.mmfAccess.Write(SharedValues.MMFPos_Settings, Settings.VNyanSettings);
                    }
                }

                foreach (HumanBodyBones Bone in BoneSelectorList) {
                    if (GUILayout.Button(Bone.ToString())) {
                        Settings.BoneClip = Bone;
                        BoneClip = Bone.ToString();
                        BoneSelector = false;
                        if ((Settings.VNyanSettings & SharedValues.OATREADCLIPPLANEPOS) == 0) { 
                            Settings.VNyanSettings += SharedValues.OATREADCLIPPLANEPOS;
                            VRnyan.mmfAccess.Write(SharedValues.MMFPos_Settings, Settings.VNyanSettings);
                        }
                    }
                }

            } else {

                GUILayout.BeginHorizontal();
                if (VRnyan.IsActive) { GUILayout.Label("VRnyan - Active"); } else { GUILayout.Label("VRnyan - Inactive"); }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(" X ")) { SetActive(false); }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Activate")) { VRnyan.SetActive(true); SetActive(false); }
                if (GUILayout.Button("Deactivate")) { VRnyan.SetActive(false); SetActive(false); }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Cursed Camera Delay");
                CursedCameraDelay = GUILayout.TextField(CursedCameraDelay, 3);
                GUILayout.Label("ms");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("BoneClip");
                if (GUILayout.Button(BoneClip)) { BoneSelector = true; }
                GUILayout.Label($"{BoneClipDistanceAdjust}");
                GUILayout.FlexibleSpace();
                BoneClipDistanceAdjust2DOnly = GUILayout.Toggle(BoneClipDistanceAdjust2DOnly, "2D");
                GUILayout.EndHorizontal();

                BoneClipDistanceAdjust = (float)System.Math.Round(GUILayout.HorizontalSlider(BoneClipDistanceAdjust, -1, 1), 2);
                uint.TryParse(CursedCameraDelay, out Settings.CursedCameraDelay);
                Settings.BoneClipDistanceAdjust = BoneClipDistanceAdjust;
                Settings.BoneClipDistanceAdjust2DOnly = BoneClipDistanceAdjust2DOnly;
            }
            GUILayout.Space(54); // Padding to avoid conflicting with the Hide UI button
            GUILayout.EndArea();
        }
    }
}

