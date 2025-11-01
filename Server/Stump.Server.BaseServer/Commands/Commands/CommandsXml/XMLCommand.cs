using Stump.DofusProtocol.Enums;
using System.Xml.Linq;
using System.Linq;
using System;
using System.IO;

namespace Stump.Server.BaseServer.Commands.Commands
{
    public class XMLCommand : SubCommandContainer
    {
        public XMLCommand()
        {
            Aliases = new[] { "xml" };
            Description = "Configurações do XML";
            RequiredRole = RoleEnum.Administrator;
        }
    }

    #region >> LogPacks
    public class ActiveLogPacketsCommand : SubCommand
    {
        const string xmlfile = @"./world_config.xml";

        public ActiveLogPacketsCommand()
        {
            ParentCommandType = typeof(XMLCommand);
            Aliases = new[] { "logpackets" };
            RequiredRole = RoleEnum.Administrator;
            AddParameter<bool>("value", "v", "Valor de ativação", isOptional: false);
        }

        public override void Execute(TriggerBase trigger)
        {
            var xdoc = XDocument.Load(xmlfile);
            var TGT = xdoc.Root.Descendants("Variable").FirstOrDefault(x => x.Attribute("name").Value == "LogPackets");
            bool value = trigger.Get<bool>("value");

            if (xdoc == null)
            {
                trigger.Reply("Documento XML não existe.");
                return;
            }
            if (TGT == null)
            {
                trigger.Reply("Variavel não existe.");
                return;
            }

            TGT.Value = value.ToString();

            xdoc.Save(xmlfile);
            ServerBase.InstanceAsBase.Config.Reload();
            trigger.Reply("O LogPackets foi marcado como [{0}].", value);
        }
    }
    #endregion

    #region >> Active Ogrines
    public class ActiveOgrinesCommand : SubCommand
    {
        const string xmlfile = @"./world_config.xml";

        public ActiveOgrinesCommand()
        {
            ParentCommandType = typeof(XMLCommand);
            Aliases = new[] { "blockogrines" };
            RequiredRole = RoleEnum.Administrator;
            AddParameter<bool>("value", "v", "Valor de ativação", isOptional: false);
        }

        public override void Execute(TriggerBase trigger)
        {
            var xdoc = XDocument.Load(xmlfile);
            var TGT = xdoc.Root.Descendants("Variable").FirstOrDefault(x => x.Attribute("name").Value == "BlockTempOgrines");
            bool value = trigger.Get<bool>("value");

            if (xdoc == null)
            {
                trigger.Reply("Documento XML não existe.");
                return;
            }
            if (TGT == null)
            {
                trigger.Reply("Variavel não existe.");
                return;
            }

            TGT.Value = value.ToString();

            xdoc.Save(xmlfile);
            ServerBase.InstanceAsBase.Config.Reload();
            trigger.Reply("O Bloqueio de Ogrines foi marcado como [{0}].", value);
        }
    }
    #endregion

    #region >> Anuncios
    public class Announce2xNpcCommand : SubCommand
    {
        const string xmlfile = @"./world_config.xml";

        public Announce2xNpcCommand()
        {
            ParentCommandType = typeof(XMLCommand);
            Aliases = new[] { "announce2x" };
            RequiredRole = RoleEnum.Administrator;
            AddParameter<bool>("value", "v", "Valor de ativação", isOptional: false);
        }

        public override void Execute(TriggerBase trigger)
        {
            var xdoc = XDocument.Load(xmlfile);
            var TGT = xdoc.Root.Descendants("Variable").FirstOrDefault(x => x.Attribute("name").Value == "Ogrines2xAnnounce");
            bool value = trigger.Get<bool>("value");

            if (xdoc == null)
            {
                trigger.Reply("Documento XML não existe.");
                return;
            }
            if (TGT == null)
            {
                trigger.Reply("Variavel não existe.");
                return;
            }

            TGT.Value = value.ToString();

            xdoc.Save(xmlfile);
            ServerBase.InstanceAsBase.Config.Reload();
            trigger.Reply("O Announce2x foi marcado como [{0}].", value);
        }
    }
    #endregion

    #region >> NPC Effects
    public class ActiveExosNpcCommand : SubCommand
    {
        private const string xmlFilePath = @"./world_config.xml";

        public ActiveExosNpcCommand()
        {
            ParentCommandType = typeof(XMLCommand);
            Aliases = new[] { "exosnpceffects" };
            RequiredRole = RoleEnum.Administrator;
            AddParameter<bool>("value", "v", "Valor de ativação", isOptional: false);
        }

