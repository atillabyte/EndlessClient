﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EOLib.Net
{
	public class PacketBuilder : IPacketBuilder
	{
		private const byte BREAK_STR_MAXVAL = 121;

		private readonly IReadOnlyList<byte> _data;

		public int Length { get { return _data.Count; } }

		public PacketFamily Family { get { return (PacketFamily)_data[1]; } }

		public PacketAction Action { get { return (PacketAction) _data[0]; } }

		public PacketBuilder(PacketFamily family, PacketAction action)
		{
			_data = new List<byte> { (byte) action, (byte) family };
		}

		private PacketBuilder(IReadOnlyList<byte> data)
		{
			_data = data;
		}

		public IPacketBuilder WithFamily(PacketFamily family)
		{
			var newData = new List<byte>(_data);
			newData[1] = (byte)family;
			return new PacketBuilder(newData);
		}

		public IPacketBuilder WithAction(PacketAction action)
		{
			var newData = new List<byte>(_data);
			newData[0] = (byte)action;
			return new PacketBuilder(newData);
		}

		public IPacketBuilder AddBreak()
		{
			return AddByte(byte.MaxValue);
		}

		public IPacketBuilder AddByte(byte b)
		{
			return AddBytes(new[] { b });
		}

		public IPacketBuilder AddChar(byte b)
		{
			return AddBytes(OldPacket.EncodeNumber(b, 1));
		}

		public IPacketBuilder AddShort(short s)
		{
			return AddBytes(OldPacket.EncodeNumber(s, 2));
		}

		public IPacketBuilder AddThree(int t)
		{
			return AddBytes(OldPacket.EncodeNumber(t, 3));
		}

		public IPacketBuilder AddInt(int i)
		{
			return AddBytes(OldPacket.EncodeNumber(i, 4));
		}

		public IPacketBuilder AddString(string s)
		{
			return AddBytes(Encoding.ASCII.GetBytes(s));
		}

		public IPacketBuilder AddBreakString(string s)
		{
			var sBytes = Encoding.ASCII.GetBytes(s);
			var sList = sBytes.Select(b => b == byte.MaxValue ? BREAK_STR_MAXVAL : b).ToList();
			sList.Add(byte.MaxValue);
			return AddBytes(sList);
		}

		public IPacketBuilder AddBytes(IEnumerable<byte> bytes)
		{
			var list = new List<byte>(_data);
			list.AddRange(bytes);
			return new PacketBuilder(list);
		}

		public IPacket Build()
		{
			return new Packet(_data.ToList());
		}
	}
}