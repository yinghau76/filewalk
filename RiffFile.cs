using System;
using System.IO;
using System.Collections.Generic;
using FileWalk.Schema;
using System.Text;

namespace FileWalk
{
    /// <summary>
    /// Base RIFF Format
    /// </summary>
    public class RiffFile : ContainerFile
    {
        protected string _fname;
        protected int _fileSize;
        protected string _fileType;

        private static Dictionary<string, StructElement> _chunkDefs;

        public RiffFile(string fname)
        {
            _fname = fname;

            if (_chunkDefs == null)
            {
                _chunkDefs = new Dictionary<string, StructElement>();
                FileSchema schema = FileSchema.Load("FileWalk.Schema.AVI.xml");
                foreach (StructElement chunk in schema.Structs)
                {
                    _chunkDefs[chunk.Name] = chunk;
                }
            }
        }

        #region ContainerFile Members

        public void Walk(ContainerVisitor visitor)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(_fname)))
            {
                // 'RIFF' fileSize fileType (data)
                if (Util.ReadFourCC(reader) == "RIFF")
                {
                    _fileSize = reader.ReadInt32();
                    _fileType = Util.ReadFourCC(reader);

                    visitor.BeginVisitNode(string.Format("RIFF ({0})", _fileType), string.Empty);

                    ParseListsChunks(visitor, reader, reader.BaseStream.Position, _fileSize + 8, null);

                    visitor.EndVisitNode();
                }
            }
        }

        #endregion

        public void ParseListsChunks(ContainerVisitor visitor, BinaryReader reader, long offset, long stopAt, StructInstance parent)
        {
            while (offset < stopAt) 
            {
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);

                string id = Util.ReadFourCC(reader);
                int size = reader.ReadInt32();
                if (size % 2 != 0)
                {
                    size++;
                }

				StructInstance inst = null;

                if (id == "LIST") // 'LIST' listSize listType listData
                {
                    string listType = Util.ReadFourCC(reader);

					// a LIST instance is needed to maintain a list of sub LISTs and CHUNKs.
					StructElement listDef = _chunkDefs["LIST"];
					inst = GetChunkInstance(reader, listDef, 0, parent);

					if (listType != "movi")
                    {
						visitor.BeginVisitNode(
							string.Format("LIST ({0})", listType), string.Empty);

                        ParseListsChunks(visitor, reader, reader.BaseStream.Position, offset + size + 8, inst);
                    }
					else
					{
						visitor.BeginVisitNode(
							string.Format("LIST ({0})", listType), "Content in this list is skipped.");
					}

                    offset += size + 8;
                }
                else // ckID ckSize ckData
                {
					StructElement chunkDef = _chunkDefs[id];
					inst = GetChunkInstance(reader, chunkDef, size, parent);

                    visitor.BeginVisitNode(id, GetChunkDescription(reader, id, size, inst));

                    offset += size + 8;
                }

				if (parent != null)
				{
					// link CHUNK to its parent LIST
					parent[id] = inst;
				}

				visitor.EndVisitNode();
            }
        }

		private StructInstance GetChunkInstance(BinaryReader reader, StructElement chunkDef, int chunkSize, StructInstance parent)
		{
			StructInstance inst = null;

			if (chunkDef != null)
			{
				int size = chunkDef.Size;
				if (size != 0)
				{
					inst = chunkDef.Parse(parent, reader.BaseStream, reader.BaseStream.Position + chunkSize) as StructInstance;
				}
				else
				{
					inst = chunkDef.Parse(parent, null, 0) as StructInstance;
				}
			}

			return inst;
		}

        public virtual string GetChunkDescription(BinaryReader reader, string id, int chunkSize, StructInstance inst)
        {
            StructElement chunkDef = _chunkDefs[id];

			StringBuilder sb = new StringBuilder();

			if (chunkDef != null && chunkDef.Description != string.Empty)
			{
				sb.Append(chunkDef.Description);
				sb.Append("\r\n\r\n");
			}
            sb.AppendFormat("Chunk size = {0}\r\n\r\n", chunkSize);

			if (inst != null)
			{
				sb.Append(FileSchema.DumpInstance(inst, string.Empty));
			}

			return sb.ToString();
        }
    }
}
