using System.Runtime.Serialization;

namespace EdgeUpdateAPI.Responses
{
    public class LatestResponse
    {
        [DataMember(Name = "ContentId", IsRequired = false)]
        public ContentId ContentId { get; set; }

        [DataMember(Name = "Files", IsRequired = false)]
        public string[] Files { get; set; }
    }

    public class ContentId
    {
        [DataMember(Name = "Namespace", IsRequired = false)]
        public string Namespace { get; set; }

        [DataMember(Name = "Name", IsRequired = false)]
        public string Name { get; set; }

        [DataMember(Name = "Version", IsRequired = false)]
        public string Version { get; set; }
    }
}
