using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1
{
	interface IHasTargetFileName
	{
		string PdfFileName { get; }
		string XpsFileName { get; }
	}
}
