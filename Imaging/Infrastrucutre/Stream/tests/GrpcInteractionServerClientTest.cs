using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visonize.UsImaging.Domain.Service.Infrastructure;

namespace Visonize.UsImaging.Infrastructure.Stream.Tests
{
    public class GrpcInteractionServerClientTest
    {
        [Test]
        public void GrpcInteractionServerClient_TestMouseEvent()
        {
            var server = new GrpcInteractionServer();
            var client = new GrpcInteractionClient();

            MouseEventData? eventData = null;
            server.MouseMove += (o, e) => eventData = e;

            Thread.Sleep(1000);

            var mouseMove = new MouseEvent();
            mouseMove.EventType = 1;

            client.SendMouseEvents(mouseMove);

            SpinWait spinWait = new();
            var sw = Stopwatch.StartNew();

            while(eventData==null && sw.ElapsedMilliseconds < 1000)
            {
                spinWait.SpinOnce();
            }

            client.Dispose();
            server.Dispose();
        }

        [Test]
        public void GrpcInteractionServerClient_TestClientDisposeAndConnectNew()
        {
            var server = new GrpcInteractionServer();
            var client = new GrpcInteractionClient();

            var eventsArrived = new List<MouseEventData>();

            server.MouseMove += (o, e) => eventsArrived.Add(e);

            Thread.Sleep(1000);

            var mouseMove = new MouseEvent();
            mouseMove.EventType = 1;
            client.SendMouseEvents(mouseMove);
            client.Dispose();

            client = new GrpcInteractionClient();

            mouseMove.EventType = 1;

            client.SendMouseEvents(mouseMove);

            SpinWait spinWait = new();
            var sw = Stopwatch.StartNew();

            while (eventsArrived.Count == 2 && sw.ElapsedMilliseconds < 1000)
            {
                spinWait.SpinOnce();
            }

            Assert.AreEqual( 2, eventsArrived.Count);

            client.Dispose();
            server.Dispose();
        }
    }
}
