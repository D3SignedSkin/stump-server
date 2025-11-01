using Newtonsoft.Json;
using Stump.Core.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Stump.DofusProtocol.D2oClasses.Tools.D2o
{
	public class D2OWriter : IDisposable
	{
		private const int NullIdentifier = -1431655766;

		private readonly object m_writingSync = new object();

		private Dictionary<Type, int> m_allocatedClassId = new Dictionary<Type, int>();

		private Dictionary<int, D2OClassDefinition> m_classes;

		private Dictionary<int, int> m_indexTable;

		private bool m_needToBeSync;

		private Dictionary<int, object> m_objects = new Dictionary<int, object>();

		private BigEndianWriter m_writer;

		private bool m_writing;

		private int m_nextIndex;

		public string BakFilename
		{
			get;
			set;
		}

		public string Filename
		{
			get;
			set;
		}

		public Dictionary<int, object> Objects
		{
			get
			{
				return this.m_objects;
			}
		}

		public D2OWriter(string filename, bool readDefinitionsOnly = false)
		{
			this.Filename = filename;
			if (File.Exists(filename))
			{
				this.OpenWrite(readDefinitionsOnly);
			}
			else
			{
				this.CreateWrite(filename);
			}
		}

		private int AllocateClassId(Type classType)
		{
			int id = (this.m_allocatedClassId.Count > 0 ? this.m_allocatedClassId.Values.Max() + 1 : 1);
			this.AllocateClassId(classType, id);
			return id;
		}

		private void AllocateClassId(Type classType, int classId)
		{
			this.m_allocatedClassId.Add(classType, classId);
		}

		private string ConvertNETTypeToAS3(Type type)
		{
			D2OClassDefinition @class;
			string str;
			bool flag;
			string name = type.Name;
			if (name != null)
			{
				switch (name)
				{
					case "Int32":
					case "Int16":
					case "UInt16":
					{
						str = "int";
						return str;
					}
					case "UInt32":
					{
						str = "uint";
						return str;
					}
					case "Int64":
					case "UInt64":
					case "Single":
					case "Double":
					{
						str = "Number";
						return str;
					}
					default:
					{
						if (name != "String")
						{
							flag = (!type.IsGenericType ? false : type.GetGenericTypeDefinition() == typeof(List<>));
							if (!flag)
							{
								@class = this.m_classes.Values.FirstOrDefault<D2OClassDefinition>((D2OClassDefinition x) => x.ClassType == type);
								if (@class == null)
								{
									throw new Exception(string.Format("Cannot found AS3 type associated to {0}", type));
								}
								str = string.Concat(@class.PackageName, "::", @class.Name);
							}
							else
							{
								str = string.Concat("Vector.<", this.ConvertNETTypeToAS3(type.GetGenericArguments()[0]), ">");
							}
							return str;
						}
						str = "String";
						return str;
					}
				}
			}
			flag = (!type.IsGenericType ? false : type.GetGenericTypeDefinition() == typeof(List<>));
			if (!flag)
			{
				@class = this.m_classes.Values.FirstOrDefault<D2OClassDefinition>((D2OClassDefinition x) => x.ClassType == type);
				if (@class == null)
				{
					throw new Exception(string.Format("Cannot found AS3 type associated to {0}", type));
				}
				str = string.Concat(@class.PackageName, "::", @class.Name);
			}
			else
			{
				str = string.Concat("Vector.<", this.ConvertNETTypeToAS3(type.GetGenericArguments()[0]), ">");
			}
			return str;
		}

		public static void CreateEmptyFile(string path)
		{
			if (File.Exists(path))
			{
				throw new Exception("File already exists, delete before overwrite");
			}
			BinaryWriter writer = new BinaryWriter(File.OpenWrite(path));
			writer.Write("D2O");
			writer.Write((int)writer.BaseStream.Position + 4);
			writer.Write(0);
			writer.Write(0);
			writer.Flush();
			writer.Close();
		}

		private void CreateWrite(string filename)
		{
			//this.m_writer = new BigEndianWriter(File.Create(filename));
			this.m_indexTable = new Dictionary<int, int>();
			this.m_classes = new Dictionary<int, D2OClassDefinition>();
			this.m_objects = new Dictionary<int, object>();
			this.m_allocatedClassId = new Dictionary<Type, int>();
		}

		private void DefineAllocatedTypes()
		{
			KeyValuePair<Type, int>[] array = (
				from entry in this.m_allocatedClassId
				where !this.m_classes.ContainsKey(entry.Value)
				select entry).ToArray<KeyValuePair<Type, int>>();
			for (int i = 0; i < (int)array.Length; i++)
			{
				this.DefineClassDefinition(array[i].Key);
			}
		}

		private void DefineClassDefinition(Type classType)
		{
			if (this.m_classes.Count<KeyValuePair<int, D2OClassDefinition>>((KeyValuePair<int, D2OClassDefinition> entry) => entry.Value.ClassType == classType) <= 0)
			{
				if (!this.m_allocatedClassId.ContainsKey(classType))
				{
					this.AllocateClassId(classType);
				}
				object[] attributes = classType.GetCustomAttributes(typeof(D2OClassAttribute), false);
				if ((int)attributes.Length != 1)
				{
					throw new Exception("The given class has no D2OClassAttribute attribute and cannot be wrote");
				}
				string package = ((D2OClassAttribute)attributes[0]).PackageName;
				string name = (!string.IsNullOrEmpty(((D2OClassAttribute)attributes[0]).Name) ? ((D2OClassAttribute)attributes[0]).Name : classType.Name);
				List<D2OFieldDefinition> fields = new List<D2OFieldDefinition>();
				FieldInfo[] fieldInfoArray = classType.GetFields();
				for (int i = 0; i < (int)fieldInfoArray.Length; i++)
				{
					FieldInfo field = fieldInfoArray[i];
					if ((field.GetCustomAttributes(typeof(D2OIgnore), false).Any<object>() || field.IsStatic || field.IsPrivate ? false : !field.IsInitOnly))
					{
						D2OFieldAttribute attr = (D2OFieldAttribute)field.GetCustomAttributes(typeof(D2OFieldAttribute), false).SingleOrDefault<object>();
						D2OFieldType fieldType = this.GetIdByType(field);
						Tuple<D2OFieldType, Type>[] vectorTypes = this.GetVectorTypes(field.FieldType);
						string fieldName = (attr != null ? attr.FieldName : field.Name);
						fields.Add(new D2OFieldDefinition(fieldName, fieldType, field, (long)-1, vectorTypes));
					}
				}
				PropertyInfo[] properties = classType.GetProperties();
				for (int j = 0; j < (int)properties.Length; j++)
				{
					PropertyInfo property = properties[j];
					if ((property.GetCustomAttributes(typeof(D2OIgnore), false).Any<object>() ? false : property.CanWrite))
					{
						D2OFieldAttribute attr = (D2OFieldAttribute)property.GetCustomAttributes(typeof(D2OFieldAttribute), false).SingleOrDefault<object>();
						D2OFieldType fieldType = this.GetIdByType(property);
						Tuple<D2OFieldType, Type>[] vectorTypes = this.GetVectorTypes(property.PropertyType);
						string str = (attr != null ? attr.FieldName : property.Name);
						if (!fields.Any<D2OFieldDefinition>((D2OFieldDefinition x) => x.Name == str))
						{
							fields.Add(new D2OFieldDefinition(str, fieldType, property, (long)-1, vectorTypes));
						}
					}
				}
				this.m_classes.Add(this.m_allocatedClassId[classType], new D2OClassDefinition(this.m_allocatedClassId[classType], name, package, classType, fields, (long)-1));
				this.DefineAllocatedTypes();
			}
		}

		public void Delete(int index)
		{
			lock (this.m_writingSync)
			{
				if (this.m_objects.ContainsKey(index))
				{
					this.m_objects.Remove(index);
				}
			}
		}

		public void Dispose()
		{
			if (this.m_writing)
			{
				this.EndWriting(true);
			}
		}

		public void EndWriting(bool searchTable = true)
		{
			lock (this.m_writingSync)
			{
				this.m_writer.Seek(0, SeekOrigin.Begin);
				this.m_writing = false;
				this.m_needToBeSync = false;
				this.WriteHeader();
				foreach (KeyValuePair<int, object> obj in this.m_objects)
				{
					if (this.m_indexTable.ContainsKey(obj.Key))
					{
						this.m_indexTable[obj.Key] = (int)this.m_writer.BaseStream.Position;
					}
					else
					{
						this.m_indexTable.Add(obj.Key, (int)this.m_writer.BaseStream.Position);
					}
					this.WriteObject(this.m_writer, obj.Value, obj.Value.GetType());
				}
				this.WriteIndexTable();
				this.WriteClassesDefinition();
				if (searchTable)
				{
					this.WriteSearchTable();
				}
				this.m_writer.Dispose();
			}
		}

		public void EndWriting(byte[] dataSearchTable)
		{
			lock (this.m_writingSync)
			{
				this.m_writer.Seek(0, SeekOrigin.Begin);
				this.m_writing = false;
				this.m_needToBeSync = false;
				this.WriteHeader();
				foreach (KeyValuePair<int, object> obj in this.m_objects)
				{
					if (this.m_indexTable.ContainsKey(obj.Key))
					{
						this.m_indexTable[obj.Key] = (int)this.m_writer.BaseStream.Position;
					}
					else
					{
						this.m_indexTable.Add(obj.Key, (int)this.m_writer.BaseStream.Position);
					}
					this.WriteObject(this.m_writer, obj.Value, obj.Value.GetType());
				}
				this.WriteIndexTable();
				this.WriteClassesDefinition();
				this.m_writer.WriteBytes(dataSearchTable);
				this.m_writer.Dispose();
			}
		}

		private D2OFieldType GetIdByType(FieldInfo field)
		{
			D2OFieldType d2OFieldType;
			Type fieldType = field.FieldType;
			d2OFieldType = (field.GetCustomAttribute<I18NFieldAttribute>() == null ? this.GetIdByType(fieldType) : D2OFieldType.I18N);
			return d2OFieldType;
		}

		private D2OFieldType GetIdByType(PropertyInfo property)
		{
			D2OFieldType d2OFieldType;
			Type fieldType = property.PropertyType;
			d2OFieldType = (property.GetCustomAttribute<I18NFieldAttribute>() == null ? this.GetIdByType(fieldType) : D2OFieldType.I18N);
			return d2OFieldType;
		}

		private D2OFieldType GetIdByType(Type fieldType)
		{
			D2OFieldType item;
			if (fieldType == typeof(int))
			{
				item = D2OFieldType.Int;
			}
			else if (fieldType == typeof(bool))
			{
				item = D2OFieldType.Bool;
			}
			else if (fieldType == typeof(string))
			{
				item = D2OFieldType.String;
			}
			else if ((fieldType == typeof(double) ? true : fieldType == typeof(float)))
			{
				item = D2OFieldType.Double;
			}
			else if (fieldType == typeof(uint))
			{
				item = D2OFieldType.UInt;
			}
			else if ((!fieldType.IsGenericType ? true : fieldType.GetGenericTypeDefinition() != typeof(List<>)))
			{
				if (!this.m_allocatedClassId.ContainsKey(fieldType))
				{
					this.AllocateClassId(fieldType);
				}
				item = (D2OFieldType)this.m_allocatedClassId[fieldType];
			}
			else
			{
				item = D2OFieldType.List;
			}
			return item;
		}

		private Tuple<D2OFieldType, Type>[] GetVectorTypes(Type vectorType)
		{
			List<Tuple<D2OFieldType, Type>> ids = new List<Tuple<D2OFieldType, Type>>();
			if (vectorType.IsGenericType)
			{
				Type currentGenericType = vectorType;
				for (Type[] genericArguments = currentGenericType.GetGenericArguments(); genericArguments.Length != 0; genericArguments = currentGenericType.GetGenericArguments())
				{
					ids.Add(Tuple.Create<D2OFieldType, Type>(this.GetIdByType(genericArguments[0]), currentGenericType));
					currentGenericType = genericArguments[0];
				}
			}
			return ids.ToArray();
		}

		private bool IsClassDeclared(Type classType)
		{
			return this.m_allocatedClassId.ContainsKey(classType);
		}

		private void OpenWrite(bool readDefinitionsOnly = false)
		{
			this.ResetMembersByReading(readDefinitionsOnly);
		}

		private void ResetMembersByReading(bool readDefinitionsOnly = false)
		{
			D2OReader reader = new D2OReader(File.OpenRead(this.Filename), true);
			this.m_indexTable = (readDefinitionsOnly ? new Dictionary<int, int>() : reader.Indexes);
			this.m_classes = reader.Classes;
			this.m_allocatedClassId = this.m_classes.ToDictionary<KeyValuePair<int, D2OClassDefinition>, Type, int>((KeyValuePair<int, D2OClassDefinition> entry) => entry.Value.ClassType, (KeyValuePair<int, D2OClassDefinition> entry) => entry.Key);
			this.m_objects = (readDefinitionsOnly ? new Dictionary<int, object>() : reader.ReadObjects(false, false));
			reader.Close();
		}

		public void ResetObjects()
		{
			this.m_objects = new Dictionary<int, object>();
		}

		public void StartWriting(bool backupFile = true)
		{
			try
			{
				if (File.Exists(this.Filename))
				{
					if (backupFile)
					{
						this.BakFilename = string.Concat(this.Filename, ".bak");
						File.Copy(this.Filename, this.BakFilename, true);
					}
					File.Delete(this.Filename);
				}
			}
			catch
			{
				throw new Exception("File already used");
			}

			this.m_writer = new BigEndianWriter(File.Create(this.Filename));
			this.m_writing = true;

			lock (this.m_writingSync)
			{
				if (this.m_needToBeSync)
				{
					this.ResetMembersByReading(false);
				}
			}
		}

        public void Write(object obj, Type type, int index)
        {
            var settings = new JsonSerializerSettings
            {
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                TypeNameHandling = TypeNameHandling.All
            };

            string strJson = JsonConvert.SerializeObject(obj, settings);

            if (!this.m_writing)
            {
                this.StartWriting(true);
            }

            lock (this.m_writingSync)
            {
                this.m_needToBeSync = true;

                if (!this.IsClassDeclared(type))
                {
                    this.DefineClassDefinition(type);
                }

                if (!this.m_objects.ContainsKey(index))
                {
                    this.m_objects.Add(index, obj);
                }
                else
                {
                    this.m_objects[index] = obj;
                }
            }
        }

        public void Write(object obj, Type type)
		{
			this.Write(obj, type, (this.m_objects.Count > 0 ? this.m_objects.Keys.Max() + 1 : 1));
		}

		public void Write(object obj, int index)
		{
			this.Write(obj, obj.GetType(), index);
		}

		public void Write(object obj)
		{
			this.Write(obj, obj.GetType());
		}

		public void Write<T>(T obj)
		{
			this.Write(obj, typeof(T));
		}

		public void Write<T>(T obj, int index)
		{
			this.Write(obj, typeof(T), index);
		}

		private void WriteClassesDefinition()
		{
			this.m_writer.WriteInt(this.m_classes.Count);
			foreach (D2OClassDefinition classDefinition in this.m_classes.Values)
			{
				classDefinition.Offset = (long)((int)this.m_writer.BaseStream.Position);
				this.m_writer.WriteInt(classDefinition.Id);
				this.m_writer.WriteUTF(classDefinition.Name);
				this.m_writer.WriteUTF(classDefinition.PackageName);
				this.m_writer.WriteInt(classDefinition.Fields.Count);
				foreach (D2OFieldDefinition field in classDefinition.Fields.Values)
				{
					field.Offset = (long)((int)this.m_writer.BaseStream.Position);
					this.m_writer.WriteUTF(field.Name);
					this.m_writer.WriteInt((int)field.TypeId);
					Tuple<D2OFieldType, Type>[] vectorTypes = field.VectorTypes;
					for (int i = 0; i < (int)vectorTypes.Length; i++)
					{
						Tuple<D2OFieldType, Type> vectorType = vectorTypes[i];
						this.m_writer.WriteUTF(this.ConvertNETTypeToAS3(vectorType.Item2));
						this.m_writer.WriteInt((int)vectorType.Item1);
					}
				}
			}
		}

		private void WriteField(IDataWriter writer, D2OFieldType fieldType, D2OFieldDefinition field, dynamic obj, int vectorDimension = 0)
		{
			D2OFieldType d2OFieldType = fieldType;
			if (d2OFieldType == D2OFieldType.List)
			{
				this.WriteFieldVector(writer, field, obj, vectorDimension);
			}
			else
			{
				switch (d2OFieldType)
				{
					case D2OFieldType.UInt:
					{
						D2OWriter.WriteFieldUInt(writer, (uint)obj);
						break;
					}
					case D2OFieldType.I18N:
					{
						D2OWriter.WriteFieldI18n(writer, (int)obj);
						break;
					}
					case D2OFieldType.Double:
					{
						WriteFieldDouble(writer, obj);
						break;
					}
					case D2OFieldType.String:
					{
						WriteFieldUTF(writer, obj);
						break;
					}
					case D2OFieldType.Bool:
					{
						WriteFieldBool(writer, obj);
						break;
					}
					case D2OFieldType.Int:
					{
						if (!(obj is string))
						{
							D2OWriter.WriteFieldInt(writer, (int)obj);
						}
						else
						{
							WriteFieldInt(writer, int.Parse(obj));
						}
						break;
					}
					default:
					{
						this.WriteFieldObject(writer, obj);
						break;
					}
				}
			}
		}

		private static void WriteFieldBool(IDataWriter writer, bool value)
		{
			writer.WriteBoolean(value);
		}

		private static void WriteFieldDouble(IDataWriter writer, double value)
		{
			writer.WriteDouble(value);
		}

		private static void WriteFieldI18n(IDataWriter writer, int value)
		{
			writer.WriteInt(value);
		}

		private static void WriteFieldInt(IDataWriter writer, int value)
		{
			writer.WriteInt(value);
		}

		private void WriteFieldObject(IDataWriter writer, object obj)
		{
			if (obj != null)
			{
				if (!this.m_allocatedClassId.ContainsKey(obj.GetType()))
				{
					this.DefineClassDefinition(obj.GetType());
				}
				this.WriteObject(writer, obj, obj.GetType());
			}
			else
			{
				writer.WriteInt(-1431655766);
			}
		}

		private static void WriteFieldUInt(IDataWriter writer, uint value)
		{
			writer.WriteUInt(value);
		}

		private static void WriteFieldUTF(IDataWriter writer, string value)
		{
			if (value == null)
			{
				value = string.Empty;
			}
			writer.WriteUTF(value);
		}

		private void WriteFieldVector(IDataWriter writer, D2OFieldDefinition field, IList list, int vectorDimension)
		{
			if (list != null)
			{
				writer.WriteInt(list.Count);
				for (int i = 0; i < list.Count; i++)
				{
					this.WriteField(writer, field.VectorTypes[vectorDimension].Item1, field, list[i], vectorDimension + 1);
				}
			}
			else
			{
				writer.WriteInt(0);
			}
		}

		private void WriteHeader()
		{
			this.m_writer.WriteUTFBytes("D2O");
			this.m_writer.WriteInt(0);
		}

		private void WriteIndexTable()
		{
			int offset = (int)this.m_writer.BaseStream.Position;
			this.m_writer.Seek(3, SeekOrigin.Begin);
			this.m_writer.WriteInt(offset);
			this.m_writer.Seek(offset, SeekOrigin.Begin);
			this.m_writer.WriteInt(this.m_indexTable.Count * 8);
			foreach (KeyValuePair<int, int> index in this.m_indexTable)
			{
				this.m_writer.WriteInt(index.Key);
				this.m_writer.WriteInt(index.Value);
			}
		}

		private void WriteObject(IDataWriter writer, object obj, Type type)
		{
			if (!this.m_allocatedClassId.ContainsKey(obj.GetType()))
			{
				this.DefineClassDefinition(obj.GetType());
			}
			D2OClassDefinition @class = this.m_classes[this.m_allocatedClassId[type]];
			writer.WriteInt(@class.Id);
			foreach (KeyValuePair<string, D2OFieldDefinition> field in @class.Fields)
			{
				object fieldValue = field.Value.GetValue(obj);
				this.WriteField(writer, field.Value.TypeId, field.Value, fieldValue, 0);
			}
		}

		private void WriteSearchTable()
		{
			long positionBefore = this.m_writer.BaseStream.Position;
			this.m_writer.WriteInt(0);
			long positionFirst = this.m_writer.BaseStream.Position;
			Dictionary<string, Tuple<long, long, int, long>> index = new Dictionary<string, Tuple<long, long, int, long>>();
			foreach (KeyValuePair<string, D2OFieldDefinition> field in this.m_classes.Values.First<D2OClassDefinition>().Fields)
			{
				this.m_writer.WriteUTF(field.Key);
				index.Add(field.Value.Name, new Tuple<long, long, int, long>(this.m_writer.BaseStream.Position, (long)0, 0, (long)0));
				this.m_writer.WriteInt(0);
				this.m_writer.WriteInt((int)field.Value.TypeId);
				index[field.Value.Name] = new Tuple<long, long, int, long>(index[field.Value.Name].Item1, (long)0, 0, this.m_writer.BaseStream.Position);
				this.m_writer.WriteInt(0);
			}
			long lastPosition = this.m_writer.BaseStream.Position;
			this.m_writer.WriteInt(1);
			long initPosData = this.m_writer.BaseStream.Position;
			foreach (KeyValuePair<string, D2OFieldDefinition> keyValuePair in this.m_classes.Values.First<D2OClassDefinition>().Fields)
			{
				IEnumerable<IGrouping<object, object>> groups = 
					from x in this.m_objects.Values
					group x by x.GetType().GetField(keyValuePair.Value.Name).GetValue(x);
				index[keyValuePair.Key] = new Tuple<long, long, int, long>(index[keyValuePair.Value.Name].Item1, this.m_writer.BaseStream.Position - initPosData, groups.Count<IGrouping<object, object>>(), index[keyValuePair.Value.Name].Item4);
				foreach (IGrouping<object, object> group in groups)
				{
					object value = group.First<object>().GetType().GetField(keyValuePair.Value.Name).GetValue(group.First<object>());
					this.WriteField(this.m_writer, keyValuePair.Value.TypeId, keyValuePair.Value, value, 0);
					int count = group.Count<object>();
					this.m_writer.WriteInt(count * 4);
					foreach (object element in group)
					{
						try
						{

							object id = element.GetType().GetField("id").GetValue(element);
							this.m_writer.WriteInt(Convert.ToInt32(id));
						}
						catch (Exception e)
						{
							// ignored
						}
					}
				}
			}
			foreach (KeyValuePair<string, Tuple<long, long, int, long>> obj in index)
			{
				this.m_writer.Seek((int)obj.Value.Item1, SeekOrigin.Begin);
				this.m_writer.WriteInt((int)obj.Value.Item2);
				this.m_writer.Seek((int)obj.Value.Item4, SeekOrigin.Begin);
				this.m_writer.WriteInt(obj.Value.Item3);
			}
			this.m_writer.Seek((int)positionBefore, SeekOrigin.Begin);
			this.m_writer.WriteInt((int)(lastPosition - positionFirst));
		}
	}
}