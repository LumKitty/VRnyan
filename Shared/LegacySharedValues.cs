using System;
using System.Security.Cryptography.X509Certificates;

namespace Shared {
    public class LegacySharedValues {
        public const string PluginName = "VRnyan";
        public const string Author = "LumKitty";
        public const string Website = "https://lum.uk/";
        //public const string Version = "v1.3a2";
        public const string ProtocolVersion = "v1.1";
        public const string MMFname = "uk.lum.livnyan.cameradata." + ProtocolVersion;
        public const int MMFSize = 9 * sizeof(float);

        public const int CAMENABLED = 0x1;
        public const int LOGENABLED = 0x2;
        public const int LOGSPAMENABLED = 0x4;

        public const long MMFPos_CamPosX = 0;
        public const long MMFPos_CamPosY = 1 * sizeof(float);
        public const long MMFPos_CamPosZ = 2 * sizeof(float);
        public const long MMFPos_CamRotW = 3 * sizeof(float);
        public const long MMFPos_CamRotX = 4 * sizeof(float);
        public const long MMFPos_CamRotY = 5 * sizeof(float);
        public const long MMFPos_CamRotZ = 6 * sizeof(float);
        public const long MMFPos_CamFOV = 7 * sizeof(float);
        public const long MMFPos_Settings = 8 * sizeof(float);
    }
}
