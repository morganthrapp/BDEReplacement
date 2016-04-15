using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BDEReplacement
{
    class BDEConfigFile
    {
        private class Record
        {
            private static readonly byte[] head = new byte[] { 0x03, 0x00, 0x00, 0x00 };
            private static readonly byte[] rfoot = new byte[] { 0x00, 0x01, 0x00, 0x03 };
            private static readonly byte[] dbInfo = new byte[] { 0x00, 0x00, 0x00, 0x44, 0x42, 0x20, 0x49, 0x4E, 0x46, 0x4F, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x54, 0x59, 0x50, 0x45, 0x00,
                                                                 0x04, 0x00, 0x53, 0x54, 0x51, 0x4E, 0x44, 0x41, 0x52, 0x44, 0x00, 0x03, 0x00, 0x01, 0x00, 0x50, 0x41, 0x54, 0x48, 0x00, 0x04, 0x00 };
            private static readonly byte[] BCDHeader = new byte[] { 0x00, 0x45, 0x4E, 0x41, 0x42, 0x4C, 0x45, 0x20, 0x42, 0x43, 0x44, 0x00, 0x04, 0x00 };
            private static readonly byte[] BCDTrue = new byte[] { 0x54, 0x52, 0x55, 0x45 };
            private static readonly byte[] BCDFalse = new byte[] { 0x46, 0x41, 0x4c, 0x53, 0x45 };
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
        }

        private string configFilePath;
        private const int headerLength = 2281;
        private static readonly byte[] INTRABASE = new byte[] { 0x49, 0x4E, 0x54, 0x52, 0x42, 0x41, 0x53, 0x45 };
    
        public BDEConfigFile(string configFilePath)
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
        private List<byte> GetEnd(List<byte> list, int bytes)
        {
            return list.GetRange(Math.Max(0, list.Count - bytes), list.Count >= bytes ? bytes : list.Count);
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
                    List <byte> name = new List<byte>();
                    long recPosition = reader.Position;
                    reader.Seek(4, SeekOrigin.Current);
                    while (GetEnd(name, 4) != new List<byte>() { 0x01, 0x00, 0x03, 0x00 })
                    {
                        name.Add((byte)reader.ReadByte());
                    }
                    alias.name = name.Take(name.Count - 4).ToString();
                    List<byte> path = new List<byte>();
                    while (GetEnd(path, 5) != new List<byte>() { 0x00, 0x03, 0x00, 0x01, 0x00 })
                    {
                        path.Add((byte)reader.ReadByte());
                    }
                    alias.path = path.Take(path.Count - 5).ToString();
                    reader.Seek(13, SeekOrigin.Current);
                    byte[] enableBDC = new byte[5];
                    reader.Read(enableBDC, (int)reader.Position, 5);
                    alias.enableBCD = enableBDC == new byte[5]{ 0x54, 0x52, 0x55, 0x45, 0x00};
                    aliases.Add(alias);
                    reader.Seek(31, SeekOrigin.Current);
                }
            }
            return aliases;
        }
    }
}
