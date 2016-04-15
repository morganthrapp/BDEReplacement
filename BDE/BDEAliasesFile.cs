using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BDEReplacement
{
    class BDEAliasesFile
    {
        private class Record
        {
            private static readonly byte[] head = new byte[] { 3, 0, 0, 0 };            
            private static readonly byte[] dbInfo = new byte[] { 0, 0, 0, 68, 66, 32, 73, 78, 70, 79, 0, 1, 0, 3, 0, 1, 0, 84, 89, 80, 69, 0,
                                                                 4, 0, 83, 84, 65, 78, 68, 65, 82, 68, 0, 3, 0, 1, 0, 80, 65, 84, 72, 0, 4, 0 };          
            private static readonly byte[] BCDHeader = new byte[] { 0, 69, 78, 65, 66, 76, 69, 32, 66, 67, 68, 0, 4, 0 };            
            private static readonly byte[] recordFooter = new byte[] { 0, 3, 0, 1, 0, 68, 69, 70, 65, 85, 76, 84, 32, 68, 82, 73, 86, 69, 82,
                                                                       0, 4, 0, 68, 66, 65, 83, 69, 0, 2, 0, 2, 0, 2, 0 };
            private string name;
            private string path;
            private bool enableBCD;
            internal byte[] record
            {
                get { return BuildRecord(); }
            }
            public Record(string name, string path, bool enableBCD)
            {
                this.name = name;
                this.path = path;
                this.enableBCD = enableBCD;
            }      
            private byte[] BuildRecord()
            {
                List<byte> _record = new List<byte>();
                _record.AddRange(head);
                _record.AddRange(Encoding.UTF8.GetBytes(name));
                _record.AddRange(nameFooter);
                _record.AddRange(dbInfo);
                _record.AddRange(Encoding.UTF8.GetBytes(path));
                _record.AddRange(pathFooter);
                _record.AddRange(BCDHeader);
                _record.AddRange(enableBCD ? BCDTrue : BCDFalse);
                _record.AddRange(recordFooter);
                return _record.ToArray();
            }
              
            
        }

        public class Alias
        {
            public string name;
            public string path;
            public bool enableBCD;
            internal int start;
            internal Alias(string name="", string path="", bool enableBCD= false)
            {
                this.name = name;
                this.path = path;
                this.enableBCD = enableBCD;
            }
        }

        private string configFilePath;
        private const int headerLength = 1015;
        private static readonly byte[] BCDTrue = new byte[] { 84, 82, 85, 69 };
        private static readonly byte[] BCDFalse = new byte[] { 70, 65, 76, 83, 69 };
        private static readonly byte[] pathFooter = new byte[] { 0, 3, 0, 1 };
        private static readonly byte[] nameFooter = new byte[] { 0, 1, 0, 3 };

        private List<T> GetEnd<T>(List<T> list, int count)
        {
            return list.GetRange(Math.Max(0, list.Count - count), list.Count >= count ? count : list.Count);
        }

        private int GetSliceIndex<T>(List<T> haystack, List<T> needle)
        {
            int startIndex = -1;
            for (int i=0;i<haystack.Count - needle.Count;i++)
            {
                if (haystack.GetRange(i, needle.Count).SequenceEqual(needle))
                {
                    return i;
                }
            }
            return startIndex;
        }

        public BDEAliasesFile(string configFilePath)
        {
            this.configFilePath = configFilePath;
        }       
        
        public void CreateNewAlias(string name, string path, bool EnableBCD)
        {
            Record rec = new Record(name, path, EnableBCD);
            using (BinaryWriter writer = new BinaryWriter(File.Open(this.configFilePath, FileMode.Open, FileAccess.ReadWrite)))
            {
                writer.Seek(-2, SeekOrigin.End);
                writer.Write(rec.record);
            }
        }

        public List<Alias> GetAliases()
        {
            List<Alias> aliases = new List<Alias>();
            using (FileStream reader = new FileStream(this.configFilePath, FileMode.Open))
            {
                reader.Seek(headerLength, SeekOrigin.Begin);
                while (reader.Position < reader.Length)
                {
                    Alias alias = new Alias();
                    alias.start = (int)reader.Position;
                    List <byte> name = new List<byte>();
                    var lastRecord = aliases.LastOrDefault() ?? new Alias("", "", false);
                    reader.Seek(2 - (lastRecord.enableBCD ? 1 : 0 ), SeekOrigin.Current);
                    while (!GetEnd(name, 4).SequenceEqual(nameFooter))
                    {
                        name.Add((byte)reader.ReadByte());
                    }
                    alias.name = Encoding.Default.GetString(name.Take(name.Count - 4).ToArray());
                    reader.Seek(44, SeekOrigin.Current);
                    List<byte> path = new List<byte>();
                    while (!GetEnd(path, 4).SequenceEqual(pathFooter))
                    {
                        path.Add((byte)reader.ReadByte());
                    }
                    alias.path = Encoding.Default.GetString(path.Take(path.Count - 4).ToArray());
                    reader.Seek(14, SeekOrigin.Current);
                    byte[] enableBDC = new byte[5];
                    reader.Read(enableBDC, 0, 5);
                    alias.enableBCD = !enableBDC.SequenceEqual(BCDFalse);
                    aliases.Add(alias);
                    reader.Seek(34, SeekOrigin.Current);
                }
            }
            return aliases;
        }

        public void UpdateAlias(string oldName, string oldPath, string newName, string newPath, bool enableBCD)
        {
            var aliases = GetAliases();
            int start = 0;
            foreach (var alias in aliases)
            {
                if (oldName == alias.name)
                {
                    start = alias.start;
                }
            }
            using (FileStream reader = new FileStream(this.configFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                byte[] tempFile = new byte[reader.Length];
                reader.Read(tempFile, 0, (int)reader.Length);
                List<byte> file = new List<byte>();
                file.AddRange(tempFile);
                int nameIndex = GetSliceIndex(file, Encoding.Default.GetBytes(oldName).ToList());
                file.RemoveRange(nameIndex, oldName.Length);
                file.InsertRange(nameIndex, Encoding.Default.GetBytes(newName).ToList());
                int pathIndex = GetSliceIndex(file, Encoding.Default.GetBytes(oldPath).ToList());
                file.RemoveRange(pathIndex, oldPath.Length);
                file.InsertRange(pathIndex, Encoding.Default.GetBytes(newPath).ToList());
                int BCDIndex = pathIndex + 18;
                bool BCDEnabled = file.GetRange(BCDIndex, 4).SequenceEqual(BCDTrue);
                if (BCDEnabled != enableBCD)
                {
                    file.RemoveRange(BCDIndex, enableBCD ? 5 : 4);
                    file.InsertRange(BCDIndex, enableBCD ? BCDTrue : BCDFalse);
                }
                reader.Seek(0, SeekOrigin.Begin);
                reader.Write(file.ToArray(), 0, file.Count);
            }
        }
    }
}