        public override void Execute(TriggerBase trigger)
        {
            var xdoc = XDocument.Load(xmlFilePath);
            var npcExoEffectsVariable = xdoc.Root.Descendants("Variable").FirstOrDefault(x => x.Attribute("name").Value == "NpcExoEffects");
            bool value = trigger.Get<bool>("value");

            if (xdoc == null)
            {
                trigger.Reply("Documento XML não existe.");
                return;
            }
            if (npcExoEffectsVariable == null)
            {
                trigger.Reply("Variável NpcExoEffects não existe.");
                return;
            }

            npcExoEffectsVariable.Value = value.ToString();

            xdoc.Save(xmlFilePath);
            ServerBase.InstanceAsBase.Config.Reload();
            trigger.Reply("A variável NpcExoEffects foi marcada como [{0}].", value);
        }
    }
    #endregion

    #region >> Comandos para aberturas de Eventos no Servidor
    public class EventPegaPegaCommand : SubCommand
    {
        private const string xmlfile = @"./world_config.xml";
        private const string PegaPegaVariableName = "PegaPega";
        private const string PegaPegaNpcVariableName = "PegaPegaNpc";

        public EventPegaPegaCommand()
        {
            ParentCommandType = typeof(XMLCommand);
            Aliases = new[] { "pegapega" };
            RequiredRole = RoleEnum.Administrator;
            AddParameter<bool>("value", "v", "Valor de ativação", isOptional: false);
        }

        public override void Execute(TriggerBase trigger)
        {
            if (!File.Exists(xmlfile))
            {
                trigger.Reply("Documento XML não existe.");
                return;
            }

            var xdoc = XDocument.Load(xmlfile);
            var TGT = xdoc.Root.Descendants("Variable").FirstOrDefault(x => x.Attribute("name").Value == PegaPegaVariableName);
            var TGTNpc = xdoc.Root.Descendants("Variable").FirstOrDefault(x => x.Attribute("name").Value == PegaPegaNpcVariableName);

            if (TGT == null)
            {
                trigger.Reply($"Variável '{PegaPegaVariableName}' não existe.");
                return;
            }

            bool value = trigger.Get<bool>("value");
            TGT.Value = value.ToString();
            TGTNpc.Value = value.ToString();
            xdoc.Save(xmlfile);
            ServerBase.InstanceAsBase.Config.Reload();
            trigger.Reply($"O evento Pega-Pega foi marcado como [{value}]");
        }
    }

    public class EventCampCommand : SubCommand
    {
        private const string xmlfile = @"./world_config.xml";
        private const string CampVariableName = "CampKoliseu";

        public EventCampCommand()
        {
            ParentCommandType = typeof(XMLCommand);
            Aliases = new[] { "CampKoliseu" };
            RequiredRole = RoleEnum.Developer;
            AddParameter<bool>("value", "v", "Valor de ativação", isOptional: false);
        }

        public override void Execute(TriggerBase trigger)
        {
            if (!File.Exists(xmlfile))
            {
                trigger.Reply("Documento XML não existe.");
                return;
            }

            var xdoc = XDocument.Load(xmlfile);
            var TGT = xdoc.Root.Descendants("Variable").FirstOrDefault(x => x.Attribute("name").Value == CampVariableName);

            if (TGT == null)
            {
                trigger.Reply($"Variável '{CampVariableName}' não existe.");
                return;
            }

            bool value = trigger.Get<bool>("value");
            TGT.Value = value.ToString();
            xdoc.Save(xmlfile);
            ServerBase.InstanceAsBase.Config.Reload();
            trigger.Reply($"O evento Campeonato foi marcado como [{value}]");
        }
    }
    #endregion

    #region >> Comando para abertura e fechamento Npc PegaPega
    public class EventPegaPegaNpcCommand : SubCommand
    {
        const string xmlfile = "./world_config.xml";

        public EventPegaPegaNpcCommand()
        {
            ParentCommandType = typeof(XMLCommand);
            Aliases = new[] { "pegapeganpc" };
            RequiredRole = RoleEnum.Administrator;
            AddParameter<bool>("value", "v", "Valor de ativação", isOptional: false);
        }

        public override void Execute(TriggerBase trigger)
        {
            if (!File.Exists(xmlfile))
            {
                trigger.Reply("O arquivo XML não existe.");
                return;
            }

            var xdoc = XDocument.Load(xmlfile);
            var TGT = xdoc.Root.Descendants("Variable").SingleOrDefault(x => x.Attribute("name").Value == "PegaPegaNpc");
            bool value = trigger.Get<bool>("value");

            if (TGT == null)
            {
                trigger.Reply("A variável não existe.");
                return;
            }

            TGT.Value = value.ToString();

            try
            {
                xdoc.Save(xmlfile);
                trigger.Reply($"O NPC do Evento Pega-Pega foi marcado como [{value}].");
                ServerBase.InstanceAsBase.Config.Reload();
            }
            catch (Exception ex)
            {
                trigger.Reply($"Ocorreu um erro ao salvar o arquivo XML: {ex.Message}");
            }
        }
    }
    #endregion
}