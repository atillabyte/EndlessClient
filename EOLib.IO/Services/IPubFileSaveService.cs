﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Pub;

namespace EOLib.IO.Services
{
    public interface IPubFileSaveService
    {
        void SaveFile(string path, IPubFile pubFile);
    }
}
