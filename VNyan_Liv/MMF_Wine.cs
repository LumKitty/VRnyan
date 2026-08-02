using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using static VRnyan.Functions;

namespace VRnyan {
    internal class MMF_Wine {
        private static MemoryMappedFile mmf = null;
        internal static MemoryMappedViewAccessor InitialiseMMF() {
            if (mmf == null) {
                string PathName = $"{VRnyan.LinuxRootDriveLetter}:\\dev\\shm\\{SharedValues.MMFname}";

                if (!File.Exists(PathName)) {
                    Log($"Creating file: {PathName}");
                    using (var f = File.Create(PathName)) {
                        byte[] b = new byte[SharedValues.MMFSize];
                        f.Write(b);
                    }
                } else {
                    Log($"Found existing file: {PathName}");
                }

                mmf = MemoryMappedFile.CreateFromFile(PathName, System.IO.FileMode.Open, SharedValues.MMFname, SharedValues.MMFSize, MemoryMappedFileAccess.ReadWrite);
            }
            Log("Creating accessor");
            return mmf.CreateViewAccessor(0, SharedValues.MMFSize);
        }
    }
}