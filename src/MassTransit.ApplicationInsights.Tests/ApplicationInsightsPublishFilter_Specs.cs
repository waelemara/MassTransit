﻿// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.ApplicationInsights.Tests
{
    using System;
    using GreenPipes;
    using Microsoft.ApplicationInsights;
    using Moq;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using Context;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Pipeline;
    using TestFramework.Messages;
    using Util;


    [TestFixture]
    public class ApplicationInsightsPublishFilter_Specs
    {
        Mock<SendHeaders> _mockHeaders;

        [SetUp]
        public void SetUp()
        {
            _mockHeaders = new Mock<SendHeaders>();
        }

        [Test]
        public async Task Should_send_context_to_next_pipe()
        {
            // Arrange.
            var mockPublishContext = new Mock<PublishContext<PingMessage>>();
            mockPublishContext.Setup(c => c.Headers).Returns(_mockHeaders.Object);
            var filter = new ApplicationInsightsPublishFilter<PingMessage>(new TelemetryClient(), "", "", null);

            var mockPipe = new Mock<IPipe<PublishContext<PingMessage>>>();

            // Act.
            await filter.Send(mockPublishContext.Object, mockPipe.Object);

            // Assert.
            mockPipe.Verify(action => action.Send(mockPublishContext.Object), Times.Once);
        }

        [Test]
        public async Task Should_invoke_the_configure_operation()
        {
            // Arrange.
            var mockPublishContext = new Mock<PublishContext<PingMessage>>();
            mockPublishContext.Setup(c => c.Headers).Returns(_mockHeaders.Object);
            bool configureOperationHasBeenCalled = false;

            var filter = new ApplicationInsightsPublishFilter<PingMessage>(new TelemetryClient(), "", "",
                (holder, context) => configureOperationHasBeenCalled = true);

            // Act.
            await filter.Send(mockPublishContext.Object, new Mock<IPipe<PublishContext<PingMessage>>>().Object);

            // Assert.
            Assert.IsTrue(configureOperationHasBeenCalled);
        }

        [Test]
        public async Task Should_use_updated_telemetryId_and_parent_id_on_context_headers_using_configure_op()
        {
            // Arrange.
            var operationId = Guid.NewGuid().ToString();
            var parentId = Guid.NewGuid().ToString();
            var rootKeyHeaderName = "RootKey";
            var parentKeyHeaderName = "ParentKey";

            var mockPublishContext = new Mock<PublishContext<PingMessage>>();
            mockPublishContext.Setup(c => c.Headers).Returns(_mockHeaders.Object);

            _mockHeaders.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<string>()));

            var filter = new ApplicationInsightsPublishFilter<PingMessage>(new TelemetryClient(),
                rootKeyHeaderName, parentKeyHeaderName, (holder, context) =>
                {
                    holder.Telemetry.Context.Operation.Id = operationId;
                    holder.Telemetry.Id = parentId;
                });

            // Act.
            await filter.Send(mockPublishContext.Object, new Mock<IPipe<PublishContext<PingMessage>>>().Object);

            // Assert.
            _mockHeaders.Verify(x => x.Set(rootKeyHeaderName, operationId), Times.Once);
            _mockHeaders.Verify(x => x.Set(parentKeyHeaderName, parentId), Times.Once);
        }

        [Test]
        public async Task Should_add_the_expected_properties_to_the_dependency_telemetry()
        {
            // Arrange.
            var messageId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var destinationAddress = new Uri("sb://my-organization.servicebus.windows.net/MyNamespace/MyMessage");

            var mockSendContext = new Mock<SendContext>();
            mockSendContext.Setup(c => c.Headers).Returns(_mockHeaders.Object);
            mockSendContext.Setup(c => c.MessageId).Returns(messageId);
            mockSendContext.Setup(c => c.ConversationId).Returns(conversationId);
            mockSendContext.Setup(c => c.CorrelationId).Returns(correlationId);
            mockSendContext.Setup(c => c.RequestId).Returns(requestId);
            mockSendContext.Setup(c => c.DestinationAddress).Returns(destinationAddress);

            var publishContextProxy = new PublishContextProxy<PingMessage>(mockSendContext.Object, new PingMessage());

            var capturedTelemetry = default(DependencyTelemetry);

            var filter = new ApplicationInsightsPublishFilter<PingMessage>(new TelemetryClient(), "", "",
                (holder, context) => capturedTelemetry = holder.Telemetry);

            // Act.
            await filter.Send(publishContextProxy, new Mock<IPipe<PublishContext<PingMessage>>>().Object);

            // Assert.
            Assert.IsNotNull(capturedTelemetry);
            Assert.AreEqual(capturedTelemetry.Properties["MessageId"], messageId.ToString());
            Assert.AreEqual(capturedTelemetry.Properties["MessageType"], TypeMetadataCache<PingMessage>.ShortName);
            Assert.AreEqual(capturedTelemetry.Properties["ConversationId"], conversationId.ToString());
            Assert.AreEqual(capturedTelemetry.Properties["CorrelationId"], correlationId.ToString());
            Assert.AreEqual(capturedTelemetry.Properties["DestinationAddress"], destinationAddress.ToString());
            Assert.AreEqual(capturedTelemetry.Properties["RequestId"], requestId.ToString());
        }

        [Test]
        public async Task Should_not_fail_if_properties_are_already_defined()
        {
            var telemetryClient = new TelemetryClient();
            // Arrange.
            var messageId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var inputAddress = new Uri("http://masstransit-project.com/");
            var destinationAddress = new Uri("sb://my-organization.servicebus.windows.net/MyNamespace/MyMessage");

            var mockReceiveContext = new Mock<ReceiveContext>();
            mockReceiveContext.Setup(c => c.InputAddress).Returns(inputAddress);

            var mockSendContext = new Mock<SendContext<PingMessage>>();
            mockSendContext.Setup(c => c.Headers).Returns(_mockHeaders.Object);
            mockSendContext.Setup(c => c.MessageId).Returns(messageId);
            mockSendContext.Setup(c => c.ConversationId).Returns(conversationId);
            mockSendContext.Setup(c => c.CorrelationId).Returns(correlationId);
            mockSendContext.Setup(c => c.RequestId).Returns(requestId);
            mockSendContext.Setup(c => c.DestinationAddress).Returns(destinationAddress);

            var publishContextProxy = new PublishContextProxy<PingMessage>(mockSendContext.Object, new PingMessage());

            var capturedTelemetry = default(DependencyTelemetry);

            var filter = new ApplicationInsightsPublishFilter<PingMessage>(new TelemetryClient(), "", "", (holder, context) => capturedTelemetry = holder.Telemetry);

            await filter.Send(publishContextProxy, new Mock<IPipe<PublishContext<PingMessage>>>().Object);

            var configuration = TelemetryConfiguration.CreateDefault();
            configuration.TelemetryInitializers.Add(new CopyPropertiesTelemetryInitializer(capturedTelemetry));

            telemetryClient = new TelemetryClient(configuration);
            filter = new ApplicationInsightsPublishFilter<PingMessage>(telemetryClient, "", "", (h, c) => { });

            // Act, Assert
            Assert.That(async () => await filter.Send(publishContextProxy, new Mock<IPipe<PublishContext<PingMessage>>>().Object), Throws.Nothing);
        }
    }
}
