
using Iot.Device.Mcp23xxx;
using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace PiFace
{
    public class Mcp23S17Fixed : Mcp23xxx
    {
        public Mcp23S17Fixed(SpiDevice spiDevice, int deviceAddress, int reset = -1, int interruptA = -1, int interruptB = -1)
           : base(CreateAdapter(spiDevice, deviceAddress), reset, interruptA, interruptB, null)
        { }

        public byte ReadByte(Register register, Port port) => InternalReadByte(register, port);
        public void WriteByte(Register register, byte value, Port port) => InternalWriteByte(register, value, port);
        public ushort ReadUInt16(Register register) => InternalReadUInt16(register);

        public void WriteUInt16(Register register, ushort value) => InternalWriteUInt16(register, value);
        public PinValue ReadInterrupt(Port port) => InternalReadInterrupt(port);

        private static SpiAdapter CreateAdapter(SpiDevice spiDevice, int deviceAddress)
        {
            if (deviceAddress < 0x20 || deviceAddress > 0x27)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceAddress), "The Mcp23s17 address must be between 32 (0x20) and 39 (0x27).");
            }
            return new SpiAdapter(spiDevice, deviceAddress);
        }

        protected override int PinCount => 16;

        protected new class SpiAdapter : BusAdapter
        {
            private SpiDevice _device;
            private int _deviceAddress;

            public SpiAdapter(SpiDevice device, int deviceAddress)
            {
                _device = device;
                _deviceAddress = deviceAddress;
            }

            public override void Dispose() => _device?.Dispose();

            public override void Read(byte registerAddress, Span<byte> buffer)
            {
                // Include OpCode and Register Address.
                Span<byte> writeBuffer = stackalloc byte[]
                {
                    GetOpCode(_deviceAddress, isReadCommand: true),
                    registerAddress,
                    0
                };

                Span<byte> readBuffer = stackalloc byte[writeBuffer.Length];
                // Span<byte> readBuffer = stackalloc byte[buffer.Length + 2];

                // Should this also contain the op code and register?
                // Why are we transferring full duplex if we only really
                // need to read?
                _device.TransferFullDuplex(writeBuffer, readBuffer);

                // First 2 bytes are from sending OpCode and Register Address.
                readBuffer.Slice(2).CopyTo(buffer);
            }

            public override void Write(byte registerAddress, Span<byte> data)
            {
                // Include OpCode and Register Address.
                Span<byte> writeBuffer = stackalloc byte[data.Length + 2];
                writeBuffer[0] = GetOpCode(_deviceAddress, isReadCommand: false);
                writeBuffer[1] = registerAddress;
                data.CopyTo(writeBuffer.Slice(2));

                _device.Write(writeBuffer);
            }

            private static byte GetOpCode(int deviceAddress, bool isReadCommand)
            {
                int opCode = deviceAddress << 1;

                if (isReadCommand)
                {
                    // Set read bit.
                    opCode |= 0b000_0001;
                }

                return (byte)opCode;
            }
        }
    }
}
