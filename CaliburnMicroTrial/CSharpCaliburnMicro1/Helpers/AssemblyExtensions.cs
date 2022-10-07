using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCaliburnMicro1.Helpers
{
    public static class AssemblyExtensions
    {
        public static Type GetUniqueAssignableTo(this Assembly assembly, Type assignableFrom)
        {
            Type[] assemblyTypes = assembly.GetTypes();
            Type retType = assemblyTypes.Where(x => assignableFrom.IsAssignableFrom(x)).SingleOrDefault();
            if (retType == null)
            {
                throw new InvalidOperationException(String.Format("No univoque implementation of \"{0}\" has been found in assembly \"{1}\"", typeof(ReportDefinitionBase), assembly));
            }
            return retType;
        }

    }
}
