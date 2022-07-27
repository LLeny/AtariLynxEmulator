using KillerApps.Emulation.AtariLynx;
using System;
using NetMQ.Sockets;
using NetMQ;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KillerApps.Emulation.Clients.CrossPlatformDesktop.Network
{
	public class ZMQComLynxClientTransport: IComLynxTransport
	{
        private readonly string TOPIC = Guid.NewGuid().ToString().Substring(0, 4);
         internal string _transport_pub;
        internal string _transport_sub;

        private PublisherSocket _publisher;
        private SubscriberSocket _subscriber;
        private SocketOptions _publisher_options;
        private SocketOptions _subscriber_options;

        private NetMQPoller _poller;
        private NetMQQueue<NetMQMessage> _queue;


		private Receiver _receiver;	
		private Transmitter _transmitter;


        public ZMQComLynxClientTransport(string transport_pub, string transport_sub)
        {
            _transport_pub = transport_pub;
            _transport_sub = transport_sub;
        }

        public virtual void Connect(Transmitter transmitter, Receiver receiver)
        {
			_transmitter = transmitter;
			_receiver = receiver;

            _publisher = new($">{_transport_sub}");
            _subscriber = new($">{_transport_pub}");


            _publisher_options = new (_publisher) 
            {
                Linger = new TimeSpan(0),
                ReceiveHighWatermark = 10000,
            };
            
            _subscriber_options = new (_subscriber) 
            {
                Linger = new TimeSpan(0),
                ReceiveHighWatermark = 10000,
            };

            _queue = new ();
            _poller = new (){ _publisher, _subscriber, _queue };
        }
		
        public virtual void Initialize()
        {
            _subscriber.SubscribeToAnyTopic();
            
            _subscriber.ReceiveReady += (s, e) => 
            {
                var msg = e.Socket.ReceiveMultipartMessage();
                if(msg[0].ConvertToString() == TOPIC)
                {
                    return;
                }

                foreach(var data in msg[1].Buffer)
                {
                    Receive(data);
                }
            };

            _queue.ReceiveReady += (s, m) => 
            {
                while(!_queue.IsEmpty)
                {
                    _publisher.TrySendMultipartMessage(_queue.Dequeue());
                }
            };
            
            _poller.RunAsync();
        }

		public void ChangeSettings(SerialControlRegister register, int baudrate)
        {

        }
		
        public void Send(byte data)
        {
            NetMQMessage msg = new ();
            msg.Append(TOPIC);
            msg.Append(new []{ data });

            _queue.Enqueue(msg);
        }
		
        public void Receive(byte data)
        {
            _receiver.ReceiveData(data);
        }

        public virtual void Dispose()
        {
            _poller?.RemoveAndDispose(_publisher);
            _poller?.RemoveAndDispose(_subscriber);
            _poller?.Dispose();
        }
	}
}
