using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.ViewInterfaces
{
    /// <summary>
    /// Mark a row as useless: ie does not count in term of used space in paging
    /// </summary>
    public interface ICanBeFakeRow
    {
        bool IsFake { get; set; }
    }
}
