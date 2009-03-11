using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using FileWalk.Schema;

namespace FileWalk
{
	/// <summary>
	/// Summary description for QuickTimeFile.
	/// </summary>
	public class QuickTimeFile : ContainerFile
	{
		private string _fname;
        private static Dictionary<string,StructElement> _atomDefs;

		public QuickTimeFile(string fname)
		{
			_fname = fname;

            if (_atomDefs == null)
            {
                _atomDefs = new Dictionary<string, StructElement>();
                FileSchema schema = FileSchema.Load("FileWalk.Schema.QuickTime.xml");
                foreach (StructElement atomDef in schema.Structs)
                {
                    _atomDefs[atomDef.Name] = atomDef;
                }
            }
		}

		#region ContainerFile Members

		public void Walk(ContainerVisitor visitor)
		{
			using (BinaryReader reader = new BinaryReader(File.OpenRead(_fname)))
			{
				ParseAtoms(visitor, reader, 0, reader.BaseStream.Length, null);
			}
		}

		#endregion

		public void ParseAtoms(ContainerVisitor visitor, BinaryReader reader, long offset, long stopAt, StructInstance parent)
		{
			while (offset < stopAt) 
			{
				reader.BaseStream.Seek(offset, SeekOrigin.Begin);

				uint atomSize = Util.EndianFlip32(reader.ReadUInt32());
				string atomType = Util.ReadFourCC(reader);

                if (_atomDefs.ContainsKey(atomType))
                {
                    StructElement atomDef = _atomDefs[atomType];

                    StructInstance inst = GetAtomInstance(reader, atomDef, atomSize, parent);
                    string desc = GetAtomDesc(atomDef, atomSize, inst);
                    visitor.BeginVisitNode(atomType, desc);

                    // If it is a container atom
                    if (atomDef != null && atomDef.Elements == null)
                    {
                        // parse children atoms recursively
                        ParseAtoms(visitor, reader, reader.BaseStream.Position, offset + atomSize, inst);
                    }

                    if (parent != null)
                    {
                        parent[atomType] = inst;
                    }

                    visitor.EndVisitNode();
                }
                
				// now set offset to next atom (or end-of-file) in special case 
				// (atomSize = 0 means atom goes to EOF)
				if (atomSize == 0)
				{
					offset = reader.BaseStream.Length;
				}
				else 
				{
					offset += atomSize;
				}

				// if a 'udta' container atom, then jump ahead 4 to work around 
				// Apple's QT 1.0 workaround 
				// @see http://developer.apple.com/technotes/qt/qt_03.html
				if (atomType == "udta")
				{
					offset += 4;
				}
			}
		}

		private StructInstance GetAtomInstance(BinaryReader reader, StructElement atomDef, uint atomSize, StructInstance parent)
		{
			StructInstance inst = null;

			if (atomDef != null)
			{
				int size = atomDef.Size;
				if (size != 0)
				{
					inst = atomDef.Parse(parent, reader.BaseStream, reader.BaseStream.Position + atomSize - 8) as StructInstance;
				}
				else
				{
					inst = atomDef.Parse(parent, null, 0) as StructInstance;
				}
			}

			return inst;
		}

		private string GetAtomDesc(StructElement atomDef, uint atomSize, StructInstance inst)
		{
			StringBuilder sb = new StringBuilder();

			if (atomDef != null && atomDef.Description != string.Empty)
			{
				sb.Append(atomDef.Description);
				sb.Append("\r\n\r\n");
			}
			sb.AppendFormat("Atom size = {0}\r\n\r\n", atomSize);

            if (inst != null)
            {
				sb.Append(FileSchema.DumpInstance(inst, string.Empty));
            }

			return sb.ToString();
		}
	}
}
