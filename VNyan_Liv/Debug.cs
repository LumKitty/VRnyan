using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using static VRnyan.Functions;

namespace VRnyan {
    internal static class Debug {
        internal static bool Enabled = true;
        internal static System.Timers.Timer DebugTimer = new System.Timers.Timer();

        internal static void StartDebug() {
            Log($"Assembly name: {typeof(VRnyan).AssemblyQualifiedName}");
            DebugTimer.Interval = 1000;
            DebugTimer.AutoReset = true;
            DebugTimer.Elapsed += DebugLog;
            DebugTimer.Start();
            Log("Debugging started");
        }

        internal static void DebugLog(Object source, ElapsedEventArgs e) {
            Log($"VRNyanControllingCamera: {FollowCam_Handlers.VRNyanControllingCamera} - MainFollowCamActive: {FollowCam_Handlers.MainFollowCamActive} - Queue Length: {VRnyan.CursedCamera.Count}");
        }
    }
}
