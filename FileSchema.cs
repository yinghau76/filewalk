using System;
using System.Xml.Serialization;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.IO;

namespace FileWalk.Schema
{
    [XmlRoot("FileSchema")]
    public class FileSchema
    {
        [XmlElement("Struct")]
        public StructElement[] Structs;

        public static FileSchema Load(string schemaResource)
        {
            try
            {
                // Serialize the order to a file.
                XmlSerializer serializer = new XmlSerializer(typeof(FileSchema));

                // Deserialize the order from the file.
                System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(schemaResource);
                FileSchema schema = (FileSchema) serializer.Deserialize(stream);

				schema.OnDeserialization();

                return schema;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }

		/// <summary>
		/// Called when the entire file schema has been deserialized.
		/// </summary>
		private void OnDeserialization() 
		{
			foreach (StructElement st in Structs)
			{
				st.OnDeserialization(this);
			}
		}

        private static string GetFullName(string namePrefix, string name)
        {
            if (namePrefix == null || namePrefix == string.Empty)
            {
                return name;
            }
            else
            {
                return string.Format("{0}.{1}", namePrefix, name);
            }
        }

		/// <summary>
		/// Dump the content of this element as string.
		/// </summary>
		/// <param name="obj">The instance to dump</param>
		/// <param name="namePrefix"></param>
		/// <returns></returns>
		public static string DumpInstance(object obj, string namePrefix)
		{
			if (obj is StructInstance)
			{
				StructInstance si = obj as StructInstance;

				StringBuilder sb = new StringBuilder();
				foreach (Element elem in si.Elements)
				{
                    string name = GetFullName(namePrefix, elem.Name);
                    if (elem.Name == null && elem is FieldElement)
                    {
                        FieldElement fieldElem = elem as FieldElement;
                        foreach (BitsField f in fieldElem.BitsFields)
                        {
                            name = GetFullName(namePrefix, f.name);
                            object readable = elem.ToReadable(si[f.name]);
						    sb.Append(DumpInstance(readable, name));
                        }
                    }
                    else if (si[elem.Name] != null)	// some element may not exist because of condition.
					{
						// Convert integer value into string if necessary.
						object readable = elem.ToReadable(si[elem.Name]);
						sb.Append(DumpInstance(readable, name));
					}               
				}
				return sb.ToString();
			}
			else if (obj is ArrayInstance)
			{
				StringBuilder sb = new StringBuilder();

				ArrayInstance inst = obj as ArrayInstance;
				for (int i = 0; i < inst.Elements.Length; i++)
				{
					string name = string.Format("{0}[{1}]", namePrefix, i);
					sb.Append(DumpInstance(inst.Type.ToReadable(inst.Elements[i]), name));
				}

				return sb.ToString();
			}
			else // it is a primitive type such as integer, string, etc.
			{
				return string.Format("{0} = {1}\r\n", namePrefix, obj);
			}
		}
    }

	public abstract class Element
	{
		/// <summary>
		/// Description of this element.
		/// </summary>
		[XmlElement("Description")]
		public string Description = string.Empty;

		/// <summary>
		/// The offset relative to the start of parent element in bytes.
		/// </summary>
		[XmlAttribute("offset")]
		public int Offset = 0;

		/// <summary>
		/// Unique name of this element.
		/// </summary>
		[XmlAttribute("name")]
		public string Name;

		/// <summary>
		/// Specify the condition to include this member. For example, 'size == "mp4v"'.
		/// </summary>
		[XmlAttribute("condition")]
		public string Condition;

		/// <summary>
		/// The size of this element in bytes.
		/// </summary>
		public abstract int Size { get; }

		/// <summary>
		/// Called when the entire element has been deserialized.
		/// </summary>
		public virtual void OnDeserialization(FileSchema schema)
		{}

		public virtual object ToReadable(object o)
		{
			return o;
		}

		/// <summary>
		/// To construct instance from specified stream.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="stream"></param>
		/// <param name="stopAt"></param>
		/// <returns></returns>
		public abstract object Parse(ElementInstance parent, Stream stream, long stopAt);

