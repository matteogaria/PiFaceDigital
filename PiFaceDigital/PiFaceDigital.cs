using System;
using System.Collections.Generic;
using System.Threading;
using System.Device.Spi;

using Iot.Device.Mcp23xxx;

namespace PiFace
{

    public class PiFaceDigital : IDisposable
    {
        // PiFaceDigital 1 wrapper class
        // based on the limited informations available online about this board
        // I/O extender ports are wired as following:
        // GPIOA: Output port
        //   Relay 0: GPIOA 0
        //   Relay 1: GPIOA 1
        // GPIOB: Input port
        //   Switch 0: GPIOB 0
        //   Switch 1: GPIOB 1
        //   Switch 2: GPIOB 2
        //   Switch 3: GPIOB 3
        // Addressing is NOT used by this class so only one board is allowed
        // (hardware doesn't allow a second board so addressing is useless)

        private readonly SpiDevice spi = null;
        private readonly Mcp23S17Fixed driver = null;
        private readonly Thread inputPolling = null;
        private readonly int pollRate;
        private bool running;

        private Dictionary<(int, bool), List<Action>> callbacks = new();

        public ByteRegister Outputs { get; } = new();
        public ByteRegister Inputs { get; } = new();

        public PiFaceOutput Out0 { get; }
        public PiFaceOutput Out1 { get; }
        public PiFaceOutput Out2 { get; }
        public PiFaceOutput Out3 { get; }
        public PiFaceOutput Out4 { get; }
        public PiFaceOutput Out5 { get; }
        public PiFaceOutput Out6 { get; }
        public PiFaceOutput Out7 { get; }
        public PiFaceOutput Relay0 { get; }
        public PiFaceOutput Relay1 { get; }

        public PiFaceDigital(int pollRate = 100)
        {
            this.pollRate = pollRate;

            spi = SpiDevice.Create(new SpiConnectionSettings(0, 0));
            driver = new Mcp23S17Fixed(spi, 0x20);
            InitBoard();

            Out0 = new PiFaceOutput(0, Outputs);
            Out1 = new PiFaceOutput(1, Outputs);
            Out2 = new PiFaceOutput(2, Outputs);
            Out3 = new PiFaceOutput(3, Outputs);
            Out4 = new PiFaceOutput(4, Outputs);
            Out5 = new PiFaceOutput(5, Outputs);
            Out6 = new PiFaceOutput(6, Outputs);
            Out7 = new PiFaceOutput(7, Outputs);

            Relay0 = Out0;
            Relay1 = Out1;

            inputPolling = new Thread(InputPollingLoop);
            inputPolling.Start();

            Outputs.ValueChange += (_, __) => driver.WriteByte(Register.GPIO, Outputs.ToByte(), Port.PortA);
            Inputs.ValueChange += (pin, value) =>
            {
                if (callbacks.TryGetValue((pin, value), out List<Action> cbList))
                    cbList.ForEach(a => a?.Invoke());
            };
        }

        public void RegisterCallback(int pin, bool value, Action callback)
        {
            if (callbacks.ContainsKey((pin, value)))
                callbacks[(pin, value)].Add(callback);
            else
                callbacks.Add((pin, value), new List<Action> { callback });
        }

        public void Dispose()
        {
            running = false;
            Thread.Sleep(pollRate * 2);
            driver.Dispose();
            spi.Dispose();
            GC.SuppressFinalize(this);
        }

        private void InitBoard()
        {
            // Register IOCON is not initialized because default values are good enough
            // See datasheet for reference
            // https://ww1.microchip.com/downloads/aemDocuments/documents/APID/ProductDocuments/DataSheets/MCP23017-Data-Sheet-DS20001952.pdf

            driver.WriteUInt16(Register.GPIO, 0x0000);   // all ports low
            driver.WriteUInt16(Register.IODIR, 0xFF00);  // PortA as input, PortB as output
            driver.WriteUInt16(Register.GPPU, 0xFF00);   // enables pullup on PortA
        }

        private void InputPollingLoop()
        {
            running = true;
            while (running)
            {
                Thread.Sleep(pollRate);
                Inputs.FromByte(driver.ReadByte(Register.GPIO, Port.PortB));
            }
        }

        public struct PiFaceOutput
        {
            private readonly int bit;
            private readonly ByteRegister register = null;

            public PiFaceOutput(int bit, ByteRegister register)
            {
                this.bit = bit;
                this.register = register;
            }

            public bool State => register.GetBit(bit);

            public void Toggle() => register.SetBit(bit, !State);
            public void Off() => register.SetBit(bit, false);
            public void On() => register.SetBit(bit, true);
        }
    }
}
