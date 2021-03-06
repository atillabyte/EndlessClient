// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.CharacterProperties
{
    public interface ICharacterPropertyRendererBuilder
    {
        IEnumerable<ICharacterPropertyRenderer> BuildList(ICharacterTextures characterTextures,
                                                          ICharacterRenderProperties renderProperties);
    }
}