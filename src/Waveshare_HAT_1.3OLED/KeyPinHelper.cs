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

        private State _currentState = State.Unknown;
        private readonly TimeSpan _minimalSignalChangingFrequency;
        private readonly SignalStabilizer<State> _signalStabilizer;

        public KeyPinHelper(GpioController controller, int gpio, KeyName keyName, Action<KeyStateChangedEventArgs> keyPressedCallback,
            Action<KeyStateChangedEventArgs> keyReleasedCallback, TimeSpan minimalSignalChangingFrequency)
        {
            _controller = controller;
            _gpio = gpio;
            _keyName = keyName;
            _keyReleasedCallback = keyReleasedCallback;
            _keyPressedCallback = keyPressedCallback;
            _minimalSignalChangingFrequency = minimalSignalChangingFrequency;

            controller.OpenPin(gpio, PinMode.Input);

            if (minimalSignalChangingFrequency == TimeSpan.Zero)
            {
                controller.RegisterCallbackForPinValueChangedEvent(gpio, PinEventTypes.Rising, RisingHandler);
                controller.RegisterCallbackForPinValueChangedEvent(gpio, PinEventTypes.Falling, FallingHandler);
            }
            else
            {
                _signalStabilizer = new SignalStabilizer<State>(minimalSignalChangingFrequency);
                _signalStabilizer.ValueChanged += SignalStabilizer_ValueChanged;
                controller.RegisterCallbackForPinValueChangedEvent(gpio, PinEventTypes.Rising, RisingWithCheckHandler);
                controller.RegisterCallbackForPinValueChangedEvent(gpio, PinEventTypes.Falling, FallingWithCheckHandler);
            }
        }

        private void SignalStabilizer_ValueChanged(object sender, ValueChangedEventArgs<State> e)
        {
            if (e.Value == State.Pressed)
            {
                if (_currentState != State.Pressed)
                {
                    _currentState = State.Pressed;
                    _keyPressedCallback(new KeyStateChangedEventArgs(_keyName, KeyState.Pressed));
                }
            }
            else if (e.Value == State.Released)
            {
                if (_currentState == State.Pressed)
                {
                    _currentState = State.Released;
                    _keyReleasedCallback(new KeyStateChangedEventArgs(_keyName, KeyState.Released));
                }
                else if (_currentState == State.Unknown)
                {
                    _currentState = State.Pressed;
                    _keyPressedCallback(new KeyStateChangedEventArgs(_keyName, KeyState.Pressed));
                    _keyReleasedCallback(new KeyStateChangedEventArgs(_keyName, KeyState.Released));
                }
            }
        }

        private void RisingHandler(object sender, PinValueChangedEventArgs e)
        {
            _keyReleasedCallback(new KeyStateChangedEventArgs(_keyName, KeyState.Released));
        }

        private void FallingHandler(object sender, PinValueChangedEventArgs e)
        {
            _keyPressedCallback(new KeyStateChangedEventArgs(_keyName, KeyState.Pressed));
        }

        private void RisingWithCheckHandler(object sender, PinValueChangedEventArgs e)
        {
            _signalStabilizer.SetValue(State.Released);
        }

        private void FallingWithCheckHandler(object sender, PinValueChangedEventArgs e)
        {
            _signalStabilizer.SetValue(State.Pressed);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_minimalSignalChangingFrequency == TimeSpan.Zero)
                    {
                        _controller.UnregisterCallbackForPinValueChangedEvent(_gpio, RisingHandler);
                        _controller.UnregisterCallbackForPinValueChangedEvent(_gpio, FallingHandler);
                    }
                    else
                    {
                        _controller.UnregisterCallbackForPinValueChangedEvent(_gpio, RisingWithCheckHandler);
                        _controller.UnregisterCallbackForPinValueChangedEvent(_gpio, FallingWithCheckHandler);
                        _signalStabilizer.ValueChanged -= SignalStabilizer_ValueChanged;
                        _signalStabilizer.Dispose();
                    }
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

        enum State
        {
            Unknown,
            Pressed,
            Released
        }
    }
}
