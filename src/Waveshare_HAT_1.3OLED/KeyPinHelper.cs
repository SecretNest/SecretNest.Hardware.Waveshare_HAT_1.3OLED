using System;
using System.Device.Gpio;

namespace SecretNest.Hardware.Waveshare.HAT_1Point3OLED
{
    class KeyPinHelper : IDisposable
    {
        private GpioController _controller;
        private readonly int _gpio;
        private readonly KeyName _keyName;
        private Action<KeyStateChangedEventArgs> _keyReleasedCallback;
        private Action<KeyStateChangedEventArgs> _keyPressedCallback;
        private bool _disposedValue;

        public KeyPinHelper(GpioController controller, int gpio, KeyName keyName, Action<KeyStateChangedEventArgs> keyPressedCallback,
            Action<KeyStateChangedEventArgs> keyReleasedCallback)
        {
            _controller = controller;
            _gpio = gpio;
            _keyName = keyName;
            _keyReleasedCallback = keyReleasedCallback;
            _keyPressedCallback = keyPressedCallback;

            controller.OpenPin(gpio, PinMode.Input);

            controller.RegisterCallbackForPinValueChangedEvent(gpio, PinEventTypes.Rising, RisingHandler);
            controller.RegisterCallbackForPinValueChangedEvent(gpio, PinEventTypes.Falling, FallingHandler);
        }

        private void RisingHandler(object sender, PinValueChangedEventArgs e)
        {
            _keyReleasedCallback(new KeyStateChangedEventArgs(_keyName, KeyState.Released));
        }

        private void FallingHandler(object sender, PinValueChangedEventArgs e)
        {
            _keyPressedCallback(new KeyStateChangedEventArgs(_keyName, KeyState.Pressed));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _controller.UnregisterCallbackForPinValueChangedEvent(_gpio, RisingHandler);
                    _controller.UnregisterCallbackForPinValueChangedEvent(_gpio, FallingHandler);
                    _controller.ClosePin(_gpio);
                }

                _controller = null;
                _keyReleasedCallback = null;
                _keyPressedCallback = null;

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
