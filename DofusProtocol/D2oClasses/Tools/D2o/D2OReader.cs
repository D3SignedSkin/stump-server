using Stump.Core.IO;
using Stump.Core.Reflection;
using Stump.DofusProtocol.D2oClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Stump.DofusProtocol.D2oClasses.Tools.D2o
{
	public class D2OReader
	{
		private const int NullIdentifier = -1431655766;

		public static List<Assembly> ClassesContainers;

		private readonly static Dictionary<Type, Func<object[], object>> objectCreators;

		private readonly string m_filePath;

		private bool m_ignoreSearchTable = true;

		private int m_classcount;

		private Dictionary<int, D2OClassDefinition> m_classes;

		private int m_headeroffset;

		private Dictionary<int, int> m_indextable = new Dictionary<int, int>();

		private int m_indextablelen;

		private IDataReader m_reader;

		private int m_contentOffset = 0;

		private byte[] m_searchTablesBin;

		private List<string> m_queryableField = new List<string>();

		private Dictionary<string, int> m_searchFieldIndex = new Dictionary<string, int>();

		private Dictionary<string, int> m_searchFieldType = new Dictionary<string, int>();

		private Dictionary<string, int> m_searchFieldCount = new Dictionary<string, int>();

		public Dictionary<int, D2OClassDefinition> Classes
		{
			get
			{
				return this.m_classes;
			}
		}

		public string FileName
		{
			get
			{
				return Path.GetFileNameWithoutExtension(this.FilePath);
			}
		}

		public string FilePath
		{
			get
			{
				return this.m_filePath;
			}
		}

		public int IndexCount
		{
			get
			{
				return this.m_indextable.Count;
			}
		}

		public Dictionary<int, int> Indexes
		{
			get
			{
				return this.m_indextable;
			}
		}

		public int IndexTableOffset
		{
			get
			{
				return this.m_headeroffset;
			}
		}

		public byte[] SearchTablesBin
		{
			get
			{
				return this.m_searchTablesBin;
			}
		}

		static D2OReader()
		{
			D2OReader.ClassesContainers = new List<Assembly>()
			{
				typeof(Breed).Assembly
			};
			D2OReader.objectCreators = new Dictionary<Type, Func<object[], object>>();
		}

		public D2OReader(string filePath, bool ignoreSearchTable = true) : this(new FastBigEndianReader(File.ReadAllBytes(filePath)), ignoreSearchTable)
		{
			this.m_filePath = filePath;
		}

		public D2OReader(Stream stream, bool ignoreSearchTable = true)
		{
			this.m_reader = new BigEndianReader(stream);
			this.m_ignoreSearchTable = ignoreSearchTable;
			this.Initialize();
		}

		public D2OReader(IDataReader reader, bool ignoreSearchTable = true)
		{
			this.m_reader = reader;
			this.m_ignoreSearchTable = ignoreSearchTable;
			this.Initialize();
		}

		private object BuildObject(D2OClassDefinition classDefinition, IDataReader reader)
		{
			if (!D2OReader.objectCreators.ContainsKey(classDefinition.ClassType))
			{
				Func<object[], object> creator = D2OReader.CreateObjectBuilder(classDefinition.ClassType, (
					from entry in classDefinition.Fields
					select entry.Value.FieldInfo).ToArray<FieldInfo>());
				D2OReader.objectCreators.Add(classDefinition.ClassType, creator);
			}
			List<object> values = new List<object>();
			foreach (D2OFieldDefinition field in classDefinition.Fields.Values)
			{
				object fieldValue = this.ReadField(reader, field, field.TypeId, 0);
				if ((fieldValue == null ? false : !field.FieldType.IsInstanceOfType(fieldValue)))
				{
					if ((!(fieldValue is IConvertible) ? true : field.FieldType.GetInterface("IConvertible") == null))
					{
						throw new Exception(string.Format("Field '{0}.{1}' with value {2} is not of type '{3}'", new object[] { classDefinition.Name, field.Name, fieldValue, fieldValue.GetType() }));
					}
					try
					{
						if ((!(fieldValue is int) || (int)fieldValue >= 0 ? true : field.FieldType != typeof(uint)))
						{
							values.Add(Convert.ChangeType(fieldValue, field.FieldType));
						}
						else
						{
							values.Add((uint)((int)fieldValue));
						}
					}
					catch
					{
						throw new Exception(string.Format("Field '{0}.{1}' with value {2} is not of type '{3}'", new object[] { classDefinition.Name, field.Name, fieldValue, fieldValue.GetType() }));
					}
				}
				else
				{
					values.Add(fieldValue);
				}
			}
			object item = D2OReader.objectCreators[classDefinition.ClassType](values.ToArray());
			return item;
		}

		internal IDataReader CloneReader()
		{
			IDataReader bigEndianReader;
			lock (this.m_reader)
			{
				if (this.m_reader.Position > (long)0)
				{
					this.m_reader.Seek(0, SeekOrigin.Begin);
				}
				if (!(this.m_reader is FastBigEndianReader))
				{
					Stream streamClone = new MemoryStream();
					((BigEndianReader)this.m_reader).BaseStream.CopyTo(streamClone);
					bigEndianReader = new BigEndianReader(streamClone);
				}
				else
				{
					bigEndianReader = new FastBigEndianReader((this.m_reader as FastBigEndianReader).Buffer);
				}
			}
			return bigEndianReader;
		}

		public void Close()
		{
			lock (this.m_reader)
			{
				this.m_reader.Dispose();
			}
		}

		private static Func<object[], object> CreateObjectBuilder(Type classType, params FieldInfo[] fields)
		{
			IEnumerable<Type> fieldsType = 
				from entry in (IEnumerable<FieldInfo>)fields
				select entry.FieldType;
			DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString("N"), typeof(object), ((IEnumerable<Type>)(new Type[] { typeof(object[]) })).ToArray<Type>());
			ILGenerator ilGenerator = method.GetILGenerator();
			ilGenerator.DeclareLocal(classType);
			ilGenerator.DeclareLocal(classType);
			ilGenerator.Emit(OpCodes.Newobj, classType.GetConstructor(Type.EmptyTypes));
			ilGenerator.Emit(OpCodes.Stloc_0);
			for (int i = 0; i < (int)fields.Length; i++)
			{
				ilGenerator.Emit(OpCodes.Ldloc_0);
				ilGenerator.Emit(OpCodes.Ldarg_0);
				ilGenerator.Emit(OpCodes.Ldc_I4, i);
				ilGenerator.Emit(OpCodes.Ldelem_Ref);
				if (!fields[i].FieldType.IsClass)
				{
					ilGenerator.Emit(OpCodes.Unbox_Any, fields[i].FieldType);
				}
				else
				{
					ilGenerator.Emit(OpCodes.Castclass, fields[i].FieldType);
				}
				ilGenerator.Emit(OpCodes.Stfld, fields[i]);
			}
			ilGenerator.Emit(OpCodes.Ldloc_0);
			ilGenerator.Emit(OpCodes.Stloc_1);
			ilGenerator.Emit(OpCodes.Ldloc_1);
			ilGenerator.Emit(OpCodes.Ret);
			Func<object[], object> func = (Func<object[], object>)method.CreateDelegate(Expression.GetFuncType(((IEnumerable<Type>)(new Type[] { typeof(object[]), typeof(object) })).ToArray<Type>()));
			return func;
		}

		public IEnumerable<object> EnumerateObjects(bool cloneReader = false)
		{
			IDataReader dataReader;
			dataReader = (cloneReader ? this.CloneReader() : this.m_reader);
			IDataReader dataReader1 = dataReader;
			foreach (KeyValuePair<int, int> mIndextable in this.m_indextable)
			{
				dataReader1.Seek(mIndextable.Value + this.m_contentOffset, SeekOrigin.Begin);
				yield return this.ReadObject(mIndextable.Key, dataReader1);
			}
			if (cloneReader)
			{
				dataReader1.Dispose();
			}
		}

		public int FindFreeId()
		{
			return this.m_indextable.Keys.Max() + 1;
		}

		private Type FindNETType(string typeName)
		{
			D2OClassDefinition @class;
			Type classType;
			Type[] typeArray;
			Type type;
			string str = typeName;
			if (str != null)
			{
				if (str == "int")
				{
					classType = typeof(int);
					return classType;
				}
				else if (str == "uint")
				{
					classType = typeof(uint);
					return classType;
				}
				else if (str == "Number")
				{
					classType = typeof(double);
					return classType;
				}
				else
				{
					if (str != "String")
					{
						if (!typeName.StartsWith("Vector.<"))
						{
							@class = this.m_classes.Values.FirstOrDefault<D2OClassDefinition>((D2OClassDefinition x) => string.Concat(x.PackageName, "::", x.Name) == typeName);
							if (@class == null)
							{
								throw new Exception(string.Format("Cannot found .NET type associated to {0}", typeName));
							}
							classType = @class.ClassType;
						}
						else
						{
							type = typeof(List<>);
							typeArray = new Type[] { this.FindNETType(typeName.Remove(typeName.Length - 1, 1).Remove(0, "Vector.<".Length)) };
							classType = type.MakeGenericType(typeArray);
						}
						return classType;
					}
					classType = typeof(string);
					return classType;
				}
			}
			if (!typeName.StartsWith("Vector.<"))
			{
				@class = this.m_classes.Values.FirstOrDefault<D2OClassDefinition>((D2OClassDefinition x) => string.Concat(x.PackageName, "::", x.Name) == typeName);
				if (@class == null)
				{
					throw new Exception(string.Format("Cannot found .NET type associated to {0}", typeName));
				}
				classType = @class.ClassType;
			}
			else
			{
				type = typeof(List<>);
				typeArray = new Type[] { this.FindNETType(typeName.Remove(typeName.Length - 1, 1).Remove(0, "Vector.<".Length)) };
				classType = type.MakeGenericType(typeArray);
			}
			return classType;
		}

		private static Type FindType(string className)
		{
			IEnumerable<Type> correspondantTypes = 
				from asm in D2OReader.ClassesContainers
				let types = asm.GetTypes()
				from type in types
				where (!type.Name.Equals(className, StringComparison.InvariantCulture) ? false : type.HasInterface(typeof(IDataObject)))
				select type;
			return correspondantTypes.FirstOrDefault<Type>();
		}

		public D2OClassDefinition GetObjectClass(int index)
		{
			D2OClassDefinition item;
			lock (this.m_reader)
			{
				int offset = this.m_indextable[index];
				this.m_reader.Seek(offset + this.m_contentOffset, SeekOrigin.Begin);
				int classid = this.m_reader.ReadInt();
				item = this.m_classes[classid];
			}
			return item;
		}

		public Dictionary<int, D2OClassDefinition> GetObjectsClasses()
		{
			Dictionary<int, D2OClassDefinition> dictionary = this.m_indextable.ToDictionary<KeyValuePair<int, int>, int, D2OClassDefinition>((KeyValuePair<int, int> index) => index.Key, (KeyValuePair<int, int> index) => this.GetObjectClass(index.Key));
			return dictionary;
		}

		private void Initialize()
		{
			lock (this.m_reader)
			{
				this.ReadHeader();
				this.ReadIndexTable(false);
				this.ReadClassesTable();
				if (!this.m_ignoreSearchTable)
				{
					this.ReadSearchTable();
				}
			}
		}

		private bool IsTypeDefined(Type type)
		{
			bool flag = this.m_classes.Count<KeyValuePair<int, D2OClassDefinition>>((KeyValuePair<int, D2OClassDefinition> entry) => entry.Value.ClassType == type) > 0;
			return flag;
		}

		private void ReadClassesTable()
		{
			Dictionary<D2OFieldDefinition, List<Tuple<D2OFieldType, string>>> tempVectorTypes = new Dictionary<D2OFieldDefinition, List<Tuple<D2OFieldType, string>>>();
			this.m_classcount = this.m_reader.ReadInt();
			this.m_classes = new Dictionary<int, D2OClassDefinition>(this.m_classcount);
			for (int i = 0; i < this.m_classcount; i++)
			{
				int classId = this.m_reader.ReadInt();
				string classMembername = this.m_reader.ReadUTF();
				string classPackagename = this.m_reader.ReadUTF();
				Type classType = D2OReader.FindType(classMembername);
				int fieldscount = this.m_reader.ReadInt();
				List<D2OFieldDefinition> fields = new List<D2OFieldDefinition>(fieldscount);
				for (int l = 0; l < fieldscount; l++)
				{
					string fieldname = this.m_reader.ReadUTF();
					D2OFieldType fieldtype = (D2OFieldType)this.m_reader.ReadInt();
					FieldInfo field = classType.GetField(fieldname);
					if (field == null)
					{
						throw new Exception(string.Format("Missing field '{0}' ({1}) in class '{2}'", fieldname, fieldtype, classType.Name));
					}
					D2OFieldDefinition fieldDefinition = new D2OFieldDefinition(fieldname, fieldtype, field, this.m_reader.Position, new Tuple<D2OFieldType, Type>[0]);
					List<Tuple<D2OFieldType, object>> vectorTypes = new List<Tuple<D2OFieldType, object>>();
					if (fieldtype == D2OFieldType.List)
					{
						while (true)
						{
							string name = this.m_reader.ReadUTF();
							D2OFieldType id = (D2OFieldType)this.m_reader.ReadInt();
							if (!tempVectorTypes.ContainsKey(fieldDefinition))
							{
								tempVectorTypes.Add(fieldDefinition, new List<Tuple<D2OFieldType, string>>());
							}
							tempVectorTypes[fieldDefinition].Add(Tuple.Create<D2OFieldType, string>(id, name));
							if (id != D2OFieldType.List)
							{
								break;
							}
						}
					}
					fields.Add(fieldDefinition);
				}
				this.m_classes.Add(classId, new D2OClassDefinition(classId, classMembername, classPackagename, classType, fields, this.m_reader.Position));
			}
			foreach (KeyValuePair<D2OFieldDefinition, List<Tuple<D2OFieldType, string>>> keyPair in tempVectorTypes)
			{
				keyPair.Key.VectorTypes = (
					from tuple in keyPair.Value
					select Tuple.Create<D2OFieldType, Type>(tuple.Item1, this.FindNETType(tuple.Item2))).ToArray<Tuple<D2OFieldType, Type>>();
			}
		}

		private object ReadField(IDataReader reader, D2OFieldDefinition field, D2OFieldType typeId, int vectorDimension = 0)
		{
			object obj;
			D2OFieldType d2OFieldType = typeId;
			if (d2OFieldType == D2OFieldType.List)
			{
				obj = this.ReadFieldVector(reader, field, vectorDimension);
			}
			else
			{
				switch (d2OFieldType)
				{
					case D2OFieldType.UInt:
					{
						obj = D2OReader.ReadFieldUInt(reader);
						break;
					}
					case D2OFieldType.I18N:
					{
						obj = D2OReader.ReadFieldI18n(reader);
						break;
					}
					case D2OFieldType.Double:
					{
						obj = D2OReader.ReadFieldDouble(reader);
						break;
					}
					case D2OFieldType.String:
					{
						obj = D2OReader.ReadFieldUTF(reader);
						break;
					}
					case D2OFieldType.Bool:
					{
						obj = D2OReader.ReadFieldBool(reader);
						break;
					}
					case D2OFieldType.Int:
					{
						obj = D2OReader.ReadFieldInt(reader);
						break;
					}
					default:
					{
						obj = this.ReadFieldObject(reader);
						break;
					}
				}
			}
			return obj;
		}

		private object ReadField(IDataReader reader, D2OFieldType typeId)
		{
			object obj;
			switch (typeId)
			{
				case D2OFieldType.UInt:
				{
					obj = D2OReader.ReadFieldUInt(reader);
					break;
				}
				case D2OFieldType.I18N:
				{
					obj = D2OReader.ReadFieldI18n(reader);
					break;
				}
				case D2OFieldType.Double:
				{
					obj = D2OReader.ReadFieldDouble(reader);
					break;
				}
				case D2OFieldType.String:
				{
					obj = D2OReader.ReadFieldUTF(reader);
					break;
				}
				case D2OFieldType.Bool:
				{
					obj = D2OReader.ReadFieldBool(reader);
					break;
				}
				case D2OFieldType.Int:
				{
					obj = D2OReader.ReadFieldInt(reader);
					break;
				}
				default:
				{
					obj = this.ReadFieldObject(reader);
					break;
				}
			}
			return obj;
		}

		private static bool ReadFieldBool(IDataReader reader)
		{
			return reader.ReadByte() != 0;
		}

		private static double ReadFieldDouble(IDataReader reader)
		{
			return reader.ReadDouble();
		}

		private static int ReadFieldI18n(IDataReader reader)
		{
			return reader.ReadInt();
		}

		private static int ReadFieldInt(IDataReader reader)
		{
			return reader.ReadInt();
		}

		private object ReadFieldObject(IDataReader reader)
		{
			object obj;
			int classid = reader.ReadInt();
			if (classid != -1431655766)
			{
				obj = (!this.Classes.Keys.Contains<int>(classid) ? null : this.BuildObject(this.Classes[classid], reader));
			}
			else
			{
				obj = null;
			}
			return obj;
		}

		private static uint ReadFieldUInt(IDataReader reader)
		{
			return reader.ReadUInt();
		}

		private static string ReadFieldUTF(IDataReader reader)
		{
			return reader.ReadUTF();
		}

		private object ReadFieldVector(IDataReader reader, D2OFieldDefinition field, int vectorDimension)
		{
			int count = reader.ReadInt();
			Type vectorType = field.FieldType;
			for (int i = 0; i < vectorDimension; i++)
			{
				vectorType = vectorType.GetGenericArguments()[0];
			}
			if (!D2OReader.objectCreators.ContainsKey(vectorType))
			{
				Func<object[], object> creator = D2OReader.CreateObjectBuilder(vectorType, new FieldInfo[0]);
				D2OReader.objectCreators.Add(vectorType, creator);
			}
			IList result = D2OReader.objectCreators[vectorType](new object[0]) as IList;
			for (int i = 0; i < count; i++)
			{
				vectorDimension++;
				object fieldO = this.ReadField(reader, field, field.VectorTypes[vectorDimension - 1].Item1, vectorDimension);
				result.Add(fieldO);
				vectorDimension--;
			}
			return result;
		}

		private void ReadHeader()
		{
			string header = this.m_reader.ReadUTFBytes(3);
			if ((header == "D2O" ? false : header != "ALP"))
			{
				this.m_reader.Seek(0, SeekOrigin.Begin);
				try
				{
					header = this.m_reader.ReadUTF();
				}
				catch (Exception exception)
				{
					throw new Exception(string.Concat("Header doesn't equal the string 'D2O' OR 'AKSF' : Corrupted file ", exception.ToString()));
				}
				if (header != "AKSF")
				{
					throw new Exception("Header doesn't equal the string 'D2O' OR 'AKSF' : Corrupted file");
				}
				this.m_reader.ReadShort();
				int len = this.m_reader.ReadInt();
				this.m_reader.Seek(len, SeekOrigin.Current);
				this.m_contentOffset = (int)this.m_reader.Position;
				header = this.m_reader.ReadUTFBytes(3);
				if (header != "D2O")
				{
					throw new Exception("Header doesn't equal the string 'D2O' : Corrupted file (signed file)");
				}
			}
		}

		private void ReadIndexTable(bool isD2OS = false)
		{
			this.m_headeroffset = this.m_reader.ReadInt();
			this.m_reader.Seek(this.m_contentOffset + this.m_headeroffset, SeekOrigin.Begin);
			this.m_indextablelen = this.m_reader.ReadInt();
			this.m_indextable = new Dictionary<int, int>(this.m_indextablelen / 8);
			for (int i = 0; i < this.m_indextablelen; i += 8)
			{
				this.m_indextable.Add(this.m_reader.ReadInt(), this.m_reader.ReadInt());
			}
		}

		public object ReadObject(int index, bool cloneReader = false)
		{
			object obj;
			if (!cloneReader)
			{
				lock (this.m_reader)
				{
					obj = this.ReadObject(index, this.m_reader);
				}
			}
			else
			{
				using (IDataReader reader = this.CloneReader())
				{
					obj = this.ReadObject(index, reader);
				}
			}
			return obj;
		}

		private object ReadObject(int index, IDataReader reader)
		{
			int offset = this.m_indextable[index];
			reader.Seek(offset + this.m_contentOffset, SeekOrigin.Begin);
			int classid = reader.ReadInt();
			return this.BuildObject(this.m_classes[classid], reader);
		}

		public T ReadObject<T>(int index, bool cloneReader = false)
		where T : class
		{
			T t;
			if (!cloneReader)
			{
				t = this.ReadObject<T>(index, this.m_reader);
			}
			else
			{
				using (IDataReader reader = this.CloneReader())
				{
					t = this.ReadObject<T>(index, reader);
				}
			}
			return t;
		}

		private T ReadObject<T>(int index, IDataReader reader)
		where T : class
		{
			if (!this.IsTypeDefined(typeof(T)))
			{
				throw new Exception("The file doesn't contain this class");
			}
			int offset = 0;
			if (!this.m_indextable.TryGetValue(index, out offset))
			{
				throw new Exception(string.Format("Can't find Index {0} in {1}", index, this.FileName));
			}
			reader.Seek(offset + this.m_contentOffset, SeekOrigin.Begin);
			int classid = reader.ReadInt();
			if ((this.m_classes[classid].ClassType == typeof(T) ? false : !this.m_classes[classid].ClassType.IsSubclassOf(typeof(T))))
			{
				throw new Exception(string.Format("Wrong type, try to read object with {1} instead of {0}", typeof(T).Name, this.m_classes[classid].ClassType.Name));
			}
			T t = (T)(this.BuildObject(this.m_classes[classid], reader) as T);
			return t;
		}

		public Dictionary<int, T> ReadObjects<T>(bool allownulled = false)
		where T : class
		{
			if (!this.IsTypeDefined(typeof(T)))
			{
				throw new Exception("The file doesn't contain this class");
			}
			Dictionary<int, T> result = new Dictionary<int, T>(this.m_indextable.Count);
			using (IDataReader reader = this.CloneReader())
			{
				foreach (KeyValuePair<int, int> index in this.m_indextable)
				{
					reader.Seek(index.Value, SeekOrigin.Begin);
					int classid = reader.ReadInt();
					if ((this.m_classes[classid].ClassType == typeof(T) ? true : this.m_classes[classid].ClassType.IsSubclassOf(typeof(T))))
					{
						try
						{
							result.Add(index.Key, (T)(this.BuildObject(this.m_classes[classid], reader) as T));
						}
						catch
						{
							if (!allownulled)
							{
								throw;
							}
							else
							{
								result.Add(index.Key, default(T));
							}
						}
					}
				}
			}
			return result;
		}

		public Dictionary<int, object> ReadObjects(bool allownulled = false, bool cloneReader = false)
		{
			Dictionary<int, object> result = new Dictionary<int, object>(this.m_indextable.Count);
			IDataReader reader = (cloneReader ? this.CloneReader() : this.m_reader);
			foreach (KeyValuePair<int, int> index in this.m_indextable)
			{
				reader.Seek(index.Value + this.m_contentOffset, SeekOrigin.Begin);
				try
				{
					result.Add(index.Key, this.ReadObject(index.Key, reader));
				}
				catch
				{
					if (!allownulled)
					{
						throw;
					}
					else
					{
						result.Add(index.Key, null);
					}
				}
			}
			if (cloneReader)
			{
				reader.Dispose();
			}
			return result;
		}

		private void ReadSearchTable()
		{
			long positionBefore = this.m_reader.Position;
			this.m_reader.Seek((int)positionBefore, SeekOrigin.Begin);
			this.m_searchTablesBin = this.m_reader.ReadBytes((int)this.m_reader.BytesAvailable);
		}
	}
}