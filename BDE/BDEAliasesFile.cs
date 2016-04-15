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
            private static readonly byte[] head = new byte[] { 0x03, 0x00, 0x00, 0x00 };
            private static readonly byte[] rfoot = new byte[] { 0x00, 0x01, 0x00, 0x03 };
            private static readonly byte[] dbInfo = new byte[] { 0x00, 0x00, 0x00, 0x44, 0x42, 0x20, 0x49, 0x4E, 0x46, 0x4F, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x54, 0x59, 0x50, 0x45, 0x00,
                                                                 0x04, 0x00, 0x53, 0x54, 0x51, 0x4E, 0x44, 0x41, 0x52, 0x44, 0x00, 0x03, 0x00, 0x01, 0x00, 0x50, 0x41, 0x54, 0x48, 0x00, 0x04, 0x00 };
            private static readonly byte[] BCDHeader = new byte[] { 0x00, 0x45, 0x4E, 0x41, 0x42, 0x4C, 0x45, 0x20, 0x42, 0x43, 0x44, 0x00, 0x04, 0x00 };
            
            
            private static readonly byte[] foot = new byte[] { 0x00, 0x03, 0x00, 0x01, 0x00, 0x44, 0x45, 0x46, 0x41, 0x55, 0x4C, 0x54, 0x20, 0x44, 0x52, 0x49, 0x56, 0x45, 0x52, 0x00, 0x04, 0x00,
                                                               0x44, 0x42, 0x41, 0x53, 0x45, 0x00, 0x02, 0x00, 0x02, 0x00 };
            private string name;
            private string path;
            private bool enableBCD;
            public string record
            {
                get { return BuildRecord(); }
            }
            public Record(string name, string path, bool enableBCD)
            {
                this.name = name;
                this.path = path;
                this.enableBCD = enableBCD;
            }      
            private string BuildRecord()
            {
                List<byte> _record = new List<byte>();
                _record.AddRange(head);
                _record.AddRange(Encoding.UTF8.GetBytes(name));
                _record.AddRange(rfoot);
                _record.AddRange(dbInfo);
                _record.AddRange(Encoding.UTF8.GetBytes(path));
                _record.AddRange(rfoot);
                _record.AddRange(BCDHeader);
                _record.AddRange(enableBCD ? BCDTrue : BCDFalse);
                _record.AddRange(foot);
                return _record.ToString();
            }
              
            
        }

        public class Alias
        {
            public string name;
            public string path;
            public bool enableBCD;
            internal int start;
        }

        private string configFilePath;
        private const int headerLength = 1015;
        private static readonly byte[] BCDTrue = new byte[] { 0x54, 0x52, 0x55, 0x45 };
        private static readonly byte[] BCDFalse = new byte[] { 0x46, 0x41, 0x4c, 0x53, 0x45 };

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
        
        public void CreateNewAlias(string name, string path, bool EnableBCD = false)
        {
            Record rec = new Record(name, path, EnableBCD);
            using (BinaryWriter writer = new BinaryWriter(File.Open(this.configFilePath, FileMode.Append)))
            {
                writer.Seek(4, SeekOrigin.End);
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
                    reader.Seek(2, SeekOrigin.Current);
                    while (!GetEnd(name, 4).SequenceEqual(new List<byte>() { 0, 1, 0, 3 }))
                    {
                        name.Add((byte)reader.ReadByte());
                    }
                    alias.name = Encoding.Default.GetString(name.Take(name.Count - 4).ToArray());
                    reader.Seek(44, SeekOrigin.Current);
                    List<byte> path = new List<byte>();
                    while (!GetEnd(path, 4).SequenceEqual(new List<byte>() { 0, 3, 0, 1 }))
                    {
                        path.Add((byte)reader.ReadByte());
                    }
                    alias.path = Encoding.Default.GetString(path.Take(path.Count - 4).ToArray());
                    reader.Seek(13, SeekOrigin.Current);
                    byte[] enableBDC = new byte[5];
                    reader.Read(enableBDC, 0, 5);
                    alias.enableBCD = enableBDC.SequenceEqual(BCDTrue);
                    aliases.Add(alias);
                    reader.Seek(35, SeekOrigin.Current);
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
                bool BCDEnabled = file.GetRange(BCDIndex, 5).SequenceEqual(new byte[5] { 0x54, 0x52, 0x55, 0x45, 0x00 });
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
