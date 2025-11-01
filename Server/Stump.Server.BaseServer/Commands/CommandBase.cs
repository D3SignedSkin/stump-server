using System.Collections.Generic;
using System.Linq;
using Stump.Core.Attributes;
using Stump.DofusProtocol.Enums;

namespace Stump.Server.BaseServer.Commands
{
    public abstract class CommandBase
    {
        protected CommandBase()
        {
            Parameters = new List<IParameterDefinition>();
        }

        /// <summary>
        /// Enable/Disable case check for server's commands
        /// </summary>
        [Variable]
        public static bool IgnoreCommandCase = true;

        public string[] Aliases
        {
            get;
            protected set;
        }

        public string Usage
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }
       
     
        private string desc_fr;
        public string Description_fr
        {
            get {
                if (desc_fr == null)
                {
                    desc_fr = Description;
                }
                return desc_fr;
            }
            set
            {
                desc_fr = value;
            }
           
        }
        private string desc_es;
        public string Description_es
        {
            get
            {
                if (desc_es == null)
                {
                    desc_es = Description;
                }
                return desc_es;
            }
            set
            {
                desc_es = value;
            }

        }
        private string desc_en;
        public string Description_en
        {
            get
            {
                if (desc_en == null)
                {
                    desc_en = Description;
                }
                return desc_en;
            }
            set
            {
                desc_en = value;
            }

        }
        public RoleEnum RequiredRole
        {
            get;
            set;
        }

        public List<IParameterDefinition> Parameters
        {
            get;
            protected set;
        }
        private List<IParameterDefinition> param_fr;
        public List<IParameterDefinition> Parameters_fr
        {
            get
            {
                if (param_fr == null)
                {
                    param_fr = Parameters;
                }
                return param_fr;
            }
            protected set
            {
                param_fr = value;
            }
             
        }
        private List<IParameterDefinition> param_es;
        public List<IParameterDefinition> Parameters_es
        {
            get
            {
                if (param_es == null)
                {
                    param_es = Parameters;
                }
                return param_es;
            }
            protected set
            {
                param_es = value;
            }

        }
        private List<IParameterDefinition> param_en;
        public List<IParameterDefinition> Parameters_en
        {
            get
            {
                if (param_en == null)
                {
                    param_en = Parameters;
                }
                return param_en;
            }
            protected set
            {
                param_en = value;
            }

        }

        public void AddParameter<T>(string name, string shortName = "", string description = "", T defaultValue = default(T), bool isOptional = false,
                                ConverterHandler<T> converter = null)
        {
            Parameters.Add(new ParameterDefinition<T>(name, shortName, description, defaultValue, isOptional, converter));
        }
        public void AddParameter_fr<T>(string name, string shortName = "", string description_fr = "", T defaultValue = default(T), bool isOptional = false,
                               ConverterHandler<T> converter = null)
        {
            Parameters_fr.Add(new ParameterDefinition<T>(name, shortName, description_fr, defaultValue, isOptional, converter));
        }
        public void AddParameter_es<T>(string name, string shortName = "", string description_es = "", T defaultValue = default(T), bool isOptional = false,
                               ConverterHandler<T> converter = null)
        {
            Parameters_es.Add(new ParameterDefinition<T>(name, shortName, description_es, defaultValue, isOptional, converter));
        }
        public void AddParameter_en<T>(string name, string shortName = "", string description_en = "", T defaultValue = default(T), bool isOptional = false,
                               ConverterHandler<T> converter = null)
        {
            Parameters_en.Add(new ParameterDefinition<T>(name, shortName, description_en, defaultValue, isOptional, converter));
        }

        public string GetSafeUsage()
        {
            if (!string.IsNullOrEmpty(Usage)) return Usage;
            if (Parameters == null)
                return "";

            return string.Join(" ", from entry in Parameters
                select entry.GetUsage());
        }
        public string GetSafeUsage_fr()
        {
            if (!string.IsNullOrEmpty(Usage)) return Usage;
            if (Parameters_fr == null)
                return "";

            return string.Join(" ", from entry in Parameters_fr
                                    select entry.GetUsage());
        }
        public string GetSafeUsage_es()
        {
            if (!string.IsNullOrEmpty(Usage)) return Usage;
            if (Parameters_es == null)
                return "";

            return string.Join(" ", from entry in Parameters_es
                                    select entry.GetUsage());
        }
        public string GetSafeUsage_en()
        {
            if (!string.IsNullOrEmpty(Usage)) return Usage;
            if (Parameters_en == null)
                return "";

            return string.Join(" ", from entry in Parameters_en
                                    select entry.GetUsage());
        }

        public virtual string[] GetFullAliases()
        {
            return Aliases;
        }

        public abstract void Execute(TriggerBase trigger);

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}