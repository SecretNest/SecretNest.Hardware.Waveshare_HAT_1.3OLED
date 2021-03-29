using System;
using System.Collections;
using System.Device.Gpio;
using System.Drawing;
using System.Threading;
using SecretNest.Hardware.Waveshare.HAT_1Point3OLED;

namespace Test
{
    class Program
    {
        private static Bitmap _screenImage;
        private static Graphics _graphics;
        private static HatDevice _hatDevice;
        private static GpioController _gpioController;
        private static ManualResetEventSlim _waitingToQuit;

        private static readonly Rectangle
            Key1 = new Rectangle(100, 0, 20, 20),
            Key2 = new Rectangle(100, 22, 20, 20),
            Key3 = new Rectangle(100, 44, 20, 20),
            KeyL = new Rectangle(0, 22, 20, 20),
            KeyR = new Rectangle(44, 22, 20, 20),
            KeyU = new Rectangle(22, 0, 20, 20),
            KeyD = new Rectangle(22, 44, 20, 20),
            KeyP = new Rectangle(22, 22, 20, 20);

        static void Main(/*string[] args*/)
        {
            _waitingToQuit = new ManualResetEventSlim(false);
            _gpioController = new GpioController();
            //_hatDevice = new HatDevice(_gpioController, TimeSpan.Zero);
            _hatDevice = new HatDevice(_gpioController, new TimeSpan(0, 0, 0, 0, 50));
            _hatDevice.SetBlankScreen();

            _screenImage = new Bitmap(128, 64);
            _graphics = Graphics.FromImage(_screenImage);
            _graphics.Clear(Color.White);

            _hatDevice.GpioJoystickDownPressed += JoystickDownDown;
            _hatDevice.GpioJoystickDownReleased += JoystickDownUp;
            _hatDevice.GpioJoystickCenterPressed += JoystickKeyDown;
            _hatDevice.GpioJoystickCenterReleased += JoystickKeyUp;
            _hatDevice.GpioJoystickLeftPressed += JoystickLeftDown;
            _hatDevice.GpioJoystickLeftReleased += JoystickLeftUp;
            _hatDevice.GpioJoystickRightPressed += JoystickRightDown;
            _hatDevice.GpioJoystickRightReleased += JoystickRightUp;
            _hatDevice.GpioJoystickUpPressed += JoystickUpDown;
            _hatDevice.GpioJoystickUpReleased += JoystickUpUp;
            _hatDevice.GpioKey1Pressed += Key1Down;
            _hatDevice.GpioKey1Released += Key1Up;
            _hatDevice.GpioKey2Pressed += Key2Down;
            _hatDevice.GpioKey2Released += Key2Up;
            _hatDevice.GpioKey3Pressed += Key3Down;
            _hatDevice.GpioKey3Released += Key3Up;

            Console.CancelKeyPress += Console_CancelKeyPress; //don't link to ctrl-c until function initialized.
            Console.WriteLine("App is started. Press Ctrl-C to quit...");
            _waitingToQuit.Wait();
            Console.WriteLine("App is quitting...");
            _hatDevice.SetBlankScreen();
            _graphics.Dispose();
            _screenImage.Dispose();
            _hatDevice.Dispose();
            _gpioController.Dispose();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; //intercept the normal ctrl-c process, waiting for the program quit gracefully.
            _waitingToQuit.Set();
        }

        private static void Refresh()
        {
            var pageData = new byte[128];
            var bitArray = new BitArray(1024);
            for (var i = 0; i < 8; i++)
            {
                var index = 0;
                var yStart = i * 8;
                for (var x = 0; x < 128; x++)
                {
                    for (var yOffset = 0; yOffset < 8; yOffset++)
                    {
                        bitArray[index++] = IsBlack(_screenImage.GetPixel(x, yStart + yOffset));
                    }
                }
                bitArray.CopyTo(pageData, 0);
                _hatDevice.ShowPage(i, pageData);
            }

            static bool IsBlack(Color color) => color.R == 0 && color.G == 0 && color.B == 0;
        }

        private static void Key1Down(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.Black, Key1);
                Refresh();
            }
        }

        private static void Key1Up(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.White, Key1);
                Refresh();
            }
        }

        private static void Key2Down(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.Black, Key2);
                Refresh();
            }
        }

        private static void Key2Up(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.White, Key2);
                Refresh();
            }
        }

        private static void Key3Down(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.Black, Key3);
                Refresh();
            }
        }

        private static void Key3Up(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.White, Key3);
                Refresh();
            }
        }

        private static void JoystickUpDown(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.Black, KeyU);
                Refresh();
            }
        }

        private static void JoystickUpUp(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.White, KeyU);
                Refresh();
            }
        }

        private static void JoystickDownDown(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.Black, KeyD);
                Refresh();
            }
        }

        private static void JoystickDownUp(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.White, KeyD);
                Refresh();
            }
        }

        private static void JoystickLeftDown(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.Black, KeyL);
                Refresh();
            }
        }

        private static void JoystickLeftUp(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.White, KeyL);
                Refresh();
            }
        }

        private static void JoystickRightDown(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.Black, KeyR);
                Refresh();
            }
        }

        private static void JoystickRightUp(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.White, KeyR);
                Refresh();
            }
        }

        private static void JoystickKeyDown(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.Black, KeyP);
                Refresh();
            }
        }

        private static void JoystickKeyUp(object sender, KeyStateChangedEventArgs e)
        {
            lock (_graphics)
            {
                _graphics.FillRectangle(Brushes.White, KeyP);
                Refresh();
            }
        }
    }
}
