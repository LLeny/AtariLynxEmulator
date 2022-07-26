using System;
using NetMQ.Sockets;
using NetMQ;

namespace KillerApps.Emulation.Clients.CrossPlatformDesktop.Network
{
	public class ZMQProxy : IDisposable
	{
        private string _transport_pub;
        private string _transport_sub;
        private XPublisherSocket _publisher;
        private XSubscriberSocket _subscriber;
        private SocketOptions _publisher_options;
        private SocketOptions _subscriber_options;
        private Proxy _proxy;
        private NetMQPoller _poller;

        public ZMQProxy(string transport_pub, string transport_sub)
        {
            _transport_pub = transport_pub;
            _transport_sub = transport_sub;
        }

        public void Initialize()
        {
            _publisher = new($"@{_transport_pub}");
            _subscriber = new($"@{_transport_sub}");

            _publisher_options = new (_publisher) 
            {
                Linger = new TimeSpan(0),
                // SendLowWatermark = 1,
                // SendHighWatermark = 1,
                // ReceiveLowWatermark = 1,
                ReceiveHighWatermark = 10000,
            };
            
            _subscriber_options = new (_subscriber) 
            {
                Linger = new TimeSpan(0),
                // SendLowWatermark = 1,
                // SendHighWatermark = 1,
                // ReceiveLowWatermark = 1,
                ReceiveHighWatermark = 10000,
            };

            _poller = new () { _publisher, _subscriber };

            _proxy = new(_subscriber, _publisher, poller: _poller);
        }

        public void Start()
        {
            _proxy.Start();
            _poller.RunAsync();
        }

        public void Dispose()
        {
            _proxy?.Stop();
            _poller?.RemoveAndDispose(_publisher);
            _poller?.RemoveAndDispose(_subscriber);
            _poller?.Dispose();
        }
    }
}
