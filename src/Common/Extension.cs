using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using hw.DebugFormatter;


namespace Common
{
  public  static class Extension
    {
        public static byte[] AsciiToByteArray(this string value) => Encoding.ASCII.GetBytes(value);

        public static void Sleep(this TimeSpan value) => Thread.Sleep(value);

        public static TimeSpan Seconds(this int value) => TimeSpan.FromSeconds(value);
        public static void WriteLine(this string value) => Tracer.Line(value);
    }
  
}
