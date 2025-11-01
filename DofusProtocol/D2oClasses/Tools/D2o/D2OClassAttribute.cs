using System;
using System.Runtime.CompilerServices;

namespace Stump.DofusProtocol.D2oClasses.Tools.D2o
{
	public class D2OClassAttribute : Attribute
	{
		public bool AutoBuild
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string PackageName
		{
			get;
			set;
		}

		public D2OClassAttribute(string name, bool autoBuild = true)
		{
			this.Name = name;
			this.AutoBuild = autoBuild;
		}

		public D2OClassAttribute(string name, string packageName, bool autoBuild = true)
		{
			this.Name = name;
			this.PackageName = packageName;
			this.AutoBuild = autoBuild;
		}
	}
}