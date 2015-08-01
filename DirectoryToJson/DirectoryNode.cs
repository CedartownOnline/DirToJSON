using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DirectoryToJson
{
    [DataContract]
    public class DirectoryNode
    {
        public DirectoryNode(string DirectoryName, string BasePath)
        {
            this.DirectoryName = DirectoryName;
            this.BasePath = BasePath;
            Children = new List<DirectoryNode>();
            Files = new List<FileNode>();
        }

        [DataMember(Name = "name", IsRequired = true)]
        public string DirectoryName { get; set; }

        [DataMember(Name = "path", IsRequired = true)]
        public string BasePath { get; set; }

        [DataMember(Name = "files", IsRequired = false)]
        public List<FileNode> Files;

        [DataMember(Name = "directories", IsRequired = false)]
        public List<DirectoryNode> Children;

    }
}