using System;
using Handlers.Spouse;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System.Drawing;
using Stump.Server.WorldServer.Handlers.Context;
using System.Threading.Tasks;
using NLog;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    [Discriminator("Weeding", typeof(NpcReply), new System.Type[]
    {
        typeof(NpcReplyRecord)
    })]
    public class WeedingReply : NpcReply
    {
      //  private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private ObjectPosition m_position;
        public uint MapId
        {
            get
            {
                return base.Record.GetParameter<uint>(0u, false);
            }
            set
            {
                base.Record.SetParameter<uint>(0u, value);
            }
        }
        public int CellId
        {
            get
            {
                return base.Record.GetParameter<int>(1u, false);
            }
            set
            {
                base.Record.SetParameter<int>(1u, value);
            }
        }
        public DirectionsEnum Direction
        {
            get
            {
                return (DirectionsEnum)base.Record.GetParameter<int>(2u, false);
            }
            set
            {
                base.Record.SetParameter<int>(2u, (int)value);
            }
        }
        public WeedingReply()
        {
            base.Record.Type = "Weeding";
        }
        public WeedingReply(NpcReplyRecord record)
            : base(record)
        {
        }
        private void RefreshPosition()
        {
            Map map = Singleton<Game.World>.Instance.GetMap(this.MapId);
            if (map == null)
            {
                throw new System.Exception(string.Format("Cannot load SkillTeleport id={0}, map {1} isn't found", base.Id, this.MapId));
            }
            Cell cell = map.Cells[this.CellId];
            this.m_position = new ObjectPosition(map, cell, this.Direction);
        }
        public ObjectPosition GetPosition()
        {
            if (this.m_position == null)
            {
                this.RefreshPosition();
            }

            return this.m_position;
        }
        public override bool Execute(Npc npc, Character character)
        {
            bool flag = true;
            if (character.CurrentSpouse != 0)               
            switch (character.Account.Lang)
            {
                case "fr":
                        character.SendServerMessage("Vous pouvez pas vous marier car vous êtes déjà marié.", Color.Red);
                        break;
                case "es":
                        character.SendServerMessage("Usted no puedes casarte porque usted ya estás casado.", Color.Red);
                        break;
                case "en":
                        character.SendServerMessage("You can not get married because you are already married.", Color.Red);
                        break;
                default:
                        character.SendServerMessage("Você não pode se casar porque você já é casado.", Color.Red);
                        break;
            }
            //
            else
            {
                Character Spouse = null;
                if (character.Cell.Id == 358 || character.Cell.Id == 329)
                {
                    int SpouseCount = 0;
                    foreach (var SpouseInCell in character.Map.GetAllCharacters())
                    {
                        if (!SpouseInCell.IsFighting() && SpouseInCell != character && (SpouseInCell.Cell.Id == 329 || SpouseInCell.Cell.Id == 358))
                        {
                            SpouseCount++;
                            if (SpouseInCell.Cell.Id == character.Cell.Id)
                            {
                               
                                switch (character.Account.Lang)
                                {
                                    case "fr":
                                        character.SendServerMessage("Il y a une personne dans la même cellule que vous.", Color.Red);
                                        break;
                                    case "es":
                                        character.SendServerMessage("Hay una persona en la misma celda que tú.", Color.Red);
                                        break;
                                    case "en":
                                        character.SendServerMessage("There is a person in the same cell as you.", Color.Red);
                                        break;
                                    default:
                                        character.SendServerMessage("Há uma pessoa na mesma célula que você.", Color.Red);
                                        break;
                                }
                                flag = false;
                                break;
                            }
                            if (SpouseCount > 1)
                            {
                                
                                switch (character.Account.Lang)
                                {
                                    case "fr":
                                        character.SendServerMessage("Il y a deux personnes dans la seconde cellule.", Color.Red);
                                        break;
                                    case "es":
                                        character.SendServerMessage("Hay dos personas en la segunda celda.", Color.Red);
                                        break;
                                    case "en":
                                        character.SendServerMessage("There are two people in the second cell.", Color.Red);
                                        break;
                                    default:
                                        character.SendServerMessage("Existem duas pessoas na segunda célula.", Color.Red);
                                        break;
                                }
                                flag = false;
                                break;
                            }
                            Spouse = SpouseInCell;
                        }
                    }
                }
                else
                {
                   
                    switch (character.Account.Lang)
                    {
                        case "fr":
                            character.SendServerMessage("Vous devez vous placer sur la cellule correspondante à votre genre.", Color.Red);
                            break;
                        case "es":
                            character.SendServerMessage("You must place yourself on the cell corresponding to your gender.", Color.Red);
                            break;
                        case "en":
                            character.SendServerMessage("Debes colocarte en la celda correspondiente a tu género.", Color.Red);
                            break;
                        default:
                            character.SendServerMessage("Você deve se colocar na cela correspondente ao seu gênero.", Color.Red);
                            break;
                    }
                    flag = false;
                }
                if (flag)
                    if (Spouse != null)
                        if (Spouse.CurrentSpouse == 0)
                            Marier(character, Spouse);
                        else                           
                switch (character.Account.Lang)
                {
                    case "fr":
                    character.SendServerMessage("Vous ne pouvez pas vous marier avec cette personne parce que elle est déjà mariée !", Color.Red);
                     break;
                    case "es":
                     character.SendServerMessage("¡Usted no puedes casarte con esta persona porque ya está casada !", Color.Red);
                     break;
                    case "en":
                    character.SendServerMessage("You can not get married to this person because she is already married !", Color.Red);
                     break;
                    default:
                    character.SendServerMessage("Você não pode se casar com essa pessoa porque ela já é casada !", Color.Red);
                      break;
                }
                    else                        
                switch (character.Account.Lang)
                {
                    case "fr":
                                character.SendServerMessage("Il n'y a personne dans la seconde cellule. Trouvez-vous quelqu'un !", Color.Red);
                                break;
                    case "es":
                                character.SendServerMessage("No hay nadie en la segunda celda. ¡Encuentre a alguien!", Color.Red);
                                break;
                    case "en":
                                character.SendServerMessage("There is no one in the second cell. Find someone !", Color.Red);
                                break;
                    default:
                                character.SendServerMessage("Não há ninguém na segunda célula. Encontre alguém !", Color.Red);
                                break;
                }
            }
            return flag;
        }

        public async Task Marier(Character character, Character Spouse)
        {
            int attendre = 0;
            if (character.Cell.Id == 358)
            {
                character.Direction = DirectionsEnum.DIRECTION_NORTH_WEST;
                ContextHandler.SendGameMapChangeOrientationMessage(character.Client.Character.CharacterContainer.Clients, character);
                Spouse.Direction = DirectionsEnum.DIRECTION_SOUTH_EAST;
                ContextHandler.SendGameMapChangeOrientationMessage(Spouse.Client.Character.CharacterContainer.Clients, Spouse);
            }
            else
            {
                character.Direction = DirectionsEnum.DIRECTION_SOUTH_EAST;
                ContextHandler.SendGameMapChangeOrientationMessage(character.Client.Character.CharacterContainer.Clients, character);
                Spouse.Direction = DirectionsEnum.DIRECTION_NORTH_WEST;
                ContextHandler.SendGameMapChangeOrientationMessage(Spouse.Client.Character.CharacterContainer.Clients, Spouse);
            }
           
            switch (character.Account.Lang)
            {
                case "fr":
                    character.Say(Spouse.Name + " Tu veux m'épouser ?");
                    break;
                case "es":
                    character.Say("¿" + Spouse.Name + " Usted quieres casarte conmigo ?");
                    break;
                case "en":
                    character.Say(Spouse.Name + " Do you want to marry me ?");
                    break;
                default:
                    character.Say(Spouse.Name + " Você quer casar comigo ?");
                    break;
            }

            await Task.Delay(2500);
          
            switch (Spouse.Account.Lang)
            {
                case "fr":
                    Spouse.Say("Oui bien-sûr !");
                    break;
                case "es":
                    Spouse.Say("¡Si claro !");
                    break;
                case "en":
                    Spouse.Say("Yes of course !");
                    break;
                default:
                    Spouse.Say("Sim claro !");
                    break;
            }
            await Task.Delay(2500);
            character.CurrentSpouse = Spouse.Id;
            Spouse.CurrentSpouse = character.Id;
            
           // logger.Info("save from in queue {0}", "WeedingReply1");
            character.SaveLater();
            //logger.Info("save from in queue {0}", "WeedingReply1");
            Spouse.SaveLater();
            //character.Spouse.AddSpouse(Spouse);
            //Spouse.Spouse.AddSpouse(character);
            //aTimer.Dispose();

            //attendre = 0; while (attendre < 99999999) attendre++;
            character.PlayEmote(EmotesEnum.EMOTE_FAIRE_UN_BISOU, true);
            Spouse.PlayEmote(EmotesEnum.EMOTE_FAIRE_UN_BISOU, true);
            //character.Spouse.SendSpouseStatueInfromation();
            //Spouse.Spouse.SendSpouseStatueInfromation();
            SpouseHandler.SendSpouseInformationMessage(character.Client, Spouse);
            SpouseHandler.SendSpouseInformationMessage(Spouse.Client, character);
            await Task.Delay(1500);
            foreach (var AllCharacterInMap in Game.World.Instance.GetCharacters(x => x.Map == character.Map && x.Id != character.Id && x.Id != Spouse.Id))
            {
                AllCharacterInMap.Direction = DirectionsEnum.DIRECTION_NORTH_EAST;
                ContextHandler.SendGameMapChangeOrientationMessage(AllCharacterInMap.Client.Character.CharacterContainer.Clients, AllCharacterInMap);
               
                switch (AllCharacterInMap.Account.Lang)
                {
                    case "fr":
                        AllCharacterInMap.Say("Félicitation");
                        break;
                    case "es":
                        AllCharacterInMap.Say("Felicitación");
                        break;
                    case "en":
                        AllCharacterInMap.Say("Congratulation");
                        break;
                    default:
                        AllCharacterInMap.Say("Parabens");
                        break;
                }
                AllCharacterInMap.PlayEmote(EmotesEnum.EMOTE_APPLAUDIR, true);
            }
            //attendre = 0; while (attendre < 999999999) attendre++;
            //aTimer.Dispose();
            await Task.Delay(1700);
            foreach (var AllCharacterInMap in Game.World.Instance.GetCharacters(x => x.Map == character.Map && x.Id != character.Id && x.Id != Spouse.Id))
                AllCharacterInMap.PlayEmote(EmotesEnum.EMOTE_FLEURS, true);
            await Task.Delay(2500);
            switch (character.Account.Lang)
            {
                case "fr":
                    character.Client.Send(new PopupWarningMessage(2, "Information", "Vous êtes a présent marié avec " + Spouse.Name + " !"));
                    break;
                case "es":
                    character.Client.Send(new PopupWarningMessage(2, "Información", "¡Ahora usted está casado con " + Spouse.Name + " !"));
                    break;
                case "en":
                    character.Client.Send(new PopupWarningMessage(2, "Information", "You are now married to " + Spouse.Name + " !"));
                    break;
                default:
                    character.Client.Send(new PopupWarningMessage(2, "Informação", "Agora você é casado com " + Spouse.Name + " !"));
                    break;
            }          
            switch (Spouse.Account.Lang)
            {
                case "fr":
                    Spouse.Client.Send(new PopupWarningMessage(2, "Information", "Vous êtes a présent marié avec " + character.Name + " !"));
                    break;
                case "es":
                    Spouse.Client.Send(new PopupWarningMessage(2, "Información", "¡Ahora usted está casado con " + character.Name + " !"));
                    break;
                case "en":
                    Spouse.Client.Send(new PopupWarningMessage(2, "Information", "You are now married to " + character.Name + " !"));
                    break;
                default:
                    Spouse.Client.Send(new PopupWarningMessage(2, "Informação", "Agora você é casado com " + character.Name + " !"));
                    break;
            }
            Game.World.Instance.SendAnnounceLang("Parabéns a " + character.Name + " e " + Spouse.Name + " agora estão casados.", "Congratulations to " + character.Name + " and " + Spouse.Name + " they are now married.", "Felicitaciones a " + character.Name + " y " + Spouse.Name + " que ahora están casados.", "Félicitations à " + character.Name + " et " + Spouse.Name + " ils sont maintenant mariés.", Color.Red);
        }
    }
}
