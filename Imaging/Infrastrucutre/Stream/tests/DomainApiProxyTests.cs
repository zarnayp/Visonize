using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DupploPulse.UsImaging.Domain.Interfaces;
using DupploPulse.UsImaging.Infrastructure.Common;
using Moq;

namespace DupploPulse.UsImaging.Infrastructure.Stream.Tests
{
    public class DomainApiProxyTests
    {
        [Test]
        public void DomainApiProxy_MessageCreated()
        {
            var messages = new List<string>();
            Action<string> processMessage = (message) => { messages.Add(message); };
            var proxy = DomainApiProxyCreator.CreateDispatcherProxy<IImageViewArea>(processMessage);

            proxy.MouseDown(10, 25);

            Assert.AreEqual(1, messages.Count);

            // Deserialize
            ApiCall apiCall = new ApiCall(messages[0]);

            Assert.AreEqual("MouseDown", apiCall.Methods[0].MethodName);
        }

        [Test]
        public void DomainApiProxy_InnerObjectMessageCreated()
        {
            var messages = new List<string>();
            Action<string> processMessage = (message) => { messages.Add(message); };
            var proxy = DomainApiProxyCreator.CreateDispatcherProxy<IImageViewArea>(processMessage);

            proxy.TwoDAcousticViewports[1].PostProcessing.ChangeGrayMapIndex(1);

            Assert.AreEqual(1, messages.Count);

            ApiCall apiCall = new ApiCall(messages[0]);

            Assert.AreEqual("ChangeGrayMapIndex", apiCall.Methods[apiCall.Methods.Count-1].MethodName);
        }

        [Test]
        public void DomainApiProxy_CallFromString()
        {
            var imageViewAreMock = new Mock<IImageViewArea>();
            var logger = new Mock<IGeneralLogger>();
            var apiCall = "[{\"MethodName\":\"MouseDown\",\"Arguments\":[\"10\",\"25\"]}]";

            var deserializer = new DomainApiProxyDeserializer(imageViewAreMock.Object, logger.Object);

            deserializer.Deserialize(apiCall);

            imageViewAreMock.Verify(m => m.MouseDown(10, 25), Times.Once);

        }

        [Test]
        public void DomainApiProxy_InnerObjectSerializedDeserialized()
        {

            var messages = new List<string>();
            Action<string> processMessage = (message) => { messages.Add(message); };
            var proxy = DomainApiProxyCreator.CreateDispatcherProxy<IImageViewArea>(processMessage);

            var sw = Stopwatch.StartNew();

            proxy.TwoDAcousticViewports[2].PostProcessing.ChangeGrayMapIndex(3);

            sw.Stop();

            Assert.AreEqual(1, messages.Count);
            ApiCall apiCall = new ApiCall(messages[0]);

            // create mock
            var postProcessing = new Mock<ITwoDImagePostProcessing>();
            var viewportMock = new Mock<IAcousticTwoDViewport>();
            viewportMock.SetupGet(x => x.PostProcessing).Returns(postProcessing.Object);
            var list = new List<IAcousticTwoDViewport>
            {
                new Mock<IAcousticTwoDViewport>().Object,  // index 0
                new Mock<IAcousticTwoDViewport>().Object,  // index 1
                viewportMock.Object                        // index 2
            };
            var imageViewAreaMock = new Mock<IImageViewArea>();
            imageViewAreaMock.SetupGet(x => x.TwoDAcousticViewports).Returns(list);

            var logger = new Mock<IGeneralLogger>();

            var deserializer = new DomainApiProxyDeserializer(imageViewAreaMock.Object, logger.Object);

            var swDeserialization = Stopwatch.StartNew();

            deserializer.Deserialize(messages[0]);

            swDeserialization.Stop();

            proxy.TwoDAcousticViewports[2].PostProcessing.ChangeGrayMapIndex(2);
            Assert.AreEqual(2, messages.Count);
            ApiCall apiCall2 = new ApiCall(messages[1]);

            Console.WriteLine($"Serialization took {sw.ElapsedMilliseconds} ms.");
            Console.WriteLine($"Deserialization took {swDeserialization.ElapsedMilliseconds} ms.");
        }
    }
}
