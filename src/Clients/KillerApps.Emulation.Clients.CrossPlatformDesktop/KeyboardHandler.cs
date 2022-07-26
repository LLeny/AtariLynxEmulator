using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KillerApps.Emulation.AtariLynx;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace KillerApps.Gaming.MonoGame
{
	public class Keyboard1Handler : InputHandler
	{
		public Keyboard1Handler(Game game): base(game) { }

		public override bool ExitGame
		{
			get
			{
				KeyboardState keyboard = Keyboard.GetState();
				return keyboard.IsKeyDown(Keys.F4) || keyboard.IsKeyDown(Keys.Escape);
			}
		}

		protected override JoystickStates BuildJoystickState()
		{
			KeyboardState keyboard = Keyboard.GetState();
			JoystickStates joystick = JoystickStates.None;

			if (keyboard.IsKeyDown(Keys.Down) == true) joystick |= JoystickStates.Down;
			if (keyboard.IsKeyDown(Keys.Up) == true) joystick |= JoystickStates.Up;
			if (keyboard.IsKeyDown(Keys.Left) == true) joystick |= JoystickStates.Left;
			if (keyboard.IsKeyDown(Keys.Right) == true) joystick |= JoystickStates.Right;
			if (keyboard.IsKeyDown(Keys.NumPad1) == true) joystick |= JoystickStates.Outside;
			if (keyboard.IsKeyDown(Keys.NumPad2) == true) joystick |= JoystickStates.Inside;
			if (keyboard.IsKeyDown(Keys.NumPad4) == true) joystick |= JoystickStates.Option1;
			if (keyboard.IsKeyDown(Keys.NumPad5) == true) joystick |= JoystickStates.Option2;

			return joystick;
		}
	}

public class Keyboard2Handler : InputHandler
	{
		public Keyboard2Handler(Game game): base(game) { }

		public override bool ExitGame
		{
			get
			{
				KeyboardState keyboard = Keyboard.GetState();
				return keyboard.IsKeyDown(Keys.F4) || keyboard.IsKeyDown(Keys.Escape);
			}
		}

		protected override JoystickStates BuildJoystickState()
		{
			KeyboardState keyboard = Keyboard.GetState();
			JoystickStates joystick = JoystickStates.None;

			if (keyboard.IsKeyDown(Keys.S) == true) joystick |= JoystickStates.Down;
			if (keyboard.IsKeyDown(Keys.W) == true) joystick |= JoystickStates.Up;
			if (keyboard.IsKeyDown(Keys.A) == true) joystick |= JoystickStates.Left;
			if (keyboard.IsKeyDown(Keys.D) == true) joystick |= JoystickStates.Right;
			if (keyboard.IsKeyDown(Keys.Q) == true) joystick |= JoystickStates.Outside;
			if (keyboard.IsKeyDown(Keys.E) == true) joystick |= JoystickStates.Inside;
			if (keyboard.IsKeyDown(Keys.D1) == true) joystick |= JoystickStates.Option1;
			if (keyboard.IsKeyDown(Keys.D2) == true) joystick |= JoystickStates.Option2;

			return joystick;
		}
	}
}
