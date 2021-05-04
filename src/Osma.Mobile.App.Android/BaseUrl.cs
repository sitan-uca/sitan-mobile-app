using Osma.Mobile.App.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(BaseUrl))]
namespace Osma.Mobile.App.Droid
{
    class BaseUrl : IBaseUrl
    {
        public string Get()
        {
            //var files = Android.App.Application.Context.Assets.List("");
            return "file:///android_asset/"; 
        }
    }
}