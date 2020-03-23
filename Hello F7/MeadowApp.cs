using System;
using System.Diagnostics;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;

namespace Hello_F7
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        const int pulseDuration = 3000;
        
        public class Color
        {
            public readonly byte Red;
            public readonly byte Green;
            public readonly byte Blue;

            public Color(byte r, byte g, byte b)
            {
                Red = r;
                Blue = b;
                Green = g;
            }
        }

        public MeadowApp()
        {
            Console.WriteLine("starting neo");
            var neo = new NeoDriver(20, Device.Pins.D01);

            byte intensity = 0x10;
            byte halfIntensity = (byte)(intensity / 2);
            byte quarterIntensity = (byte)(halfIntensity / 2);
            byte quarterHalfIntensity = (byte)(halfIntensity + quarterIntensity);

            Console.WriteLine("VARS SET");
            //Color[] background =
            //{
            //    new Color(0,intensity,0),
            //    new Color(0,quarterHalfIntensity,quarterIntensity),
            //    new Color(0,0,intensity),
            //    new Color(quarterIntensity,0,quarterHalfIntensity),
            //    new Color(intensity,0,0),
            //    new Color(quarterHalfIntensity,quarterIntensity,0),
            //    new Color(quarterIntensity,quarterHalfIntensity,0),
            //};



            neo.ChangeAllColor((byte)100, (byte)0, (byte)200);
            neo.Off();
            neo.Show();
            Console.WriteLine("neo started");

            //int r = 10;
            //int g = 50;
            //int b = 100;

            //while (r <= 256 && g <= 256 && b <= 256)
            //{
            //    r += 30; g += 20; b += 10;
            //    Console.WriteLine($"R: {r}; G: {g}; B: {b};");
            //    neo.ChangeAllColor((byte)r, (byte)g, (byte)b);
            //    neo.Off();
            //    Console.WriteLine("showing pixels");
            //    neo.Show();
            //    Console.WriteLine("nappy time");
            //    Thread.Sleep(200);
            //}

        }

        public class NeoDriver
        {
            private const long ZERO_HIGH = 400;
            private const long ONE_HIGH = 700;
            private const long DATA_LOW = 600;
            private const long LATCH_LOW = 6500;

            IDigitalOutputPort outputPort;

            //static ISpiBus spiBus = Device.CreateSpiBus();
            //static IDigitalOutputPort spiPeriphChipSelect = Device.CreateDigitalOutputPort(Device.Pins.ESP_MOSI);
            //ISpiPeripheral spiPeriph = new SpiPeripheral(spiBus, spiPeriphChipSelect);



            /// <summary>
            /// Number of DELs in chain 
            /// </summary>
            public int Size { get; }
            private readonly byte[] _buffer;
            private readonly byte[] _temp;
            /// <summary>
            /// Constructor of NeoPixel class
            /// </summary>
            /// <param name="spi">Identifier of spi bus</param>
            /// <param name="numLed">Number of DELs in chain</param>
            public NeoDriver(int numLed, IPin pin)
            {
                Console.WriteLine("new ctor 1");
                outputPort = Device.CreateDigitalOutputPort(pin);
                Console.WriteLine("new ctor 2");
                Size = numLed;
                _temp = new byte[4];
                _buffer = new byte[Size * 12];
                Console.WriteLine("new ctor 3");
            }

            public void Reset()
            {
                for (int i = 0; i < _buffer.Length; i++)
                {
                    _buffer[i] = 0x0;
                }
                Show(); // Send reset cmd

                // Initialize _buffer to 0 (0x88 values)
                for (int i = 0; i < _buffer.Length; i++)
                {
                    _buffer[i] = 0x88;
                }
                Show(); // Set all Black
            }

            /// <summary>
            /// Change color of one of DEL
            /// </summary>
            /// <param name="index">Index (starting from 0) of DEL</param>
            /// <param name="r">Value of red component</param>
            /// <param name="g">Value of green component</param>
            /// <param name="b">Value of blue component</param>
            /// <exception cref="ArgumentOutOfRangeException">Could be raised if index param is out of range</exception>
            public void ChangeColor(int index, byte r, byte g, byte b)
            {
                if (index < 0 || index >= Size) throw new ArgumentOutOfRangeException(nameof(index));
                var startArray = index * 12;
                Compute(g, _temp);
                Array.Copy(_temp, 0, _buffer, startArray, 4);
                Compute(r, _temp);
                Array.Copy(_temp, 0, _buffer, startArray + 4, 4);
                Compute(b, _temp);
                Array.Copy(_temp, 0, _buffer, startArray + 8, 4);
            }

            /// <summary>
            /// Change color of all DEL
            /// </summary>
            /// <param name="r">Value of red component</param>
            /// <param name="g">Value of green component</param>
            /// <param name="b">Value of blue component</param>
            public void ChangeAllColor(byte r, byte g, byte b)
            {
                Compute(g, _temp);
                for (int index = 0; index < Size; index++)
                {
                    var startArray = index * 12;
                    Array.Copy(_temp, 0, _buffer, startArray, 4);
                }
                Compute(r, _temp);
                for (int index = 0; index < Size; index++)
                {
                    var startArray = index * 12;
                    Array.Copy(_temp, 0, _buffer, startArray + 4, 4);
                }
                Compute(b, _temp);
                for (int index = 0; index < Size; index++)
                {
                    var startArray = index * 12;
                    Array.Copy(_temp, 0, _buffer, startArray + 8, 4);
                }
            }

            /// <summary>
            /// Turn off one DEL
            /// </summary>
            /// <param name="index">Index (starting from 0) of DEL</param>
            public void Off(int index)
            {
                ChangeColor(index, 0, 0, 0);
            }

            /// <summary>
            /// Turn off all DELs
            /// </summary>
            public void Off()
            {
                ChangeAllColor(0, 0, 0);
            }

            private static void Compute(byte b, byte[] array)
            {
                for (byte i = 0; i < 4; i++)
                {
                    array[3 - i] = (byte)(((b & (0x01 << (2 * i))) == (0x01 << (2 * i)) ? 0x0c : 0x08) | ((b & (0x01 << (2 * i + 1))) == (0x01 << (2 * i + 1)) ? 0xc0 : 0x80));
                }
            }


            /// <summary>
            /// Send buffer to DELs to display
            /// </summary>
            public void Show()
            {
                Console.WriteLine("SHOW COLORS");
                var wait = 0.01;//(TimeSpan.TicksPerMillisecond / 1000000);
                Console.WriteLine($"TicksPerMillisecond{TimeSpan.TicksPerMillisecond}");
                Console.WriteLine($"wait{wait}");
                var waitSpan_ONE_HIGH = TimeSpan.FromTicks(7);//(long)wait * ONE_HIGH);
                var waitSpan_ZERO_HIGH = TimeSpan.FromTicks(4);//wait * ZERO_HIGH);
                var waitSpan_DATA_LOW = TimeSpan.FromTicks(6);//wait * DATA_LOW);
                var waitSpan_LATCH_LOW = TimeSpan.FromTicks(65);// wait * LATCH_LOW);
                Console.WriteLine($"waitSpan_ONE_HIGH{waitSpan_ONE_HIGH}");
                Console.WriteLine($"waitSpan_ZERO_HIGH{waitSpan_ZERO_HIGH}");
                Console.WriteLine($"waitSpan_DATA_LOW{waitSpan_DATA_LOW}");
                Console.WriteLine($"waitSpan_LATCH_LOW{waitSpan_LATCH_LOW}");

                bool bitVal;
                foreach (var byt in _buffer)
                {
                    foreach (var bit in new System.Collections.BitArray(byt))
                    {
                        bitVal = (bool)bit;

                        Stopwatch s = new Stopwatch();
                        outputPort.State = bitVal;
                        s.Start();
                        if (bitVal)
                        {
                            while (s.Elapsed < waitSpan_ONE_HIGH) { }
                        }
                        else
                        {
                            while (s.Elapsed < waitSpan_ZERO_HIGH) { }
                        }

                        outputPort.State = false;
                        while (s.Elapsed < waitSpan_DATA_LOW) { }
                    }

                    Stopwatch ss = new Stopwatch();
                    outputPort.State = false;
                    ss.Start();
                    while (ss.Elapsed < waitSpan_LATCH_LOW) { }

                }
            }
        }
    }
}
