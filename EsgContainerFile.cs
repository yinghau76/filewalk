using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using FileWalk.Schema;

namespace FileWalk
{
    public enum EsgFileType
    {
        Container,
        AccessDescriptor,
    };

    /// <summary>
    /// Esg Container File Format
    /// </summary>
    public class EsgContainerFile : ContainerFile
    {
        private string _fname;
        private static Dictionary<string, StructElement> _defs;
        private StructInstance _esg = new StructInstance(null, null);
        private EsgFileType _fileType;

        public EsgContainerFile(string fname, EsgFileType fileType)
        {
            _fname = fname;
            _fileType = fileType;

            if (_defs == null)
            {
                _defs = new Dictionary<string, StructElement>();
                FileSchema schema = FileSchema.Load("FileWalk.Schema.ESG.xml");
                foreach (StructElement def in schema.Structs)
                {
                    _defs[def.Name] = def;
                }
            }
        }

        #region ContainerFile Members

        public void Walk(ContainerVisitor visitor)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(_fname)))
            {
                switch (_fileType)
                {
                    case EsgFileType.Container:
                        ParseContainer(visitor, reader, 0, reader.BaseStream.Length, _esg);
                        break;
                    case EsgFileType.AccessDescriptor:
                        ParseAccessDescriptor(visitor, reader, 0, reader.BaseStream.Length, _esg);
                        break;
                    default:
                        Debug.Fail("Unhandled ESG file type!");
                        break;
                }
            }
        }

        #endregion

        enum EsgFragmentType
        {
            XmlFragment = 0,
            EsgAuxiliaryData = 1,
            PrivateAuxiliaryData = 2,
        }

        enum EsgStructureType
        {
            Reserved = 0x00,
            FragmentManagementInformation = 0x01,
            DataRepository = 0x02,
            IndexList = 0x03,
            Index = 0x04,
            MultiFieldSubIndex = 0x05,
            EsgDataRepository = 0xE0,
            SessionPartitionDeclaration = 0xE1,
            InitMessage = 0xE2,
        }

        class EsgStructure
        {
            public EsgStructureType type;
            public uint id;
            public uint ptr;
            public uint length;
        }

        public string GetDescription(StructElement def, StructInstance inst)
        {
            StringBuilder sb = new StringBuilder();

            if (def.Description != string.Empty)
            {
                sb.Append(def.Description);
                sb.Append("\r\n\r\n");
            }

            sb.Append(FileSchema.DumpInstance(inst, string.Empty).Replace("\n", "\r\n"));
            
            return sb.ToString();
        }

        public void ParseContainer(ContainerVisitor visitor, BinaryReader reader, long offset, long stopAt, StructInstance parent)
        {
            int num_structures = reader.ReadByte();
            for (int i = 0; i < num_structures; i++)
            {
                EsgStructure s = new EsgStructure();
                s.type = (EsgStructureType)reader.ReadByte();
                s.id = reader.ReadByte();
                s.ptr = Util.ReadUimsbf(reader, 3);
                s.length = Util.ReadUimsbf(reader, 3);

                // Save current position
                Int64 pos = reader.BaseStream.Position;

                // Parse this structure
                StructElement def = _defs[s.type.ToString()];
                string desc = string.Empty;
                string id = string.Empty;
                if (def != null)
                {
                    reader.BaseStream.Position = s.ptr;
                    StructInstance inst = (StructInstance) def.Parse(null, reader.BaseStream, s.ptr + s.length);
                    desc = GetDescription(def, inst);

                    // Link with the root
                    id = s.type.ToString() + s.id.ToString();
                    _esg[id] = inst;
                }

                visitor.BeginVisitNode(s.type.ToString(), desc);
                if (id == "EsgDataRepository0")
                {
                    ParseEsgDataRepository(id, reader, s, visitor);
                }
                visitor.EndVisitNode();

                // Restore original position
                reader.BaseStream.Position = pos;
            }
        }

        private void ParseEsgDataRepository(string id, BinaryReader reader, EsgStructure s, ContainerVisitor visitor)
        {
            StructInstance fmi = (StructInstance)_esg["FragmentManagementInformation0"];
            StructInstance repos = (StructInstance)_esg[id];

            if (fmi != null)
            {
                ArrayInstance fragments = (ArrayInstance)fmi["fragment_references"];
                foreach (StructInstance reference in fragments.Elements)
                {
                    uint esg_data_repository_offset = (uint)reference["esg_data_repository_offset"];
                    uint fragment_id = (uint)reference["fragment_id"];
                    EsgFragmentType fragmentType = (EsgFragmentType)reference["esg_fragment_type"];

                    // Find definition of this fragment.
                    StructElement fragmentDef = _defs[fragmentType.ToString()];

                    // Seek to offset relative to the beginning of ESG data repository.
                    reader.BaseStream.Position = s.ptr + esg_data_repository_offset;
                    StructInstance inst = (StructInstance)fragmentDef.Parse(
                        repos, reader.BaseStream, s.ptr + s.length);

                    string fragmentName = fragmentType.ToString() + " #" + fragment_id.ToString();
                    visitor.BeginVisitNode(fragmentName, GetDescription(fragmentDef, inst));
                    visitor.EndVisitNode();
                }
            }
        }

        private void ParseAccessDescriptor(ContainerVisitor visitor, BinaryReader reader, long offset, long stopAt, StructInstance parent)
        {
            StructElement def = _defs["ESGAccessDescriptor"];
            StructInstance inst = (StructInstance) def.Parse(_esg, reader.BaseStream, reader.BaseStream.Length);

            visitor.BeginVisitNode(def.Name, GetDescription(def, inst));
            visitor.EndVisitNode();
        }
   }
}
