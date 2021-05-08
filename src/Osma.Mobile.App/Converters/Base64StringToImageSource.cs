using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace Osma.Mobile.App.Converters
{
    public class Base64StringToImageSource
    {
        public static ImageSource Base64StringToImage(string base64string)
        {
            if (base64string == null)
                return null;            

            try
            {
                var imageBytes = Convert.FromBase64String(base64string);
                var imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                return imageSource;
            }
            catch (FormatException) 
            {
                return null; 
            }

        }
    }
}
