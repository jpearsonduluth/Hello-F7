using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using app = Meadow.App<Meadow.Devices.F7Micro, Hello_F7.MeadowApp>;
namespace Hello_F7
{
    //public class NeoDriver
    //{
    //    private const long ZERO_HIGH = 350;
    //    private const long ONE_HIGH = 700;
    //    private const long DATA_LOW = 600;
    //    private const long LATCH_LOW = 6500;

    //    private static F7Micro Device = app.Device;
    //    IDigitalOutputPort digitalOutputPort = Device.CreateDigitalOutputPort(new Pin("test", "derp"));
    //    IDigitalOutputPort outputPort;

    //    static ISpiBus spiBus = Device.CreateSpiBus();
    //    static IDigitalOutputPort spiPeriphChipSelect = Device.CreateDigitalOutputPort(Device.Pins.ESP_MOSI);
    //    ISpiPeripheral spiPeriph = new SpiPeripheral(spiBus, spiPeriphChipSelect);



    //    /// <summary>
    //    /// Number of DELs in chain 
    //    /// </summary>
    //    public int Size { get; }
    //    private readonly byte[] _buffer;
    //    private readonly byte[] _temp;
    //    private ISpiBus _spi;
    //    /// <summary>
    //    /// Constructor of NeoPixel class
    //    /// </summary>
    //    /// <param name="spi">Identifier of spi bus</param>
    //    /// <param name="numLed">Number of DELs in chain</param>
    //    public NeoDriver(int numLed, IPin pin)
    //    {
    //        Console.WriteLine("new ctor 1");
    //        outputPort = Device.CreateDigitalOutputPort(pin);
    //        Console.WriteLine("new ctor 2");
    //        Size = numLed;
    //        _temp = new byte[4];
    //        _buffer = new byte[Size * 12];
    //        Console.WriteLine("new ctor 3");
    //    }

    //    public void Reset()
    //    {
    //        for (int i = 0; i < _buffer.Length; i++)
    //        {
    //            _buffer[i] = 0x0;
    //        }
    //        Show(); // Send reset cmd

    //        // Initialize _buffer to 0 (0x88 values)
    //        for (int i = 0; i < _buffer.Length; i++)
    //        {
    //            _buffer[i] = 0x88;
    //        }
    //        Show(); // Set all Black
    //    }

    //    /// <summary>
    //    /// Change color of one of DEL
    //    /// </summary>
    //    /// <param name="index">Index (starting from 0) of DEL</param>
    //    /// <param name="r">Value of red component</param>
    //    /// <param name="g">Value of green component</param>
    //    /// <param name="b">Value of blue component</param>
    //    /// <exception cref="ArgumentOutOfRangeException">Could be raised if index param is out of range</exception>
    //    public void ChangeColor(int index, byte r, byte g, byte b)
    //    {
    //        if (index < 0 || index >= Size) throw new ArgumentOutOfRangeException(nameof(index));
    //        var startArray = index * 12;
    //        Compute(g, _temp);
    //        Array.Copy(_temp, 0, _buffer, startArray, 4);
    //        Compute(r, _temp);
    //        Array.Copy(_temp, 0, _buffer, startArray + 4, 4);
    //        Compute(b, _temp);
    //        Array.Copy(_temp, 0, _buffer, startArray + 8, 4);
    //    }

    //    /// <summary>
    //    /// Change color of all DEL
    //    /// </summary>
    //    /// <param name="r">Value of red component</param>
    //    /// <param name="g">Value of green component</param>
    //    /// <param name="b">Value of blue component</param>
    //    public void ChangeAllColor(byte r, byte g, byte b)
    //    {
    //        Compute(g, _temp);
    //        for (int index = 0; index < Size; index++)
    //        {
    //            var startArray = index * 12;
    //            Array.Copy(_temp, 0, _buffer, startArray, 4);
    //        }
    //        Compute(r, _temp);
    //        for (int index = 0; index < Size; index++)
    //        {
    //            var startArray = index * 12;
    //            Array.Copy(_temp, 0, _buffer, startArray + 4, 4);
    //        }
    //        Compute(b, _temp);
    //        for (int index = 0; index < Size; index++)
    //        {
    //            var startArray = index * 12;
    //            Array.Copy(_temp, 0, _buffer, startArray + 8, 4);
    //        }
    //    }

    //    /// <summary>
    //    /// Turn off one DEL
    //    /// </summary>
    //    /// <param name="index">Index (starting from 0) of DEL</param>
    //    public void Off(int index)
    //    {
    //        ChangeColor(index, 0, 0, 0);
    //    }

    //    /// <summary>
    //    /// Turn off all DELs
    //    /// </summary>
    //    public void Off()
    //    {
    //        ChangeAllColor(0, 0, 0);
    //    }

    //    private static void Compute(byte b, byte[] array)
    //    {
    //        for (byte i = 0; i < 4; i++)
    //        {
    //            array[3 - i] = (byte)(((b & (0x01 << (2 * i))) == (0x01 << (2 * i)) ? 0x0c : 0x08) | ((b & (0x01 << (2 * i + 1))) == (0x01 << (2 * i + 1)) ? 0xc0 : 0x80));
    //        }
    //    }


    //    /// <summary>
    //    /// Send buffer to DELs to display
    //    /// </summary>
    //    public void Show()
    //    {
    //        Console.WriteLine("SHOW COLORS");
    //        var wait = (TimeSpan.TicksPerMillisecond / 1000000);

    //        var waitSpan_ONE_HIGH = TimeSpan.FromTicks(wait * LATCH_LOW);
    //        var waitSpan_ZERO_HIGH = TimeSpan.FromTicks(wait * LATCH_LOW);
    //        var waitSpan_DATA_LOW = TimeSpan.FromTicks(wait * LATCH_LOW);
    //        var waitSpan_LATCH_LOW = TimeSpan.FromTicks(wait * LATCH_LOW);

    //        bool bitVal;
    //        foreach (var byt in _buffer)
    //        {
    //            foreach (var bit in new System.Collections.BitArray(byt))
    //            {
    //                bitVal = (bool)bit;
                    
    //                Stopwatch s = new Stopwatch();
    //                outputPort.State = bitVal;
    //                s.Start();
    //                if (bitVal)
    //                {
    //                    while (s.Elapsed < waitSpan_ONE_HIGH) { }
    //                }
    //                else 
    //                {
    //                    while (s.Elapsed < waitSpan_ZERO_HIGH) { }
    //                }
                    
    //                outputPort.State = false;
    //                while (s.Elapsed < waitSpan_DATA_LOW) { }
    //            }

    //            Stopwatch ss = new Stopwatch();
    //            outputPort.State = false; 
    //            ss.Start();
    //            while (ss.Elapsed < waitSpan_LATCH_LOW) { }
                
    //        }
    //    }
    //}
}