        /// <summary>
        /// Calculate array size
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
		public static int CalcElementArraySize(Element[] elements)
		{
			int size = 0;

			if (elements != null) 
			{
				foreach (Element elem in elements)
				{
					if (elem.Size < 0)
					{
						return -1;	// there is a variable-length field.
					}

					size += elem.Size;
				}
			}

			return size;
		}

        /// <summary>
        /// Evaluate bool expression
        /// </summary>
        /// <param name="inst"></param>
        /// <returns></returns>
		public bool EvalCondition(ElementInstance inst)
		{
			if (Condition != null)
			{
				try
				{
					BoolExprLexer lexer = new BoolExprLexer(new StringReader(Condition));
					BoolExprParser parser = new BoolExprParser(lexer);
					ConditionEvaluator treeParser = new ConditionEvaluator();

					parser.expression();
					antlr.collections.AST ast = parser.getAST();

					treeParser.Instance = inst;
					return treeParser.expression(ast);
				}
				catch (Exception ex)
				{
					Debug.Write(ex);
					return false;
				}
			}

			return true;
		}
	}

	public class StructElement : Element
	{
		[XmlElement("Field", typeof(FieldElement))]
		[XmlElement("Array", typeof(ArrayElement))]
		[XmlElement("Struct", typeof(StructElement))]
		public Element[] Elements;

		[XmlAttribute("type")]
		public string DataType;

		public override int Size
		{
			get 
			{
				return Element.CalcElementArraySize(Elements);
			}
		}

		public override void OnDeserialization(FileSchema schema)
		{
			if (Elements != null)
			{
				foreach (Element elem in Elements)
				{
					elem.OnDeserialization(schema);
				}
			}
			else if (DataType != null)
			{
				foreach (StructElement s in schema.Structs)
				{
					if (s.Name == DataType)
					{
						Elements = s.Elements;
					}
				}
			}
		}	

		/// <summary>
		/// Create an instance by specified byte array and start position.
		/// </summary>
		public override object Parse(ElementInstance parent, Stream stream, long stopAt)
		{
			StructInstance inst = new StructInstance(parent, Elements);
			if (Elements != null)
			{
				foreach (Element elem in Elements)
				{
					if (elem.EvalCondition(inst))
					{
						if (stream.Position >= stopAt)
						{
							break;	// don't read beyond boundary.
						}

                        object elemInst = elem.Parse(inst, stream, stopAt);
                        // Elements that don't have a name will not be added directly
                        if (elem.Name != null && elem.Name != string.Empty)
                        {
                            inst[elem.Name] = elemInst;
                        }
					}
				}
			}
			return inst;
		}
	}

	/// <summary>
	/// DataType + Length = Array
	/// </summary>
	public class ArrayElement : Element
	{
		[XmlAttribute("length")]
		public string Length;

		[XmlAttribute("type")]
		public string DataType;

		[XmlAttribute("format")]
		public string FormatType;

		private Element DataElement;

		public override int Size
		{
			get
			{
				if (Length == null)
				{
					return -1;
				}

				int len = -1;
				try
				{
					len = Convert.ToInt32(Length);
					Debug.Assert(DataElement != null);
					Debug.Assert(DataElement.Size > 0);
					return len * DataElement.Size;
				}
				catch (FormatException)
				{}

				return len;
			}
		}

		/// <summary>
		/// Create an instance by specified byte array and start position.
		/// </summary>
		/// <returns>An System.Array that contains all array members.</returns>
		public override object Parse(ElementInstance parent, Stream stream, long stopAt)
		{
			Debug.Assert(DataElement != null);

			int len = EvalLength(parent, stream, stopAt);
			
			ArrayList list = new ArrayList();
			for (int i = 0; i < len; i++)
			{
				list.Add(DataElement.Parse(parent, stream, stopAt));
			}

			ArrayInstance inst = new ArrayInstance(parent, DataElement, list.ToArray());
			return inst;
		}

		public override void OnDeserialization(FileSchema schema)
		{
			foreach (StructElement s in schema.Structs)
			{
				// The array element is a struct.
				if (s.Name == DataType)
				{
					DataElement = s;
					return;
				}
			}

			// Treat specified DataType as FieldElement.
			FieldElement field = new FieldElement();
			field.DataType = DataType;
			field.FormatType = FormatType;
			field.Name = "ArrayElement";
			DataElement = field;
		}

