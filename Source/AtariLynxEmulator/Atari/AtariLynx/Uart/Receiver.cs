﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class Receiver
	{
		private SerialControlRegister2 controlRegister;
		private byte? acceptRegister;
		private int receivePulseCountdown;
		private const int RECEIVE_PERIODS = 11;

		public event EventHandler<UartDataEventArgs> DataReceived;

		public byte SerialData { get; private set; }
		
		public Receiver(SerialControlRegister2 register)
		{
			this.controlRegister = register;
		}

		public void AcceptData(byte data)
		{
			if (!acceptRegister.HasValue)
			{
				acceptRegister = data;
				receivePulseCountdown = RECEIVE_PERIODS;
			}
			else
			{
				controlRegister.FrameError = true;
			}
		}

		public bool IsReceiving { get { return acceptRegister.HasValue; } }

		public void HandleBaudPulse(object sender, EventArgs e)
		{
			receivePulseCountdown--;

			if (receivePulseCountdown == 0)
			{
				byte data = acceptRegister.Value;
				ReceiveData(data);
			}
		}

		private void ReceiveData(byte data)
		{
			// Data is ready to receive
			SerialData = data;

			// Remove data currently received from holding register
			acceptRegister = null;

			// Check for overrun
			if (controlRegister.ReceiveReady)
			{
				// Previous data has not been read yet, so overrun 
				controlRegister.OverrunError = true;
			}

			// Mark receive ready
			controlRegister.ReceiveReady = true;

			// Complete sending current shift register contents
			UartDataEventArgs args = new UartDataEventArgs() 
				{ 
					Break = false, Data = data, StopBitPresent = false
				};
			OnReceived(args);
		}

		protected virtual void OnReceived(UartDataEventArgs args)
		{
			args.ParityBit = Uart4.ComputeParityBit(args.Data, controlRegister);
			if (DataReceived != null) DataReceived(this, args);
		}
	}
}
