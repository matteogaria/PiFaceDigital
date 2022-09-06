# PiFaceDigital
An easy to use object for managing the old PiFace I/O board.
Maybe someone, like me, has one of these abandoned under a cover of dust and wants to reuse it...

This board was designed to be installed old Raspberry PI model A/B, but it is compatible also with newer models with some riser headers.

The library makes use of [Iot.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings/) for hardware communication

## Basic Usage

Just create an istance of the PiFaceDigital object: 
```C#
  PiFaceDigital piface = new();
```

To control outputs:

```C#
  piface.Out0.On();
  piface.Out0.Off();
  piface.Out0.Toggle();
  bool state = piface.Out0.State;
```
Available outputs are *Out0* to *Out7*
*Relay0* is an alias for *Out0*, *Relay1* is an alias for *Out1*.

To subscribe an input change:

```C#
// Sample callback for input 0 (switch S1) executed on PRESS
// (or input LOW if screw terminals are used)
piface.RegisterCallback(0, false, () => 
{
  // Your stuff here
});

// Sample callback for input 0 (switch S1) executed on RELEASE 
// (or input HIGH if screw terminals are used)
piface.RegisterCallback(0, true, () => {
  // Your stuff here
});
```

A way to unsubscribe from events is not provided yet, the idea is that you will attach your handlers at the initialization and then use them though the entire application lifetime.

## *Direct* port access
The library provides a way to access the whole PORTA and PORTB of the I/O expander

> The access is not really *direct* as you can do, for example, by reading and writing the GPIO register of the MCP23S17.
> Take a look at the source code for more detail.

**PORTA** manages the board outputs and it's accessible through the **Outputs** property.
Any update to this property is immediately reflected to the outputs.
Using the FromByte method allows you to change more than one port in a single step, but you have to take care for the outputs you *don't want* to change

**PORTB** manages the board inputs and it's mapped as **Inputs** property.
inputs are updated by the library every 100ms by default

> Technically you can't **set** a bit of the input port, but the library
> allows you to do so. Nothing will appen at the hardware level and your
> value will be overvritten by the next input update. The output port
> can be freely read and written.

Methods available are:
```C#
// Get the value of a single bit.
// bit must be in range 0:7
bool GetBit(int bit);
// Set the value of a single bit, leaving the others unchanged.
// bit must be in range 0:7
void SetBit(int bit, bool value)
// Get the whole port value as a byte
byte ToByte();
// Set the whole port from a byte value
void FromByte(byte init)
// Event of value change
// int => the bit changed, 0 to 7
// bool => the current value
event Action<int,bool> ValueChange
```
## External resources
* [PiFace website](http://www.piface.org.uk/products/piface_digital/)
*  [Board schematic](https://github.com/Elektordi/pi-accesscontrol/blob/master/doc/rpBreakOutV0_4_sch.pdf)
* [MCP23S17 Datasheet](https://ww1.microchip.com/downloads/aemDocuments/documents/APID/ProductDocuments/DataSheets/MCP23017-Data-Sheet-DS20001952.pdf)
*  [dotnet/iot Source Code](https://github.com/dotnet/iot) 

## Licensing
This library is free software, released under the [GPLv3 license](https://www.gnu.org/licenses/gpl-3.0.en.html).
