using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.ViewInterfaces
{
    public interface IPieChartSlice
    {
        double Percentage { get; set; }
        string Name { get; set; }
    }
}
