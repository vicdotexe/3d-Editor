using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Eli
{

    public class Ps4Input
    {
        public VirtualIntegerAxis HPad;
        public VirtualIntegerAxis VPad;

        public VirtualButton X;
        public VirtualButton Square;
        public VirtualButton Circle;
        public VirtualButton Triangle;

        public VirtualButton L1;
        public VirtualButton L2;
        public VirtualButton R1;
        public VirtualButton R2;

        public VirtualJoystick LeftStick;
        public VirtualJoystick RightStick;

        private int _slot = 0;

        public static Ps4Input Main { get; }

        static Ps4Input()
        {
            Main = new Ps4Input();
        }

        public Ps4Input(int slot = 0)
        {
            _slot = 0;

            HPad = new VirtualIntegerAxis().AddGamePadDPadLeftRight();//.AddGamePadLeftStickX();
            VPad = new VirtualIntegerAxis().AddGamePadDPadUpDown();

            X = new VirtualButton().AddGamePadButton(_slot, Buttons.A);
            Square = new VirtualButton().AddGamePadButton(_slot, Buttons.X);
            Circle = new VirtualButton().AddGamePadButton(_slot, Buttons.B);
            Triangle = new VirtualButton().AddGamePadButton(_slot, Buttons.Y);

            L1 = new VirtualButton().AddGamePadButton(_slot, Buttons.LeftShoulder);
            L2 = new VirtualButton().AddGamePadButton(_slot, Buttons.LeftTrigger);
            R1 = new VirtualButton().AddGamePadButton(_slot, Buttons.RightShoulder);
            R2 = new VirtualButton().AddGamePadButton(_slot, Buttons.RightTrigger);

            LeftStick = new VirtualJoystick(true).AddGamePadLeftStick(_slot);
            LeftStick.Normalized = false;
            RightStick = new VirtualJoystick(true).AddGamePadRightStick(_slot);
            RightStick.Normalized = false;
        }
    }
}
