﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;

namespace EOLib.Net
{
	public interface ISafeInBandNetworkOperationFactory
	{
		SafeInBandNetworkOperation<T> CreateSafeOperation<T>(Func<Task<T>> networkOperation,
			Action errorAction);
	}
}