﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EOLib.Net.Communication;
using EOLib.Net.Connection;

namespace EOLib.Net
{
	/// <summary>
	/// Wraps an in-band network operation (send/receive), handling send and receive exceptions and executing code when a failure occurs.
	/// <para>By default, disconnects from server and stops receiving on error</para>
	/// </summary>
	/// <typeparam name="T">The expected output data of the network operation</typeparam>
	public class SafeInBandNetworkOperation<T>
	{
		private readonly IBackgroundReceiveActions _backgroundReceiveActions;
		private readonly INetworkConnectionActions _networkConnectionActions;
		private readonly Func<Task<T>> _operation;
		private readonly Action _errorAction;

		public T Result { get; private set; }

		public SafeInBandNetworkOperation(IBackgroundReceiveActions backgroundReceiveActions,
										  INetworkConnectionActions networkConnectionActions,
										  Func<Task<T>> operation,
										  Action errorAction = null)
		{
			_backgroundReceiveActions = backgroundReceiveActions;
			_networkConnectionActions = networkConnectionActions;
			_operation = operation;
			_errorAction = errorAction ?? (() => { });
		}

		public async Task<bool> Invoke()
		{
			try
			{
				Result = await _operation();
				return true;
			}
			catch (NoDataSentException) { }
			catch (EmptyPacketReceivedException) { }

			_errorAction();
			DisconnectAndStopReceiving();
			return false;
		}

		private void DisconnectAndStopReceiving()
		{
			_backgroundReceiveActions.CancelBackgroundReceiveLoop();
			_networkConnectionActions.DisconnectFromServer();
		}
	}
}
