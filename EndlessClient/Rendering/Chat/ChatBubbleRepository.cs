﻿// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib;

namespace EndlessClient.Rendering.Chat
{
    public interface IChatBubbleRepository : IDisposable
    {
        Optional<IChatBubble> MainCharacterChatBubble { get; set; }

        Dictionary<int, IChatBubble> OtherCharacterChatBubbles { get; set; }

        Dictionary<int, IChatBubble> NPCChatBubbles { get; set; }
    }

    public interface IChatBubbleProvider
    {
        Optional<IChatBubble> MainCharacterChatBubble { get; }

        IReadOnlyDictionary<int, IChatBubble> OtherCharacterChatBubbles { get; }

        IReadOnlyDictionary<int, IChatBubble> NPCChatBubbles { get; }
    }

    public class ChatBubbleRepository : IChatBubbleRepository, IChatBubbleProvider
    {
        public Optional<IChatBubble> MainCharacterChatBubble { get; set; }

        public Dictionary<int, IChatBubble> OtherCharacterChatBubbles { get; set; }
        public Dictionary<int, IChatBubble> NPCChatBubbles { get; set; }

        IReadOnlyDictionary<int, IChatBubble> IChatBubbleProvider.OtherCharacterChatBubbles => OtherCharacterChatBubbles;
        IReadOnlyDictionary<int, IChatBubble> IChatBubbleProvider.NPCChatBubbles => NPCChatBubbles;

        public ChatBubbleRepository()
        {
            MainCharacterChatBubble = Optional<IChatBubble>.Empty;
            OtherCharacterChatBubbles = new Dictionary<int, IChatBubble>();
            NPCChatBubbles = new Dictionary<int, IChatBubble>();
        }

        public void Dispose()
        {
            if (MainCharacterChatBubble.HasValue)
                MainCharacterChatBubble.Value.Dispose();

            foreach (var bubble in OtherCharacterChatBubbles.Values)
                bubble.Dispose();
            OtherCharacterChatBubbles.Clear();

            foreach (var bubble in NPCChatBubbles.Values)
                bubble.Dispose();
            NPCChatBubbles.Clear();
        }
    }
}
