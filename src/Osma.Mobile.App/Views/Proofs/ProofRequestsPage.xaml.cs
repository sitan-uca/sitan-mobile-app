using Hyperledger.Aries.Features.PresentProof;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Osma.Mobile.App.Views.Proofs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProofRequestsPage : ContentPage
    {
        public ProofRequestsPage()
        {
            InitializeComponent();

            var segmentedBarTabs = new List<string>() { nameof(ProofState.Requested), nameof(ProofState.Accepted), nameof(ProofState.Proposed)};
            //segmentedBarControl.Children = segmentedBarTabs;
        }
    }
}