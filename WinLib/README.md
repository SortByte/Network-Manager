# WinLib
C# wrapper for WinAPI and other common functions

Made with Microsoft Visual Studio 2015

To use this library/project in another project you have to do the following:
- download this project to your Visual Studio solution folder
- in Visual Studio, in Solution Explorer, right click the solution and click Add > Existing Project and select WinLib.csproj
- in Visual Studio, in Solution Explorer, right click References of your project and click Add Reference, then check WinLib project and click ok

Now you can use the library:
```C#
using System;
using static WinLib.Network.IP;

namespace ConsoleApplication1
{
    class Program
    {
        private static string destination = "8.8.8.8";

        static void Main(string[] args)
        {
            Console.WriteLine("Sending ping ...");
            Console.WriteLine("Ping to " + destination + " took " + Ping(System.Net.IPAddress.Parse(destination)) + " ms");
            Console.ReadKey();
        }
    }
}
```
