using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Visonize.UsImaging.Infrastructure.CompositionRoot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} Starting Ultrasound Imaging");

            Composer composer = new Composer();

        }
    }
}