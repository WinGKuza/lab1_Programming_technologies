using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    internal class Program
    {
        static void Main()
        {
            const string fileName = "../../file.bin";
            const int arraySize = 10000;

            VirtualMemoryController vmm = new VirtualMemoryController(fileName, arraySize);

            vmm.Write(100, 1);
            vmm.Write(5000, 2);

            int value1 = vmm.Read(100);
            int value2 = vmm.Read(5000);

            Console.WriteLine($"Значение по индексу 100: {value1}");
            Console.WriteLine($"Значение по индексу 5000: {value2}");
            Console.ReadLine();
        }
    }
}
