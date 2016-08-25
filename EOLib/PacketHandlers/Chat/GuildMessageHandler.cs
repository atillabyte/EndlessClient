﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Net;

namespace EOLib.PacketHandlers.Chat
{
    public class GuildMessageHandler : PlayerChatByNameBase
    {
        private readonly IChatRepository _chatRepository;

        public override PacketAction Action { get { return PacketAction.Request; } }

        public GuildMessageHandler(IPlayerInfoProvider playerInfoProvider,
                                   IChatRepository chatRepository)
            : base(playerInfoProvider)
        {
            _chatRepository = chatRepository;
        }

        protected override void PostChat(string name, string message)
        {
            var data = new ChatData(name, message);
            _chatRepository.AllChat[ChatTab.Group].Add(data);
        }
    }
}