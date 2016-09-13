using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

namespace Porrey.SensorTelemetry.Views
{
	public abstract class SensorTelemetryPage : SessionStateAwarePage
	{
		private INavigationService _navigationService = null;

		public SensorTelemetryPage()
		{
			this.GoBackCommand = DelegateCommand.FromAsyncHandler(this.OnGoBack, this.OnCanGoBack);
		}

		public virtual INavigationService NavigationService
		{
			get
			{
				if (_navigationService == null)
				{
					_navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
				}

				return _navigationService;
			}
			set
			{
				this._navigationService = value;
			}
		}

		public virtual DelegateCommand GoBackCommand { get; set; }

		public virtual bool CanGoBack
		{
			get
			{
				return this.NavigationService.CanGoBack();
			}
		}

		protected virtual Task OnGoBack()
		{
			this.NavigationService.GoBack();
			return Task.FromResult(0);
		}

		protected virtual bool OnCanGoBack()
		{
			return this.NavigationService.CanGoBack();
		}
	}
}
