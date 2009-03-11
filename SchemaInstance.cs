using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;

namespace FileWalk.Schema
{
	/// <summary>
	/// Define common members and methods of StructInstance and ArrayInstance.
	/// </summary>
	public abstract class ElementInstance
	{
		/// <summary>
		/// The parent of this instance in structure hierarchy.
		/// </summary>
		protected ElementInstance _parent;

		public ElementInstance(ElementInstance parent)
		{
			_parent = parent;
		}

		/// <summary>
		/// Evaluate specified expression.
		/// </summary>
		public abstract object Eval(string expr);
	}

	/// <summary>
	/// The instance of an struct element.
	/// </summary>
	public class StructInstance : ElementInstance
	{
		private Hashtable _memberOfName = new Hashtable();
		private Element[] _elements;

		public StructInstance(ElementInstance parent, Element[] elements) : base(parent)
		{
			_elements = elements;
		}

		public object this[string name]
		{
			get
			{
				return _memberOfName[name];
			}
			set
			{
				_memberOfName[name] = value;
			}
		}

		public IEnumerable Elements
		{
			get 
			{
				if (_elements == null)
				{
					_elements = new Element[0];
				}

				return _elements;
			}
		}

		public Hashtable Members
		{
			get
			{
				if (_memberOfName == null)
				{
					_memberOfName = new Hashtable();
				}
				return _memberOfName;
			}
		}

		/// <summary>
		/// Evaluate given expression to value.
		/// </summary>
		/// <param name="expr">format: a.b.c.d</param>
		public override object Eval(string expr)
		{
			string[] names = expr.Split(new char[] {'.'});

			object val = (names[0] == "parent") ? 
				_parent :			// parent object
				this[names[0]];		// lookup by name

			if (val is StructInstance)
			{   
				// rebuild the rest of names.
				StringBuilder sb = new StringBuilder();
				for (int i = 1; i < names.Length; i++)
				{
					if (sb.Length > 0)
					{
						sb.Append('.');
					}
					sb.Append(names[i]);
				}

				return (val as StructInstance).Eval(sb.ToString());
			}
			else
			{
				return val;
			}
		}
	}

	public class ArrayInstance : ElementInstance
	{
		public Element Type;
		public Object[] Elements;

		public ArrayInstance(ElementInstance parent, Element type, Object[] elements) : base(parent)
		{
			this.Type = type;
			this.Elements = elements;
		}

		/// <summary>
		/// To evaluate "[index]" to access array element.
		/// </summary>
		/// <param name="expr"></param>
		/// <returns></returns>
		public override object Eval(string expr)
		{
			Regex regexIndexer = new Regex(@"\[(.*)\]");
			Match match = regexIndexer.Match(expr);
			if (match.Success)
			{
				int index = Int32.Parse(match.Groups[1].Value);
				return Elements[index];
			}

			return null;
		}
	}
}