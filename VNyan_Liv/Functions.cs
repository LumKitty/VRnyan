using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static VRnyan.Settings;

namespace VRnyan {
    internal static class Functions {
        [DllImport("ntdll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr wine_get_version();

        internal static void Log(string message) {
            if ((VNyanSettings & SharedValues.LOGENABLED) != 0) {
                UnityEngine.Debug.Log("[VRnyan] " + message);
            }
        }
        internal static void ErrorHandler(Exception e) {
            VNyanInterface.VNyanInterface.VNyanParameter.setVNyanParameterString("_lum_liv_err", e.ToString());
            UnityEngine.Debug.Log("[VRnyan] ERR:" + e.ToString());
        }

        internal static bool IsWine() {
            try {
                wine_get_version();
                return true;
            } catch (EntryPointNotFoundException) {
                return false;
            //} catch (DllNotFoundException) {
            //    return Platform.Unix;
            }
        }
    }
}
