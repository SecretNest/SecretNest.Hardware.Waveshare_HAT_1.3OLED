using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

namespace SecretNest.Hardware.IO
{
    class KeyPinHelper : IDisposable
    {
        private GpioController _controller;
        private readonly int _gpio;
        private Action<PinValueChangedEventArgs> _rising;
        private Action<PinValueChangedEventArgs> _falling;
        private bool _disposedValue;

        public KeyPinHelper(GpioController controller, int gpio, Action<PinValueChangedEventArgs> falling,
            Action<PinValueChangedEventArgs> rising)
        {
            _controller = controller;
            _gpio = gpio;
            _rising = rising;
            _falling = falling;

            controller.OpenPin(gpio, PinMode.Input);
            controller.RegisterCallbackForPinValueChangedEvent(gpio, PinEventTypes.Rising, RisingHandler);
            controller.RegisterCallbackForPinValueChangedEvent(gpio, PinEventTypes.Falling, FallingHandler);
        }

        private void RisingHandler(object sender, PinValueChangedEventArgs e)
        {
            _rising(e);
        }

        private void FallingHandler(object sender, PinValueChangedEventArgs e)
        {
            _falling(e);
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
                _rising = null;
                _falling = null;

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
