using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;

namespace CSharpCaliburnMicro1.Helpers
{
	public static class FixedDocumentExtension
	{
		public static void Save(this FixedDocument doc, string path)
		{
			var paginator = doc.DocumentPaginator;
			var xpsDocument = new XpsDocument(path, FileAccess.Write);
			var documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
			documentWriter.Write(paginator);
			xpsDocument.Close();
		}
	}
}
