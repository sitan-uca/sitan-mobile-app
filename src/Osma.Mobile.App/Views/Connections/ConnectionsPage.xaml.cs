using Hyperledger.Aries.Features.DidExchange;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Osma.Mobile.App.Views.Connections
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConnectionsPage : ContentPage
	{
		public ConnectionsPage()
		{
			InitializeComponent();

			//var segmentedBarTabs = new List<string>() { nameof(ConnectionState.Connected), nameof(ConnectionState.Invited), nameof(ConnectionState.Negotiating) };
			//segmentedBarControl.Children = segmentedBarTabs;
		}
	}
}