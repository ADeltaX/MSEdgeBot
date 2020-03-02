using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeUpdateAPI.Classes
{
    public class EdgeFiles
    {
        public string Version { get; set; }
        public EdgeFile[] EdgeFile { get; set; }
    }

    public class EdgeFile
    {
        /// <summary>
        /// Gets the SHA1 hash in byte array
        /// </summary>
        public byte[] Sha1 { get; set; }

        /// <summary>
        /// Gets the SHA256 hash in byte array
        /// </summary>
        public byte[] Sha256 { get; set; }

        /// <summary>
        /// Gets the download url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets the architecture of the file
        /// </summary>
        public Arch Arch { get; set; }

        /// <summary>
        /// Gets the file size in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets the filename
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets the update type
        /// </summary>
        public EdgeFileUpdateType EdgeFileUpdateType { get; set; }

        /// <summary>
        /// Gets the version of the delta update if EdgeFileUpdateType == EdgeFileUpdateType.Delta, otherwise null
        /// </summary>
        public string DeltaVersion { get; set; }
    }

    public enum EdgeFileUpdateType
    {
        Delta,
        Full
    }

    public enum Arch
    {
        X86,
        X64,
        ARM64,
        ARM
    }
}
