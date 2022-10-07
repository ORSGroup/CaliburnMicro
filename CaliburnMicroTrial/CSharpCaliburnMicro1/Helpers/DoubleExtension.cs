using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.Helpers
{
    static class DoubleExtension
    {
        public static double ToAngleSpan(this double d)
        {
            return d * Math.PI * 2 / 100;
        }
    }
}
