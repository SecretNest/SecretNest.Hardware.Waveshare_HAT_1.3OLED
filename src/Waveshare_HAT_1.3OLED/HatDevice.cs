using System;
using System.Device.Gpio;
using SecretNest.Hardware.SinoWealth;

namespace SecretNest.Hardware.Waveshare.HAT_1Point3OLED
{
    public class HatDevice : IDisposable
    {
        private SH1106 _oled;
        private bool _disposedValue;
        private const int Reset = 25;
        private const int Dc = 24;
        private const int Cs = 8;
        private const int Bl = 18;
        private const int Key1 = 21;
        private const int Key2 = 20;
        private const int Key3 = 16;
        private const int JoystickUp = 6;
        private const int JoystickDown = 19;
        private const int JoystickLeft = 5;
        private const int JoystickRight = 26;
        private const int JoystickCenter = 13;
        private const int SpiClockFrequency = 10000000;
        private const int Width = 128;
        //private const int Height = 64;
        private const int ImagePageCount = 8; //Height / 8
        private const int PageSize = 1024; //Width * Height /8

        private KeyPinHelper[] _keyPinHelpers;

        private GpioController _controller;

        public HatDevice(GpioController controller)
        {
            _controller = controller;
            controller.OpenPin(Bl, PinMode.Output);
            _controller.Write(Bl, PinValue.High);

            _oled = new SH1106(controller, Reset, Dc, Cs, SpiClockFrequency);

            _keyPinHelpers = new[]
            {
                new KeyPinHelper(controller, Key1, KeyName.Key1, e => GpioKey1Pressed?.Invoke(this, e),
                    e => GpioKey1Released?.Invoke(this, e)),
                new KeyPinHelper(controller, Key2, KeyName.Key2, e => GpioKey2Pressed?.Invoke(this, e),
                    e => GpioKey2Released?.Invoke(this, e)),
                new KeyPinHelper(controller, Key3, KeyName.Key3, e => GpioKey3Pressed?.Invoke(this, e),
                    e => GpioKey3Released?.Invoke(this, e)),
                new KeyPinHelper(controller, JoystickUp, KeyName.JoystickUp, e => GpioJoystickUpPressed?.Invoke(this, e),
                    e => GpioJoystickUpReleased?.Invoke(this, e)),
                new KeyPinHelper(controller, JoystickDown, KeyName.JoystickDown, e => GpioJoystickDownPressed?.Invoke(this, e),
                    e => GpioJoystickDownReleased?.Invoke(this, e)),
                new KeyPinHelper(controller, JoystickLeft, KeyName.JoystickLeft, e => GpioJoystickLeftPressed?.Invoke(this, e),
                    e => GpioJoystickLeftReleased?.Invoke(this, e)),
                new KeyPinHelper(controller, JoystickRight, KeyName.JoystickRight, e => GpioJoystickRightPressed?.Invoke(this, e),
                    e => GpioJoystickRightReleased?.Invoke(this, e)),
                new KeyPinHelper(controller, JoystickCenter, KeyName.JoystickCenter, e => GpioJoystickCenterPressed?.Invoke(this, e),
                    e => GpioJoystickCenterReleased?.Invoke(this, e))
            };

            _ones = new byte[PageSize];
            //Unsafe.InitBlock(ref ones[0], 255, PageSize);
        }

        public event EventHandler<KeyStateChangedEventArgs> GpioKey1Pressed;
        public event EventHandler<KeyStateChangedEventArgs> GpioKey1Released;
        public event EventHandler<KeyStateChangedEventArgs> GpioKey2Pressed;
        public event EventHandler<KeyStateChangedEventArgs> GpioKey2Released;
        public event EventHandler<KeyStateChangedEventArgs> GpioKey3Pressed;
        public event EventHandler<KeyStateChangedEventArgs> GpioKey3Released;
        public event EventHandler<KeyStateChangedEventArgs> GpioJoystickUpPressed;
        public event EventHandler<KeyStateChangedEventArgs> GpioJoystickUpReleased;
        public event EventHandler<KeyStateChangedEventArgs> GpioJoystickDownPressed;
        public event EventHandler<KeyStateChangedEventArgs> GpioJoystickDownReleased;
        public event EventHandler<KeyStateChangedEventArgs> GpioJoystickLeftPressed;
        public event EventHandler<KeyStateChangedEventArgs> GpioJoystickLeftReleased;
        public event EventHandler<KeyStateChangedEventArgs> GpioJoystickRightPressed;
        public event EventHandler<KeyStateChangedEventArgs> GpioJoystickRightReleased;
        public event EventHandler<KeyStateChangedEventArgs> GpioJoystickCenterPressed;
        public event EventHandler<KeyStateChangedEventArgs> GpioJoystickCenterReleased;

        public void ShowImage(byte[] buffer) => ShowImage(buffer.AsSpan());

        public void ShowImage(ReadOnlySpan<byte> buffer)
        {
            for (int page = 0; page < ImagePageCount; page++)
            {
                var slice = buffer.Slice(page * Width, Width);
                _oled.ShowPage(page, slice);
            }
        }

        public void ShowPage(int page, byte[] buffer) => ShowPage(page, buffer.AsSpan());

        public void ShowPage(int page, ReadOnlySpan<byte> buffer) => _oled.ShowPage(page, buffer);

        byte[] _ones;
        public void SetBlankScreen()
        {
            for (int page = 0; page < ImagePageCount; page++)
            {
                _oled.ShowPage(page, _ones);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _oled.Dispose();
                    foreach (var helper in _keyPinHelpers)
                        helper.Dispose();
                    _controller.ClosePin(Bl);
                }

                _oled = null;
                _keyPinHelpers = null;
                _controller = null;
                _ones = null;
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
