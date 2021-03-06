﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Character
{
    public class CharacterTextures : ICharacterTextures
    {
        private readonly ICharacterSpriteCalculator _characterSpriteCalculator;
        public Texture2D Boots { get; private set; }
        public Texture2D Armor { get; private set; }
        public Texture2D Hat { get; private set; }
        public Texture2D Shield { get; private set; }
        public Texture2D Weapon { get; private set; }

        public Texture2D Hair { get; private set; }
        public ISpriteSheet Skin { get; private set; }

        public ISpriteSheet Emote { get; private set; }
        public ISpriteSheet Face { get; private set; }

        public CharacterTextures(ICharacterSpriteCalculator characterSpriteCalculator)
        {
            _characterSpriteCalculator = characterSpriteCalculator;
        }

        public void Refresh(ICharacterRenderProperties characterRenderProperties)
        {
            Boots = _characterSpriteCalculator.GetBootsTexture(characterRenderProperties).SheetTexture;
            Armor = _characterSpriteCalculator.GetArmorTexture(characterRenderProperties).SheetTexture;
            Hat = _characterSpriteCalculator.GetHatTexture(characterRenderProperties).SheetTexture;
            Shield = _characterSpriteCalculator.GetShieldTexture(characterRenderProperties).SheetTexture;
            Weapon = _characterSpriteCalculator.GetWeaponTexture(characterRenderProperties).SheetTexture;

            Hair = _characterSpriteCalculator.GetHairTexture(characterRenderProperties).SheetTexture;
            Skin = _characterSpriteCalculator.GetSkinTexture(characterRenderProperties);
            Emote = _characterSpriteCalculator.GetEmoteTexture(characterRenderProperties);
            Face = _characterSpriteCalculator.GetFaceTexture(characterRenderProperties);
        }
    }
}
