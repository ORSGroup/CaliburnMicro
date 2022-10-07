using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.ViewInterfaces
{
    public interface IMultipageTarget
    {
        void Add(object a);
        bool Continues { get; set; }
    }
}
