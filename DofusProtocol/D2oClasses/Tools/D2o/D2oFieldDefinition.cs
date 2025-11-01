using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Stump.DofusProtocol.D2oClasses.Tools.D2o
{
	public class D2OFieldDefinition
	{
		internal System.Reflection.FieldInfo FieldInfo
		{
			get;
			set;
		}

		public Type FieldType
		{
			get
			{
				return (this.PropertyInfo != null ? this.PropertyInfo.PropertyType : this.FieldInfo.FieldType);
			}
		}

		public string Name
		{
			get;
			set;
		}

		internal long Offset
		{
			get;
			set;
		}

		internal System.Reflection.PropertyInfo PropertyInfo
		{
			get;
			set;
		}

		public D2OFieldType TypeId
		{
			get;
			set;
		}

		public Tuple<D2OFieldType, Type>[] VectorTypes
		{
			get;
			set;
		}

		public D2OFieldDefinition(string name, D2OFieldType typeId, System.Reflection.FieldInfo fieldInfo, long offset, params Tuple<D2OFieldType, Type>[] vectorsTypes)
		{
			this.Name = name;
			this.TypeId = typeId;
			this.FieldInfo = fieldInfo;
			this.Offset = offset;
			this.VectorTypes = vectorsTypes;
		}

		public D2OFieldDefinition(string name, D2OFieldType typeId, System.Reflection.PropertyInfo propertyInfo, long offset, params Tuple<D2OFieldType, Type>[] vectorsTypes)
		{
			this.Name = name;
			this.TypeId = typeId;
			this.PropertyInfo = propertyInfo;
			this.Offset = offset;
			this.VectorTypes = vectorsTypes;
		}

		public object GetValue(object instance)
		{
			object value;
			if (this.PropertyInfo == null)
			{
				if (this.FieldInfo == null)
				{
					throw new NullReferenceException();
				}
				value = this.FieldInfo.GetValue(instance);
			}
			else
			{
				value = this.PropertyInfo.GetValue(instance, null);
			}
			return value;
		}

		public void SetValue(object instance, object value)
		{
			if (this.PropertyInfo != null)
			{
				this.PropertyInfo.SetValue(instance, value, null);
			}
			else if (this.FieldInfo != null)
			{
				this.FieldInfo.SetValue(instance, value);
			}
		}
	}
}