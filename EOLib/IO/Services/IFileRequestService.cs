﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Protocol;
using EOLib.IO.Map;
using EOLib.IO.Old;

namespace EOLib.IO.Services
{
    public interface IFileRequestService
    {
        Task<IMapFile> RequestMapFile(short mapID);

        Task<IModifiableDataFile<T>> RequestFile<T>(InitFileType fileType) where T : IDataRecord;
    }
}
