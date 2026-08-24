using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static VRnyan.Functions;

namespace VRnyan {
    internal static class Settings {
        internal static int VNyanSettings = 2;
        internal static uint CursedCameraDelay = 0;
        internal static HumanBodyBones? BoneClip = HumanBodyBones.Hips;   //_lum_liv_BoneClip
        internal static float BoneClipDistanceAdjust = 0;                 //_lum_liv_BoneClipDistanceAdjust
        internal static bool BoneClipDistanceAdjust2DOnly = true;
        internal static char LinuxRootDriveLetter;
        internal static bool ActiveOnStart = false;

        private static readonly string OldSettingsFileName = VNyanInterface.VNyanInterface.VNyanSettings.getProfilePath() + "\\LIVnyan.cfg";
        private static readonly string SettingsFileName = VNyanInterface.VNyanInterface.VNyanSettings.getProfilePath() + "\\VRnyan.cfg";

        internal static void LoadPluginSettings() {
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
                            ActiveOnStart = true;
                            tempVNyanSettings += SharedValues.CAMENABLED;
                            Log("Camera sync enabled on startup");
                        } else {
                            ActiveOnStart = false;
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
                        if (String.IsNullOrEmpty(tempSetting)) {
                            BoneClip = null;
                            Log("Clipping bone tracker disabled");
                        } else if (Enum.TryParse<HumanBodyBones>(tempSetting, out TempBoneClip)) {
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
                    if (settings.TryGetValue("LinuxRootDriveLetter", out tempSetting)) {
                        if (char.TryParse(tempSetting.ToLower(), out LinuxRootDriveLetter)) {
                            Log("Linux root drive letter: " + LinuxRootDriveLetter);
                        } else {
                            Log("Linux root drive letter setting invalid, defaulting to Z");
                            LinuxRootDriveLetter = 'z';
                            SettingMissing = true;
                        }
                    } else {
                        Log("Linux root drive letter setting missing, defaulting to Z");
                        LinuxRootDriveLetter = 'z';
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

        internal static void SavePluginSettings() {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["ActiveOnStart"] = ActiveOnStart.ToString();
            settings["LogEnabled"] = ((VNyanSettings & SharedValues.LOGENABLED) != 0).ToString();
            settings["LogSpam"] = false.ToString();
            settings["CursedCamera"] = CursedCameraDelay.ToString();
            settings["BoneClip"] = BoneClip.ToString();
            settings["BoneClipDistanceAdjust"] = BoneClipDistanceAdjust.ToString("0.00");
            settings["BoneClipDistanceAdjust2DOnly"] = BoneClipDistanceAdjust2DOnly.ToString();
            settings["LinuxRootDriveLetter"] = LinuxRootDriveLetter.ToString().Trim();
            VNyanInterface.VNyanInterface.VNyanSettings.saveSettings(SettingsFileName, settings);
        }
    }
}