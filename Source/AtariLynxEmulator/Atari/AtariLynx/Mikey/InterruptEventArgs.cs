﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KillerApps.Emulation.Atari.Lynx
{
	public class InterruptEventArgs: EventArgs
	{
		public byte Mask { get; set; }
	}
}