		private int EvalLength(ElementInstance parent, Stream stream, long stopAt)
		{
			int len = 0;

			if (Length == null)
			{
				Debug.Assert(stopAt >= stream.Position);
				len = (int)((stopAt - stream.Position) / DataElement.Size);
			}
			else
			{
				try
				{
					len = Convert.ToInt32(Length);
				}
				catch (FormatException)
				{
					IConvertible evalLen = parent.Eval(Length) as IConvertible;
					if (evalLen != null)
					{
						len = evalLen.ToInt32(null);
					}
				}
			}

			return len;
		}
	}

    public class FieldElement : Element
    {
        [XmlAttribute("type")]
        public string DataType;

		[XmlElement("Flag")]
		public FieldFlag[] Flags;

		[XmlElement("Enum")]
		public FieldEnum[] Enums;

        [XmlElement("BitsField")]
        public BitsField[] BitsFields;

		[XmlAttribute("format")]
		public string FormatType;

		public override int Size
        {
			get
			{
				char prefix = Char.ToLower(DataType[0]);

				if ("bs".IndexOf(prefix) >= 0)
				{
					return Int32.Parse(DataType.Substring(1));
				}
				else if ("iuf".IndexOf(prefix) >= 0)
				{
					return Int32.Parse(DataType.Substring(1))/8;
				}
				else if ("zv".IndexOf(prefix) >= 0)
				{
					return -1;   // variable length
				}

				return 0;
			}
        }

		public override object Parse(ElementInstance parent, Stream stream, long stopAt)
		{
			char type = DataType[0];
			char prefix = Char.ToLower(type);
			int size = Size;

			string dataType = DataType.ToLower();

			if (prefix == 'b')  // byte
			{
				if (size <= 1)
				{
                    int b = stream.ReadByte();
                    AddBitFields(parent, b);
					return b;
				}
				else
				{
					byte[] data = new byte[size];
					stream.Read( data, 0, size);
					return data;
				}
			}
			else if (prefix == 's') // fixed-length string
			{
				byte[] data = new byte[size];
				stream.Read( data, 0, size);
				string s = Encoding.UTF8.GetString(data, 0, size);

				// a workaround for '\0'-stuffed stream
				if (s.IndexOf('\0') >= 0)
				{
					s = s.Substring( 0, s.IndexOf("\0"));
				}
				return s;
			}
			else if (prefix == 'z') // zero-ended string
			{
				size = (int) (stopAt - stream.Position);

				byte[] data = new byte[size];
				stream.Read( data, 0, size);

				int z = Array.IndexOf(data, 0, 0, size);
				if (z < 0)
					z = data.Length;

				string s = Encoding.UTF8.GetString(data, 0, z);
				return s;
			}
            // vluimsbf8: Variable length code unsigned integer, most significant bit first. 
            // The size of vluimsbf8 is a multiple of one byte. The first bit (Ext) of each 
            // byte specifies if set to 1 that another byte is present for this vluimsbf8 code
            // word. The unsigned integer is encoded by the concatenation of the seven least 
            // significant bits of each byte belonging to this vluimsbf8 code word.
            else if (prefix == 'v')
            {
                int i = 0;
                int b = 0x80;
                int bytes = 0;
                for (; (b & 0x80) != 0; bytes++)
                {
                    b = stream.ReadByte();
                    i = i << 7 | (b & 0x7f);
                }

                if (type == 'v' && bytes > 1)
                {
                    return Util.EndianFlip(i, bytes);
                }
                return i;
            }
			else if ("iuf".IndexOf(prefix) >= 0) // signed/unsigned integer
			{
				byte[] data = new byte[size];
				stream.Read( data, 0, size);

				if (size == 8)
				{
					long i = data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24 |
						     data[4] << 32 | data[5] << 40 | data[6] << 48 | data[7] << 56;
					return i;
				}
				else if (size == 4 || size == 3)
				{
					int i;
					
					if (size == 4)
					{
						i = data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24;
					}
					else
					{
						i = data[0] | data[1] << 8 | data[2] << 16;
					}

					// Big-indian integer is identified by upper case 'I'
					if (type == 'I')
					{
						return Util.EndianFlip(i, size);
					}
                    // Big-indian unsigned integer is identified by upper case 'U'
					else if (type == 'U')
					{
						return Util.EndianFlip((uint) i, size);
					}
					else if (type == 'u')
					{
						return (uint) i;
					}
					return i;
				}
				else if (size == 2)
				{
					short i = (short) (data[0] | data[1] << 8);
					if (type == 'I')
					{
						return (short) Util.EndianFlip((int) i, size);
					}
					else if (type == 'U')
					{
						return (ushort) Util.EndianFlip(((uint) i), size);
					}
					else if (type == 'u')
					{
						return (ushort) i;
					}

					return i;
				}
				else
				{
					Debug.Assert( false, "Invalid field size!");
				}
			}

			return null;
		}

