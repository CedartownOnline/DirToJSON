
using System.Runtime.Serialization;

namespace DirectoryToJson
{
    [DataContract]
    public class FileNode
    {

        private string _extension;

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        [DataMember(Name = "date", IsRequired = true)]
        public string Date { get; set; }

        [DataMember(Name = "time", IsRequired = true)]
        public string Time { get; set; }

        [DataMember(Name = "size", IsRequired = true)]
        public string FileSize { get; set; }

        [DataMember(Name = "ext", IsRequired = true)]
        public string Extension
        {
            get { return _extension; }
            set { _extension = value.TrimStart('.'); }
        }

        [DataMember(Name = "type", IsRequired = true)]
        public string Type { get; set; }
    }
}