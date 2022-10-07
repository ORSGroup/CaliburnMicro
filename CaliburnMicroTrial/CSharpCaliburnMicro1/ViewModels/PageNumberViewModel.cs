using Caliburn.Micro;

namespace CSharpCaliburnMicro1.ViewModels
{
	public class PageNumberViewModel : PropertyChangedBase
	{
		public static int GetCurrentCount()
		{
			return count;
		}
		protected static int count;
		public int TotalPages
		{
			get { return count; }
			set { count = value; NotifyOfPropertyChange(() => TotalPages); }
		}
		protected int page;
		public int Page
		{
			get { return page; }
			set { page = value; NotifyOfPropertyChange(() => Page); }
		}


	}
}
