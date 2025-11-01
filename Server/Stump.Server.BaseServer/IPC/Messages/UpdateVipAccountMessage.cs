#region License GNU GPL

// UpdateAccountMessage.cs
// 
// Copyright (C) 2013 - BehaviorIsManaged
// 
// This program is free software; you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free Software Foundation;
// either version 2 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details. 
// You should have received a copy of the GNU General Public License along with this program; 
// if not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

#endregion

using ProtoBuf;
using Stump.Server.BaseServer.IPC.Objects;
using System;

namespace Stump.Server.BaseServer.IPC.Messages
{
    [ProtoContract]
    public class UpdateVipAccountMessage : IPCMessage
    {
        public UpdateVipAccountMessage()
        {

        }
        public UpdateVipAccountMessage(AccountData account, int usergroupid, DateTime subscriptionenddate, DateTime goldsubscriptionenddate, Boolean isSubscribe)
        {
            Account = account;
            UserGroupId = usergroupid;
            SubscriptionEndDate = subscriptionenddate;
            GoldSubscriptionEndDate = goldsubscriptionenddate;
            IsSubscribe = isSubscribe;
        }
        [ProtoMember(2)]
        public AccountData Account
        {
            get;
            set;
        }
        [ProtoMember(3)]
        public int UserGroupId
        {
            get;
            set;
        }
        [ProtoMember(4)]
        public DateTime SubscriptionEndDate
        {
            get;
            set;
        }
        [ProtoMember(5)]
        public DateTime GoldSubscriptionEndDate
        {
            get;
            set;
        }
        [ProtoMember(6)]
        public Boolean IsSubscribe
        {
            get;
            set;
        }
    }
}