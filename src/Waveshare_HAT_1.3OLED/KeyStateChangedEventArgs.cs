using System;

namespace SecretNest.Hardware.Waveshare.HAT_1Point3OLED
{
    public class KeyStateChangedEventArgs : EventArgs
    {
        public KeyStateChangedEventArgs(KeyName keyName, KeyState keyState)
        {
            KeyName = keyName;
            KeyState = keyState;
        }

        public KeyName KeyName { get; }
        public KeyState KeyState { get; }
    }

    public enum KeyName
    {
        Key1,
        Key2,
        Key3,
        JoystickUp,
        JoystickDown,
        JoystickLeft,
        JoystickRight,
        JoystickCenter
    }

    public enum KeyState
    {
        Pressed,
        Released
    }
}
