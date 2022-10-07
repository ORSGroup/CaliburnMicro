using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.ViewModels
{
	/// <summary>
	/// Model representing the cause of breaking a sequence of pages
	/// </summary>
	public class BreakerViewModel
	{
		public BreakerViewModel()
		{
			GroupValue = "UNDEFINED";
		}

		//the grouping value
		public object GroupValue { get; set; }


		public string LeftPart
		{
			get
			{
				var s = GroupValue as string;
				if (s != null)
				{
					int n = s.LastIndexOf('-');
					return s.Substring(0, n + 1);
				}
				else
				{
					return null;
				}
			}
		}
		public string RightPart
		{
			get
			{
				var s = GroupValue as string;
				if (s != null)
				{
					int n = s.LastIndexOf('-');
					return s.Substring(n + 1);
				}
				else
				{
					return null;
				}
			}
		}

	}
}
