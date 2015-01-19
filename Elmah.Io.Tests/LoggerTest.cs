﻿using System.Linq;
using Elmah.Io.Client;
using System;
using System.Collections;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Elmah.Io.Tests
{
    public class LoggerTest
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void CanLogMessage()
        {
            // Arrange
            var id = _fixture.Create<int>().ToString(CultureInfo.InvariantCulture);
            var logId = _fixture.Create<Guid>();
            Uri actualUri = null;
            string actualData = null;

            var requestHeaders = new WebHeaderCollection();
            var webClientMock = new Mock<IWebClient>();
            webClientMock
                .Setup(x => x.Post(It.IsAny<WebHeaderCollection>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Func<WebHeaderCollection, string, string>>()))
                .Callback<WebHeaderCollection, Uri, string, Func<WebHeaderCollection, string, string>>((headers, uri, data, resultor) => { requestHeaders = headers; actualUri = uri; actualData = data; })
                .Returns(Task.FromResult("https://elmah.io/api/v2/messages?id=" + id + "&logid=" + logId));

            var logger = new Logger(logId, null, webClientMock.Object);
            var message = _fixture.Create<Message>();

            // Act
            var result = logger.Log(message);

            // Assert
            Assert.That(result, Is.EqualTo(id));
            Assert.That(requestHeaders[HttpRequestHeader.ContentType], Is.EqualTo("application/json"));
            Assert.That(actualUri.AbsoluteUri, Is.Not.Null.And.StringEnding(string.Format("api/v2/messages?logId={0}", logId)));
            Assert.That(actualData, Is.Not.Null.And.StringContaining(message.Title));
        }

        [Test]
        public void CanGetMessage()
        {
            // Arrange
            var id = _fixture.Create<string>();
            var logId = _fixture.Create<Guid>();
            var message = _fixture.Create<Message>();
            Uri actualUri = null;

            var webClientMock = new Mock<IWebClient>();
            webClientMock
                .Setup(x => x.Get(It.IsAny<WebHeaderCollection>(), It.IsAny<Uri>(), It.IsAny<Func<WebHeaderCollection, string, string>>()))
                .Callback<WebHeaderCollection, Uri, Func<WebHeaderCollection, string, string>>((headers, uri, resultor) => { actualUri = uri; })
                .Returns(Task.FromResult(JsonConvert.SerializeObject(message)));

            var logger = new Logger(logId, null, webClientMock.Object);

            // Act
            var result = logger.GetMessage(id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(actualUri.AbsoluteUri, Is.Not.Null.And.StringEnding(string.Format("api/v2/messages?logId={1}&id={0}", id, logId)));
            Assert.That(result.Title, Is.EqualTo(message.Title));
        }

        [Test]
        public void CanGetErrors()
        {
            // Arrange
            var pageIndex = _fixture.Create<int>();
            var pageSize = _fixture.Create<int>();
            var logId = _fixture.Create<Guid>();
            var messages = _fixture.Create<MessagesResult>();
            Uri actualUri = null;

            var webClientMock = new Mock<IWebClient>();
            webClientMock
                .Setup(x => x.Get(It.IsAny<WebHeaderCollection>(), It.IsAny<Uri>(), It.IsAny<Func<WebHeaderCollection, string, string>>()))
                .Callback<WebHeaderCollection, Uri, Func<WebHeaderCollection, string, string>>((headers, uri, resultor) => { actualUri = uri; })
                .Returns(Task.FromResult(JsonConvert.SerializeObject(messages)));

            var logger = new Logger(logId, null, webClientMock.Object);
            var results = new ArrayList();

            // Act
            var result = logger.GetMessages(pageIndex, pageSize);

            // Assert
            Assert.That(actualUri.AbsoluteUri, Is.Not.Null.And.StringEnding(string.Format("api/v2/messages?logId={0}&pageindex={1}&pagesize={2}", logId, pageIndex, pageSize)));
            Assert.That(result.Total, Is.EqualTo(messages.Total));
            Assert.That(result.Messages, Is.Not.Null);
            Assert.That(result.Messages.Count, Is.EqualTo(messages.Messages.Count));
            messages.Messages.ForEach(message => Assert.That(result.Messages.Any(msg => msg.Title == message.Title)));
        } 
    }
}