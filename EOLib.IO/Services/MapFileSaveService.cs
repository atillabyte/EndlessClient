﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using EOLib.IO.Map;

namespace EOLib.IO.Services
{
    public class MapFileSaveService : IMapFileSaveService
    {
        private readonly INumberEncoderService _numberEncoderService;
        private readonly IMapStringEncoderService _mapStringEncoderService;

        public MapFileSaveService(INumberEncoderService numberEncoderService,
            IMapStringEncoderService mapStringEncoderService)
        {
            _numberEncoderService = numberEncoderService;
            _mapStringEncoderService = mapStringEncoderService;
        }

        public void SaveFile(string path, IMapFile mapFile)
        {
            File.WriteAllBytes(string.Format(MapFile.MapFileFormatString, mapFile.Properties.MapID),
                mapFile.SerializeToByteArray(_numberEncoderService, _mapStringEncoderService));
        }
    }
}