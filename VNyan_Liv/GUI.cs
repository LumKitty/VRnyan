using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VNyanInterface;

namespace VRnyan {
    internal class GUI {
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

        private IVNyanUIWindow wndVRnyan;
        internal IVNyanUILabel lblStatus;
        IVNyanUISlider sldCursedCamera;
        IVNyanUIDropdown drpBoneClip;
        IVNyanUIToggle tgl2D;
        IVNyanUISlider sldBoneClipDist;
        private readonly List<string> strBoneSelectorList = new List<string> { "NONE", "Hips", "Spine", "Chest", "UpperChest", "Head", "LeftHand", "RightHand", "LeftFoot", "RightFoot" };

        internal GUI() {
            wndVRnyan = VNyanInterface.VNyanInterface.VNyanUI.createDraggableWindow(330, 230, true, false);
            IVNyanUIContentPanel pnlVRnyan = wndVRnyan.getContentPanel();
            IVNyanUILabel lblTitle = VNyanInterface.VNyanInterface.VNyanUI.createLabel(pnlVRnyan, $"VRnyan - v{VNyan_Handlers.VersionString} by LumKitty", 20, -5, 300, 20);
            
            lblStatus = VNyanInterface.VNyanInterface.VNyanUI.createLabel(pnlVRnyan, "Status - Unknown", 20, -47, 150, 30);
            IVNyanUIButton btnActivate = VNyanInterface.VNyanInterface.VNyanUI.createButton(pnlVRnyan, "Activate", 160, -40, 70, 30);
            IVNyanUIButton btnDeactivate = VNyanInterface.VNyanInterface.VNyanUI.createButton(pnlVRnyan, "Deactivate", 240, -40, 70, 30);
            
            IVNyanUILabel lblCursedCamera = VNyanInterface.VNyanInterface.VNyanUI.createLabel(pnlVRnyan, "Cursed Camera Delay", 20, -80, 150, 30);
            sldCursedCamera = VNyanInterface.VNyanInterface.VNyanUI.createSlider(pnlVRnyan, 20, -90, 291, 30, 0, 1000, 0, true);
            
            IVNyanUILabel lblBoneClip = VNyanInterface.VNyanInterface.VNyanUI.createLabel(pnlVRnyan, "BoneClip", 20, -137, 60, 30);
            drpBoneClip = VNyanInterface.VNyanInterface.VNyanUI.createDropdown(pnlVRnyan, 90, -130, 130, 30, strBoneSelectorList, 0);
            IVNyanUILabel lbl2D = VNyanInterface.VNyanInterface.VNyanUI.createLabel(pnlVRnyan, "2D", 250, -137, 30, 30);
            tgl2D = VNyanInterface.VNyanInterface.VNyanUI.createToggle(pnlVRnyan, "2D", 270, -136, 30, 30, false);

            IVNyanUILabel lblBoneClipDist = VNyanInterface.VNyanInterface.VNyanUI.createLabel(pnlVRnyan, "Bone Clip Distance Adjust", 20, -170, 150, 30);
            sldBoneClipDist = VNyanInterface.VNyanInterface.VNyanUI.createSlider(pnlVRnyan, 20, -180, 291, 30, -100, 100, 0, true);

            btnActivate.onButtonClicked += btnActivate_Clicked;
            btnDeactivate.onButtonClicked += btnDeactivate_Clicked;
            sldCursedCamera.onSliderValueChanged += sldCursedCamera_Changed;
            drpBoneClip.onDropdownValueChanged += drpBoneClip_Changed;
            tgl2D.onToggleValueChanged += tgl2D_Changed;
            sldBoneClipDist.onSliderValueChanged += sldBoneClipDist_Changed;

            PreFill();
        }

        internal void PreFill() {
            if (VRnyan.IsActive) { lblStatus.setText("Status - Active"); } else { lblStatus.setText("Status - Disabled"); }
            sldCursedCamera.setValue(Settings.CursedCameraDelay);
            drpBoneClip.setSelectedIndex(strBoneSelectorList.IndexOf(Settings.BoneClip.ToString()));
            tgl2D.setIsOn(Settings.BoneClipDistanceAdjust2DOnly);
            sldBoneClipDist.setValue((int)(Settings.BoneClipDistanceAdjust * 100));
        }

        internal void Show() {
            wndVRnyan.setVisible(true);
            PreFill();
        }

        internal void Hide() {
            wndVRnyan.setVisible(false);
        }

        void btnActivate_Clicked()   { VRnyan.SetActive(true); }
        void btnDeactivate_Clicked() { VRnyan.SetActive(false); }

        void sldCursedCamera_Changed() {
            Settings.CursedCameraDelay = (uint)sldCursedCamera.getValue();
        }

        void drpBoneClip_Changed() {
            HumanBodyBones Temp;
            if (drpBoneClip.getSelectedIndex() > 0) {
                if (Enum.TryParse<HumanBodyBones>(strBoneSelectorList[drpBoneClip.getSelectedIndex()], out Temp)) {
                    Settings.BoneClip = Temp;
                }
            } else { 
                Settings.BoneClip = null;
            }
        }

        void sldBoneClipDist_Changed() {
            int Temp = (int)sldBoneClipDist.getValue();
            Settings.BoneClipDistanceAdjust = Temp / 100f;
        }

        void tgl2D_Changed() {
            Settings.BoneClipDistanceAdjust2DOnly = tgl2D.getIsOn();
        }
    }
}
