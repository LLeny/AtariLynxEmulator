using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using KillerApps.Emulation.AtariLynx;
using KillerApps.Gaming.MonoGame;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using KillerApps.Emulation.Clients.CrossPlatformDesktop.Network;

namespace KillerApps.Emulation.Clients.CrossPlatformDesktop
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class EmulatorClient : Game
    {
        // Emulator 
        private List<LynxHandheld> emulators = new();
        private ContentManager romContent;

        // Video
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private List<Texture2D> lcdScreens = new();
        private int graphicsWidth;
        private int graphicsHeight;

        public const int DEFAULT_MAGNIFICATION = 8;
        private const int DEFAULT_GRAPHICS_WIDTH = Suzy.SCREEN_WIDTH * DEFAULT_MAGNIFICATION;
        private const int DEFAULT_GRAPHICS_HEIGHT = Suzy.SCREEN_HEIGHT * DEFAULT_MAGNIFICATION;
        private readonly EmulatorClientOptions clientOptions;

        // Input
        private List<InputHandler> inputHandlers = new();

        // Audio
        private byte[] soundBuffer;
        private DynamicSoundEffectInstance dynamicSound;

        public EmulatorClient(EmulatorClientOptions options = null) : base()
        {
            for(int i=0; i<options.EmulatorCount; ++i)
            {
                emulators.Add(new LynxHandheld());
            }
            graphics = new GraphicsDeviceManager(this);
            romContent = new ResourceContentManager(Services, Roms.ResourceManager);

            clientOptions = options ?? EmulatorClientOptions.Default;
            graphicsHeight = clientOptions.Magnification * Suzy.SCREEN_HEIGHT;
            graphicsWidth = clientOptions.Magnification * Suzy.SCREEN_WIDTH;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Content.RootDirectory = "Content";
            Window.Title = "Atari Lynx Emulator";
            Window.AllowUserResizing = false;
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(6); // 60Hz

            InitializeVideo(clientOptions.FullScreen);
            InitializeEmulator(clientOptions.BootRom, clientOptions.GameRom);
            InitializeAudio();

            for(int i=0; i<clientOptions.EmulatorCount; ++i)
            {
                InputHandler handler = clientOptions.Controller switch
                {
                    ControllerType.Gamepad => new GamePadHandler(this),
                    ControllerType.Keyboard => i % 2 == 0 ? new Keyboard1Handler(this) : new Keyboard2Handler(this),
                    _ => throw new NotImplementedException()
                };

                inputHandlers.Add(handler);                    
                Components.Add(handler);
            }

            InitializeSerial();

            base.Initialize();
        }

        private ICartridge LoadCartridge(FileInfo gameRomFileInfo)
        {
            ICartridge cartridge = null;
            LnxRomImageFileFormat gameRomImage = new LnxRomImageFileFormat();

            Stream gameRomStream = gameRomFileInfo?.OpenRead();
            if (gameRomStream is null) gameRomStream = new MemoryStream(Roms.junglejack);

            try
            {
                cartridge = gameRomImage.LoadCart(gameRomStream);
            }
            catch (Exception)
            {
                cartridge = new FaultyCart();
            }
            return cartridge;
        }

        private void InitializeEmulator(FileInfo bootRomFileInfo, FileInfo gameRomFileInfo)
        {
            // Lynx related
            Stream bootRomImage = bootRomFileInfo?.OpenRead();

            emulators.ForEach(emulator => 
            {
                emulator.BootRomImage = bootRomImage ?? (Stream)(new MemoryStream(Roms.LYNXBOOT));
                emulator.InsertCartridge(LoadCartridge(gameRomFileInfo));
                emulator.Initialize();                
                emulator.Reset();
            });
        }

        private void InitializeVideo(bool fullScreen)
        {
            // Set video options
            graphics.PreferredBackBufferWidth = graphicsWidth*clientOptions.EmulatorCount;
            graphics.PreferredBackBufferHeight = graphicsHeight;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            for(int i=0; i<clientOptions.EmulatorCount; ++i)
            {
                lcdScreens.Add(new Texture2D(graphics.GraphicsDevice, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT, false, SurfaceFormat.Color));
            }
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private void InitializeAudio()
        {
            dynamicSound = new DynamicSoundEffectInstance(22050, AudioChannels.Mono);
            soundBuffer = new byte[dynamicSound.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(250))];
            //dynamicSound.BufferNeeded += new EventHandler<EventArgs>(DynamicSoundBufferNeeded);
            emulators[0].Mikey.AudioFilter.BufferReady += new EventHandler<BufferEventArgs>(OnAudioFilterBufferReady);
            dynamicSound.Play();
        }

        private void InitializeSerial()
        {
            if(emulators.Count > 1)
            {
                emulators[0].InsertComLynxCable(new ZMQComLynxHostTransport("inproc://comlynx.publisher", "inproc://comlynx.subscriber"));

                foreach(var emulator in emulators.Skip(1))
                {
                    emulator.InsertComLynxCable(new ZMQComLynxClientTransport("inproc://comlynx.publisher", "inproc://comlynx.subscriber"));
                } 
            }          
        }

        void OnAudioFilterBufferReady(object sender, BufferEventArgs e)
        {
            byte[] buffer = e.Buffer;
            dynamicSound.SubmitBuffer(buffer, 0, buffer.Length / 2);
            dynamicSound.SubmitBuffer(buffer, buffer.Length / 2, buffer.Length / 2);
        }

        private void DynamicSoundBufferNeeded(object sender, EventArgs e)
        {
            byte[] buffer = emulators[0].Mikey.AudioFilter.Buffer;
            dynamicSound.SubmitBuffer(buffer, 0, buffer.Length / 2);
            dynamicSound.SubmitBuffer(buffer, buffer.Length / 2, buffer.Length / 2);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (inputHandlers.Any(ih => ih.ExitGame))
                this.Exit();

            for(int i=0; i< clientOptions.EmulatorCount; ++i)
            {
                inputHandlers[i].Update(gameTime);
                JoystickStates joystick = inputHandlers[i].Joystick;
                emulators[i].UpdateJoystickState(joystick);
            }

            int cycle_divisor = 100;

            for(int i=0; i<86667/cycle_divisor; ++i)// 4 MHz worth of cycles divided by 60 seconds
            {
                emulators.ForEach(e => e.Update((ulong)cycle_divisor));
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);

            for(int i=0; i<clientOptions.EmulatorCount; ++i)
            {
                lcdScreens[i].SetData(emulators[i].LcdScreenDma, 0, 0x3FC0);

                spriteBatch.Draw(lcdScreens[i],
                    new Rectangle(graphicsWidth*i, 0, graphicsWidth, graphicsHeight),
                    new Rectangle(0, 0, Suzy.SCREEN_WIDTH, Suzy.SCREEN_HEIGHT), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            // Stop sound before exiting
            //if (dynamicSound.State != SoundState.Stopped) dynamicSound.Stop(true);
            base.OnExiting(sender, args);
        }
    }
}
