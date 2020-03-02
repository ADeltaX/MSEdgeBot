using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EdgeUpdateAPI.Responses
{
    public class FilesResponse
    {
        [DataMember(Name = "FileId", IsRequired = false)]
        public string FileId { get; set; }

        [DataMember(Name = "SizeInBytes", IsRequired = false)]
        public long SizeInBytes { get; set; }

        [DataMember(Name = "TimeLimitedUrl", IsRequired = false)]
        public bool? TimeLimitedUrl { get; set; }

        [DataMember(Name = "Url", IsRequired = false)]
        public string Url { get; set; }

        [DataMember(Name = "Hashes", IsRequired = false)]
        public Hashes Hashes { get; set; }

        [DataMember(Name = "DeliveryOptimization", IsRequired = false)]
        public DeliveryOptimization DeliveryOptimization { get; set; }
    }

    public class DeliveryOptimization
    {
        [DataMember(Name = "PiecesHash", IsRequired = false)]
        public PiecesHash PiecesHash { get; set; }
    }

    public class PiecesHash
    {
        [DataMember(Name = "Url")]
        public object Url { get; set; }

        [DataMember(Name = "SizeInBytes", IsRequired = false)]
        public long? SizeInBytes { get; set; }

        [DataMember(Name = "Hashes")]
        public object Hashes { get; set; }

        [DataMember(Name = "HashOfHashes", IsRequired = false)]
        public string HashOfHashes { get; set; }
    }

    public class Hashes
    {
        [DataMember(Name = "Sha1", IsRequired = false)]
        public string Sha1 { get; set; }

        [DataMember(Name = "Sha256", IsRequired = false)]
        public string Sha256 { get; set; }
    }
}
