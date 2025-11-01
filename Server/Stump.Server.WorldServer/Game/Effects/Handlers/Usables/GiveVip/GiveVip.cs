using System;
using System.Globalization;
using System.Linq;
using MongoDB.Bson;
using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.IPC.Messages;
using Stump.Server.BaseServer.Logging;
using Stump.Server.WorldServer.Core.IPC;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Handlers.Characters;
using Stump.Server.WorldServer.Handlers.Context.RolePlay;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
{
    [EffectHandler(EffectsEnum.Effect_AddVip)]
    public class GiveVip : UsableEffectHandler
    {
        public GiveVip(EffectBase effect, Character target, BasePlayerItem item) : base(effect, target, item)
        { }

        protected override bool InternalApply()
        {
            var effectInt = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

            if (effectInt == null)
                return false;

            if (!IPCAccessor.Instance.IsConnected || World.Instance.GetWorldStatus() == ServerStatusEnum.SAVING)
            {
                Target.SendServerMessageLang(
                    "Você não pode adicionar VIP a sua conta no momento, por favor, tente novamente mais tarde.",
                    "You cannot add VIP to your account at the moment, please try again later.",
                    "No puede agregar VIP a su cuenta en este momento, intente nuevamente más tarde.",
                    "Vous ne pouvez pas ajouter de VIP à votre compte pour le moment, veuillez réessayer plus tard.");

                return false;
            }

            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                var amount = (int)(effectInt.Value * NumberOfUses);
                int UserGroupId = Target.Client.Account.UserGroupId;

                DateTime MongoSubscribeTargetEnd = Target.Account.SubscriptionEndDate;
                DateTime SubscribeTargetEnd = Target.Account.SubscriptionEndDate;
                DateTime SubscribeTargetEndFinal = DateTime.Now;

                if (Target.Account.IsSubscribe)
                {
                    SubscribeTargetEndFinal = SubscribeTargetEnd.AddDays(amount);
                }
                else
                {
                    SubscribeTargetEnd = DateTime.Now;
                    SubscribeTargetEndFinal = SubscribeTargetEnd.AddDays(amount);
                }

                characterRefresh(SubscribeTargetEndFinal);

                if (UserGroupId <= 1)
                {
                    UserGroupId = 2;
                }

                IPCAccessor.Instance.Send(new UpdateVipAccountMessage(Target.Client.Account, UserGroupId, SubscribeTargetEndFinal, Target.Account.GoldSubscriptionEndDate, true));
                Target.SaveLater();
                Target.RefreshActor();

                #region // ----------------- Sistema de Logs MongoDB Adição de VIP by: Kenshin ---------------- //
                try
                {
                    var CharacterRank = "Player";

                    if (Target.Client.Account.UserGroupId >= 4 && Target.Client.Account.UserGroupId <= 9)
                        CharacterRank = "Staff";

                    var document = new BsonDocument
                        {
                          { "AccountUserGroup", CharacterRank },
                          { "AccountId", Target.Account.Id },
                          { "AccountName", Target.Account.Login },
                          { "CharacterId", Target.Id },
                          { "CharacterName", Target.Name },
                          { "Status", "Create" },
                          { "AmountDays", amount },
                          { "AfterDate", MongoSubscribeTargetEnd },
                          { "BeforeDate", SubscribeTargetEndFinal },
                          { "GoldVIPDate", Target.Account.GoldSubscriptionEndDate },
                          { "HardwareId", Target.Client.Account.LastHardwareId },
                          { "Date", DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("pt-BR")) }
                        };

                    MongoLogger.Instance.Insert("Player_Vips", document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erro no Mongologs das Create VIP : " + e.Message);
                }
                #endregion

                Target.SendServerMessageLang(
                    $"Você adicionou a sua conta {amount} dias de VIP.",
                    $"You have added {amount} VIP days to your account.",
                    $"Ha agregado {amount} días VIP a su cuenta.",
                    $"Vous avez ajouté {amount} jours VIP à votre compte.");
            });

            UsedItems = NumberOfUses;

            return true;
        }

        private void characterRefresh(DateTime endSubscríption)
        {
            var findClient = WorldServer.Instance.FindClients(client => client.Account != null && client.Account == Target.Account && client.Connected).FirstOrDefault();

            findClient.Account.m_UserGroupId = 2;
            findClient.UserGroup.Role = RoleEnum.Vip;

            Target.Client.Send(new AccountInformationsUpdateMessage(endSubscríption.GetUnixTimeStampDouble()));
        }
    }
}