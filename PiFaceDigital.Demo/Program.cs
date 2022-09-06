using System;

namespace PiFace.Demo
{
    internal class Program
    {
        static void Main()
        {
            // Inputs are polled at default 100ms interval
            PiFaceDigital piface = new();

            // Sample callback for input 0 (switch S1) executed on press event
            piface.RegisterCallback(0, false, () => Console.WriteLine("Switch S1 pressed"));

            // Sample callback for input 0 (switch S1) executed on release event
            piface.RegisterCallback(0, true, () => Console.WriteLine("Switch S1 pressed"));

            // Relay 1 is toggled on input1 (switch S2) press event
            piface.RegisterCallback(1, false, () => piface.Relay1.Toggle());

            // Eventually I/O can be accessed directly by using piface.Inputs and piface.Outputs

            Console.WriteLine("Press any key to exit");
        }
    }
}