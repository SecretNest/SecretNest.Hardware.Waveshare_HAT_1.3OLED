using System;
using System.Collections.Generic;
using System.Device.Gpio;
//using System.Runtime.CompilerServices;
using System.Text;
using SecretNest.Hardware.IO;
using SecretNest.Hardware.SinoWealth;

namespace SecretNest.Hardware.Waveshare.HAT_1Point3OLED
{
    public class HatDevice : IDisposable
    {
        private SH1106 _oled;
        private bool _disposedValue;
        private const int GpioReset = 25;
        private const int GpioDc = 24;
        private const int GpioCs = 8;
        private const int GpioBl = 18;
        private const int GpioKey1 = 21;
        private const int GpioKey2 = 20;
        private const int GpioKey3 = 16;
        private const int GpioJoystickUp = 6;
        private const int GpioJoystickDown = 19;
        private const int GpioJoystickLeft = 5;
        private const int GpioJoystickRight = 26;
        private const int GpioJoystickPress = 13;
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
            controller.OpenPin(GpioBl, PinMode.Output);
            _controller.Write(GpioBl, PinValue.High);

            _oled = new SH1106(controller, GpioReset, GpioDc, GpioCs, SpiClockFrequency);

            _keyPinHelpers = new[]
            {
                new KeyPinHelper(controller, GpioKey1, e => GpioKey1Down?.Invoke(this, e),
                    e => GpioKey1Up?.Invoke(this, e)),
                new KeyPinHelper(controller, GpioKey2, e => GpioKey2Down?.Invoke(this, e),
                    e => GpioKey2Up?.Invoke(this, e)),
                new KeyPinHelper(controller, GpioKey3, e => GpioKey3Down?.Invoke(this, e),
                    e => GpioKey3Up?.Invoke(this, e)),
                new KeyPinHelper(controller, GpioJoystickUp, e => GpioJoystickUpDown?.Invoke(this, e),
                    e => GpioJoystickUpUp?.Invoke(this, e)),
                new KeyPinHelper(controller, GpioJoystickDown, e => GpioJoystickDownDown?.Invoke(this, e),
                    e => GpioJoystickDownUp?.Invoke(this, e)),
                new KeyPinHelper(controller, GpioJoystickLeft, e => GpioJoystickLeftDown?.Invoke(this, e),
                    e => GpioJoystickLeftUp?.Invoke(this, e)),
                new KeyPinHelper(controller, GpioJoystickRight, e => GpioJoystickRightDown?.Invoke(this, e),
                    e => GpioJoystickRightUp?.Invoke(this, e)),
                new KeyPinHelper(controller, GpioJoystickPress, e => GpioJoystickKeyDown?.Invoke(this, e),
                    e => GpioJoystickKeyUp?.Invoke(this, e))
            };

            ones = new byte[PageSize];
            //Unsafe.InitBlock(ref ones[0], 255, PageSize);
        }

        public event EventHandler<PinValueChangedEventArgs> GpioKey1Down;
        public event EventHandler<PinValueChangedEventArgs> GpioKey1Up;
        public event EventHandler<PinValueChangedEventArgs> GpioKey2Down;
        public event EventHandler<PinValueChangedEventArgs> GpioKey2Up;
        public event EventHandler<PinValueChangedEventArgs> GpioKey3Down;
        public event EventHandler<PinValueChangedEventArgs> GpioKey3Up;
        public event EventHandler<PinValueChangedEventArgs> GpioJoystickUpDown;
        public event EventHandler<PinValueChangedEventArgs> GpioJoystickUpUp;
        public event EventHandler<PinValueChangedEventArgs> GpioJoystickDownDown;
        public event EventHandler<PinValueChangedEventArgs> GpioJoystickDownUp;
        public event EventHandler<PinValueChangedEventArgs> GpioJoystickLeftDown;
        public event EventHandler<PinValueChangedEventArgs> GpioJoystickLeftUp;
        public event EventHandler<PinValueChangedEventArgs> GpioJoystickRightDown;
        public event EventHandler<PinValueChangedEventArgs> GpioJoystickRightUp;
        public event EventHandler<PinValueChangedEventArgs> GpioJoystickKeyDown;
        public event EventHandler<PinValueChangedEventArgs> GpioJoystickKeyUp;

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

        byte[] ones;
        public void SetBlankScreen()
        {
            for (int page = 0; page < ImagePageCount; page++)
            {
                _oled.ShowPage(page, ones);
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
                    _controller.ClosePin(GpioBl);
                }

                _oled = null;
                _keyPinHelpers = null;
                _controller = null;
                ones = null;
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