		/// <summary>
		/// Convert specified value to readable string if necessary
		/// </summary>
		public override object ToReadable(object o)
		{
			if (Enums != null && (o is byte || o is short || o is ushort || o is int || o is uint))
			{
				return ToEnum(Convert.ToInt32(o));
			}

			if (Flags != null && (o is ushort || o is uint))
			{
				return ToFlags(Convert.ToUInt32(o));
			}

			if (FormatType != null)
			{
                if (FormatType == "ipv4" && o is int)
                {
                    int ipAddr = Convert.ToInt32(o);
                    string readable = string.Format("{0}.{1}.{2}.{3}",
                        (ipAddr & 0x000000ff),
                        (ipAddr & 0x0000ff00) >> 8,
                        (ipAddr & 0x00ff0000) >> 16,
                        (ipAddr & 0xff000000) >> 24);
                    return readable;
                }
                else if (o is IFormattable)
                {
                    string readable = (o as IFormattable).ToString(FormatType, null);
                    if (FormatType.StartsWith("x"))
                    {
                        readable = "0x" + readable;
                    }
                    return readable;
                }
			}

			return o;
		}

		private string ToFlags(uint i)
		{
			StringBuilder sb = new StringBuilder(128);
			foreach (FieldFlag f in Flags)
			{
				uint mask = Convert.ToUInt32( f.mask, 16);
				if ((i & mask) == mask)
				{
					if (sb.Length > 0)
					{
						sb.Append(" | ");
					}

					sb.Append(f.name);
				}
			}

			if (sb.Length == 0)
			{
				// no flags was matched.
				sb.Append(i.ToString());
			}

			return sb.ToString();
		}

		private string ToEnum(int i)
		{
			return ToEnum((uint) i);
		}

		private string ToEnum(uint i)
		{
			foreach (FieldEnum f in Enums)
			{
				string v = f.value.Trim();
				int value = v.StartsWith("0x") ? 
					Convert.ToInt32(v, 16) : Convert.ToInt32(v);
				if (i == value)
				{
					return f.name;
				}
			}

			return i.ToString();
		}

        private void AddBitFields(ElementInstance parent, int data)
        {
            if (BitsFields != null && parent is StructInstance)
            {
                foreach (BitsField f in BitsFields)
                {
                    Debug.Assert(f.length >= 1);
                    int mask = (1 << f.length) - 1;
                    int bits = (data >> f.offset) & mask;
                    ((StructInstance) parent)[f.name] = bits;
                }
            }
        }
    }

	/// <summary>
	/// Represent named mask to show readable value.
	/// </summary>
	public class FieldFlag
	{
		[XmlAttribute("name")]
		public string name;

		[XmlAttribute("mask")]
		public string mask;
	}

	/// <summary>
	/// Represent named enumeration to show readable value.
	/// </summary>
	public class FieldEnum
	{
		[XmlAttribute("name")]
		public string name;

		[XmlAttribute("value")]
		public string value;
	}

    /// <summary>
    /// Provides access to fields that does not begin or end in byte boundary.
    /// </summary>
    public class BitsField
    {
        [XmlAttribute("name")]
        public string name;

        [XmlAttribute("offset")]
        public int offset;

        [XmlAttribute("length")]
        public int length;
    }
}
