using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.Helpers
{
    public static class GraphUtils
    {
        public static int CmToDeviceIndependentUnit(double cms)
        {
            return Convert.ToInt32(cms / 2.54 * 96);
        }

        public static double GetA4WideSideDeviceIndependentUnit()
        {
            return 96 * 11.69;
        }

        public static double GetA4NarrowSideDeviceIndependentUnit()
        {
            return 96 * 8.27;
        }

    }
}
