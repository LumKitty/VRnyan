using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VRnyan {
    internal class SharedValues {
        public const string PluginName       = "VRnyan";
        public const string Author           = Shared.LegacySharedValues.Author;
        public const string Website          = Shared.LegacySharedValues.Website;
        public const string ProtocolVersion  = Shared.LegacySharedValues.ProtocolVersion;
        public const string MMFname          = Shared.LegacySharedValues.MMFname;
        
        public const int MMFSize = (sizeof(float) * 12) + 2 * sizeof(int);

        public const int CAMENABLED          = Shared.LegacySharedValues.CAMENABLED; // 0x1
        public const int LOGENABLED          = Shared.LegacySharedValues.LOGENABLED; // 0x2
        public const int LOGSPAMENABLED      = Shared.LegacySharedValues.LOGSPAMENABLED; // 0x3
        public const int OATREADCLIPPLANEPOS = 0x8;

        public const long MMFPos_CamPosX  = Shared.LegacySharedValues.MMFPos_CamPosX; //0;
        public const long MMFPos_CamPosY  = Shared.LegacySharedValues.MMFPos_CamPosY; //1 * sizeof(float);
        public const long MMFPos_CamPosZ  = Shared.LegacySharedValues.MMFPos_CamPosZ; //2 * sizeof(float);
        public const long MMFPos_CamRotW  = Shared.LegacySharedValues.MMFPos_CamRotW; //3 * sizeof(float);
        public const long MMFPos_CamRotX  = Shared.LegacySharedValues.MMFPos_CamRotX; //4 * sizeof(float);
        public const long MMFPos_CamRotY  = Shared.LegacySharedValues.MMFPos_CamRotY; //5 * sizeof(float);
        public const long MMFPos_CamRotZ  = Shared.LegacySharedValues.MMFPos_CamRotZ; //6 * sizeof(float);
        public const long MMFPos_CamFOV   = Shared.LegacySharedValues.MMFPos_CamFOV;  //7 * sizeof(float);
        public const long MMFPos_Settings = Shared.LegacySharedValues.MMFPos_Settings;//8 * sizeof(float);
        // Only used by OnAirTap. Ignored by LIV camera plugin
        public const long MMFPos_ResX     = 8 * sizeof(float) + 1 * sizeof(int);
        public const long MMFPos_ResY     = 8 * sizeof(float) + 2 * sizeof(int);
        public const long MMFPos_ClipPosX = 9 * sizeof(float) + 2 * sizeof(int);
        public const long MMFPos_ClipPosY = 10 * sizeof(float) + 2 * sizeof(int);
        public const long MMFPos_ClipPosZ = 11 * sizeof(float) + 2 * sizeof(int);
    }
}
