using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;


namespace DirectoryToJson
{
    public class Directories
    {
        private readonly string _directoryPath;
        private DirectoryNode _directories;
        public static Dictionary<string, string> Filters = new Dictionary<string, string>();
        private static bool _useFilter = false;
        public static StringBuilder Log;
        private bool _suppressEmptyDirectories;
        private bool _writeChildren;


        public Directories(String DirectoryPath)
        {
            _directoryPath = DirectoryPath;
            Initialize(DirectoryPath, "*.*", false, false);

        }


        public Directories(String DirectoryPath, string AddFilters)
        {
            _directoryPath = DirectoryPath;
            Initialize(DirectoryPath, AddFilters, false, false);
        }
        public Directories(String DirectoryPath, string AddFilters, bool SuppressEmptyDirectories)
        {
            _directoryPath = DirectoryPath;
            Initialize(DirectoryPath, AddFilters, SuppressEmptyDirectories,false);
        }

        public Directories(String DirectoryPath, string AddFilters, bool SuppressEmptyDirectories, bool WriteChildren)
        {
            _directoryPath = DirectoryPath;
            Initialize(DirectoryPath, AddFilters, SuppressEmptyDirectories, WriteChildren);
        }

        private void Initialize(String DirectoryPath, string AddFilters, bool SuppressEmptyDirectories,bool  WriteChildren)
        {
            _suppressEmptyDirectories = SuppressEmptyDirectories;
            _writeChildren = WriteChildren;

            if (DirectoryPath == null) throw new ArgumentNullException("DirectoryPath");
            Log = new StringBuilder("Start of Build for " + DirectoryPath);
            var filterList = AddFilters.Split(';');
            foreach (var parts in filterList.Select(FilterSection => FilterSection.Split('|')).Where(Parts => Parts.Length == 2))
            {
                try
                {
                    var name = parts[0].ToLower();
                    if (!name.StartsWith(".")) name = "." + name;
                    var type = parts[1];
                    Filters.Add(name, type.ToLower());
                }
                catch (Exception ex)
                {
                    Log.AppendLine(ex.Message);
                }
            }

            _useFilter = (Filters.Count > 0);


            ProcessPath();
        }

        public string ProcessPath()
        {

            DirectoryInfo di = new DirectoryInfo(_directoryPath);
            _directories = new DirectoryNode(di.Name, "");
            WalkDirectoryTree(_directories, di);
            _directories.BasePath = "\t";

            if (_writeChildren)
            {
                foreach (DirectoryNode subFolder in _directories.Children)
                {
                    subFolder.Children = null;
                    subFolder.Files = null;
                }

            }
            File.WriteAllText(Path.Combine(_directoryPath, "directory.json"), GetJson());

            return (GetJson());
        }

        public  string GetJson()
        {
            return (GetJson(_directories));
        }

        public static string GetJson(DirectoryNode WorkDirectory)
        {
            return (Serialize(WorkDirectory));
        }

        public string GetJsonPretty()
        {
            return (FormatOutput(GetJson()));
        }


        public static string Serialize<T>(T Obj)
        {
            string returnVal;
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(Obj.GetType());
                using (MemoryStream ms = new MemoryStream())
                {

                    serializer.WriteObject(ms, Obj);

                  returnVal = Encoding.Default.GetString(ms.ToArray());
               

                }
            }
            catch (Exception ex)
            {
                returnVal = "";
                Log.AppendLine(ex.Message);
            }

            return returnVal;
        }

        private void WalkDirectoryTree(DirectoryNode DirNode, DirectoryInfo Root)
        {
            List<DirectoryInfo> subDirs = new List<DirectoryInfo>();

            // First, process all the files directly under this folder 

            FileInfo[] files;
            try
            {
                files = Root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater 
            // than the application provides. 
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse. 
                // You may decide to do something different here. For example, you 
                // can try to elevate your privileges and access the file again.
                Log.AppendLine(e.Message);
                return;
            }

            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return;
            }


            foreach (FileInfo fi in files)
            {

                if (fi.Name == "directory.json") continue;

                var cType = fi.Extension.StartsWith(".") ? fi.Extension.Substring(1).ToUpper() : fi.Extension.ToUpper();

                if (_useFilter)
                {
                    if (!Filters.ContainsKey(fi.Extension.ToLower())) continue;
                    cType = Filters[fi.Extension.ToLower()];
                }

                DirNode.Files.Add(new FileNode
                {
                    Name = fi.Name,
                    Date = fi.CreationTime.ToShortDateString(),
                    Time = fi.CreationTime.ToShortTimeString(),
                    Extension = fi.Extension.ToLower(),
                    FileSize = fi.Length.ToString(),
                    Type = cType
                }
                );
            }

            // Now find all the subdirectories under this directory.
            subDirs.AddRange(Root.GetDirectories());
            foreach (DirectoryInfo dirInfo in subDirs)
            {
                if (DirNode == null) continue;
                DirectoryNode d = new DirectoryNode(dirInfo.Name, DirNode.BasePath + "/" + dirInfo.Name);
                WalkDirectoryTree(d, dirInfo);

                if (_suppressEmptyDirectories && d.Files.Count + d.Children.Count == 0) continue;
                DirNode.Children.Add(d);
                string wkPath = Path.Combine(dirInfo.FullName, "directory.json");
                if (File.Exists(wkPath)) File.Delete(wkPath);
                if (_writeChildren) File.WriteAllText(wkPath, GetJson(d));
            }

        }

        private static string FormatOutput(string JSONString)
        {
            var stringBuilder = new StringBuilder();

            bool escaping = false;
            bool inQuotes = false;
            int indentation = 0;

            foreach (char character in JSONString)
            {
                if (escaping)
                {
                    escaping = false;
                    stringBuilder.Append(character);
                }
                else
                {
                    switch (character)
                    {
                        case '\\':
                            escaping = true;
                            stringBuilder.Append(character);
                            break;
                        case '\"':
                            inQuotes = !inQuotes;
                            stringBuilder.Append(character);
                            break;
                        default:
                            if (!inQuotes)
                            {
                                switch (character)
                                {
                                    case ',':
                                        stringBuilder.Append(character);
                                        stringBuilder.Append("\r\n");
                                        stringBuilder.Append('\t', indentation);
                                        break;
                                    case '{':
                                    case '[':
                                        stringBuilder.Append(character);
                                        stringBuilder.Append("\r\n");
                                        stringBuilder.Append('\t', ++indentation);
                                        break;
                                    case '}':
                                    case ']':
                                        stringBuilder.Append("\r\n");
                                        stringBuilder.Append('\t', --indentation);
                                        stringBuilder.Append(character);
                                        break;
                                    case ':':
                                        stringBuilder.Append(character);
                                        stringBuilder.Append('\t');
                                        break;
                                    default:
                                        stringBuilder.Append(character);
                                        break;
                                }
                            }
                            else
                            {
                                stringBuilder.Append(character);
                            }
                            break;
                    }
                }
            }

            return stringBuilder.ToString();
        }
    }
}
