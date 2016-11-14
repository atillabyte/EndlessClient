// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.NPC;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.NPC
{
    public interface INPCRenderer : IDrawable, IUpdateable, IGameComponent, IDisposable
    {
        int TopPixel { get; }

        Rectangle DrawArea { get; }

        Rectangle MapProjectedDrawArea { get; }

        INPC NPC { get; set; }

        void DrawToSpriteBatch(SpriteBatch spriteBatch);
    }
}