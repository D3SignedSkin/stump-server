using System;
using System.Runtime.CompilerServices;

namespace Stump.DofusProtocol.D2oClasses.Tools.D2o
{
	public class D2OSearchEntry
	{
		public int FieldCount
		{
			get;
			set;
		}

		public int FieldIndex
		{
			get;
			set;
		}

		public string FieldName
		{
			get;
			set;
		}

		public D2OFieldType FieldType
		{
			get;
			set;
		}

		public D2OSearchEntry()
		{
		}

		public D2OSearchEntry(string fieldName, int fieldIndex, D2OFieldType fieldType, int fieldCount)
		{
			this.FieldName = fieldName;
			this.FieldIndex = fieldIndex;
			this.FieldType = fieldType;
			this.FieldCount = fieldCount;
		}
	}
}