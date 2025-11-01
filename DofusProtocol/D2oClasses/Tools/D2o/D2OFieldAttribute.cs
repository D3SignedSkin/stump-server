using System;
using System.Runtime.CompilerServices;

namespace Stump.DofusProtocol.D2oClasses.Tools.D2o
{
	public class D2OFieldAttribute : Attribute
	{
		public string FieldName
		{
			get;
			set;
		}

		public D2OFieldAttribute()
		{
		}

		public D2OFieldAttribute(string fieldName)
		{
			this.FieldName = fieldName;
		}
	}
}