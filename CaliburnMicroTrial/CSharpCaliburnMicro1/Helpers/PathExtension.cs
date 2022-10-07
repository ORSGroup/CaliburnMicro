using System;
using System.IO;

namespace CSharpCaliburnMicro1.Helpers
{
	public static class PathExtension
	{
		public static string GetTempFileWithExtension(string ext)
		{
			var res = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
			var mv = Path.ChangeExtension(res, ext);
			//File.Move(s, mv);
			return mv;
		}
	}
}
