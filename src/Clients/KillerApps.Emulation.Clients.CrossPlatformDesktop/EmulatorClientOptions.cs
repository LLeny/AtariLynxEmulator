using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KillerApps.Emulation.Clients.CrossPlatformDesktop
{
    public class EmulatorClientOptions
    {
        public int Magnification { get; set; }
        public FileInfo BootRom { get; set; }
        public FileInfo GameRom { get; set; }
        public bool FullScreen { get; set; }
        public ControllerType Controller { get; set; }
        public bool ComLynxHost { get; set; }
        public bool ComLynxClient { get; set; }
        public string ComLynxSubscriber { get; set; }
        public string ComLynxPublisher { get; set; }

        public static EmulatorClientOptions Default 
        { 
            get => new() {
                Magnification = EmulatorClient.DEFAULT_MAGNIFICATION,
                FullScreen = false,
                Controller = ControllerType.Keyboard,
                ComLynxHost = false,
                ComLynxClient = false,
                ComLynxPublisher = "tcp://127.0.0.1:1234",
                ComLynxSubscriber = "tcp://127.0.0.1:5678"
            };
        }
    }

    public enum ControllerType
    {
        Gamepad,
        Keyboard
    }
}
