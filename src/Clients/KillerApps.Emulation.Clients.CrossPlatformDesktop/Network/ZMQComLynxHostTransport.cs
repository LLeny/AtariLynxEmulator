using KillerApps.Emulation.AtariLynx;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Clients.CrossPlatformDesktop.Network
{
	public class ZMQComLynxHostTransport: ZMQComLynxClientTransport
	{
        private ZMQProxy _proxy;

        public ZMQComLynxHostTransport(string transport_pub, string transport_sub)
        :base(transport_pub, transport_sub)
        {
        }

        public override void Connect(Transmitter transmitter, Receiver receiver)
        {
            _proxy = new(_transport_pub, _transport_sub);
            _proxy.Initialize();
            _proxy.Start();
            base.Connect(transmitter, receiver);
        }
		
        public override void Initialize()
        {
            base.Initialize();
        }
	}
}
