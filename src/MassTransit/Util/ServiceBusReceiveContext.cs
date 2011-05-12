﻿// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Util
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading;
	using Context;
	using Events;
	using Exceptions;
	using log4net;
	using Magnum.Extensions;
	using Magnum.Pipeline;
	using Magnum.Reflection;

	public class ServiceBusReceiveContext
	{
		static readonly ILog _log = LogManager.GetLogger(typeof (ServiceBusReceiveContext));

		readonly IServiceBus _bus;
		readonly Stopwatch _consumeTime;
		readonly Pipe _eventAggregator;
		readonly Stopwatch _receiveTime;
		readonly TimeSpan _receiveTimeout;
		int _consumeCount;
		bool _consumeNotified;
		IEnumerator<Action<IConsumeContext>> _consumers;
		bool _receiveNotified;

		public ServiceBusReceiveContext(IServiceBus bus, Pipe eventAggregator, TimeSpan receiveTimeout)
		{
			_bus = bus;
			_receiveTimeout = receiveTimeout;
			_eventAggregator = eventAggregator;
			_receiveTime = new Stopwatch();
			_consumeTime = new Stopwatch();
			_consumeCount = 0;
		}

		public void ReceiveFromEndpoint()
		{
			try
			{
				if (_log.IsDebugEnabled)
					_log.DebugFormat("Calling Receive on {0} from thread {1} ({2})", _bus.Endpoint.Uri,
						Thread.CurrentThread.ManagedThreadId, _receiveTimeout);

				_receiveTime.Start();

				_bus.Endpoint.Receive(context =>
					{
						if (_log.IsDebugEnabled)
							_log.DebugFormat("Enumerating pipeline on {0} from thread {1}", _bus.Endpoint.Uri,
								Thread.CurrentThread.ManagedThreadId);

						context.SetBus(_bus);

						IEnumerable<Action<IConsumeContext>> enumerable = _bus.InboundPipeline.Enumerate(context);

						_consumers = enumerable.GetEnumerator();
						if (!_consumers.MoveNext())
						{
							_consumers.Dispose();
							return null;
						}

						return DeliverMessageToConsumers;
					}, _receiveTimeout);
			}
			catch (ObjectDisposedException)
			{
				Thread.Sleep(1000);
			}
			catch (Exception ex)
			{
				_log.Error("Consumer Exception Exposed", ex);
			}
			finally
			{
				NotifyReceiveCompleted();
				NotifyConsumeCompleted();
			}
		}

		void DeliverMessageToConsumers(IConsumeContext context)
		{
			try
			{
				NotifyReceiveCompleted();

				_receiveTime.Stop();
				_consumeTime.Start();

				if (_log.IsDebugEnabled)
					_log.DebugFormat("Dispatching message on {0} from thread {1}", _bus.Endpoint.Uri,
						Thread.CurrentThread.ManagedThreadId);

				bool atLeastOneConsumerFailed = false;

				Exception lastException = null;

				do
				{
					try
					{
						_consumers.Current(context);
						_consumeCount++;
					}
					catch (Exception ex)
					{
						_log.Error(string.Format("'{0}' threw an exception consuming message '{1}'",
							_consumers.Current.GetType().FullName,
							context.GetType().FullName), ex);

						atLeastOneConsumerFailed = true;
						lastException = ex;

						CreateAndPublishFault(context, ex);
					}
				} while (_consumers.MoveNext());

				if (atLeastOneConsumerFailed)
				{
					throw new MessageException(context.GetType(),
						"At least one consumer threw an exception",
						lastException);
				}
			}
			finally
			{
				_consumeTime.Stop();

				_consumers.Dispose();
				_consumers = null;

				ReportConsumerTime(context.MessageType, _receiveTime.Elapsed, _consumeTime.Elapsed);
				ReportConsumerCount(context.MessageType, _consumeCount);
			}
		}

		void CreateAndPublishFault(object message, Exception ex)
		{
			// todo rip this stuff and use the context to publish
			if (message.Implements(typeof (CorrelatedBy<>)))
				this.FastInvoke("PublishFault", FastActivator.Create(typeof (Fault<,>), message, ex));
			else
				this.FastInvoke("PublishFault", FastActivator.Create(typeof (Fault<>), message, ex));
		}

		[UsedImplicitly]
		void PublishFault<T>(T message) where T : class
		{
			CurrentMessage.GenerateFault(message);
		}

		void ReportConsumerTime(string messageType, TimeSpan receiveTime, TimeSpan consumeTime)
		{
			var message = new MessageReceived
				{
					MessageType = messageType,
					ReceiveDuration = receiveTime,
					ConsumeDuration = consumeTime,
				};

			_eventAggregator.Send(message);
		}

		void ReportConsumerCount(string messageType, int count)
		{
			var message = new MessageConsumed
				{
					MessageType = messageType,
					ConsumeCount = count,
				};

			_eventAggregator.Send(message);
		}

		void NotifyReceiveCompleted()
		{
			if (_receiveNotified)
				return;

			_eventAggregator.Send(new ReceiveCompleted());
			_receiveNotified = true;
		}

		void NotifyConsumeCompleted()
		{
			if (_consumeNotified)
				return;

			_eventAggregator.Send(new ConsumeCompleted());
			_consumeNotified = true;
		}
	}
}