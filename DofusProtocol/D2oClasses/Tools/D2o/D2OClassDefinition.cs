using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Stump.DofusProtocol.D2oClasses.Tools.D2o
{
	[DebuggerDisplay("Name = {Name}")]
	public class D2OClassDefinition
	{
		public Type ClassType
		{
			get;
			private set;
		}

		public Dictionary<string, D2OFieldDefinition> Fields
		{
			get;
			private set;
		}

		public int Id
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		internal long Offset
		{
			get;
			set;
		}

		public string PackageName
		{
			get;
			private set;
		}

		public D2OClassDefinition(int id, string classname, string packagename, Type classType, IEnumerable<D2OFieldDefinition> fields, long offset)
		{
			this.Id = id;
			this.Name = classname;
			this.PackageName = packagename;
			this.ClassType = classType;
			this.Fields = fields.ToDictionary<D2OFieldDefinition, string>((D2OFieldDefinition entry) => entry.Name);
			this.Offset = offset;
		}
	}
}