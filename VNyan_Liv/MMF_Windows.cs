using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;
using static VRnyan.Functions;

namespace VRnyan {
    internal static class MMF_Windows {
        private static MemoryMappedFile mmf = null;
        internal static MemoryMappedViewAccessor InitialiseMMF() {
            if (mmf == null) {
                Log("Creating file");
                mmf = MemoryMappedFile.CreateOrOpen(SharedValues.MMFname, SharedValues.MMFSize);
            }
            Log("Creating accessor");
            return mmf.CreateViewAccessor(0, SharedValues.MMFSize);
        }
    }
}
