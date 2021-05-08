using System;
using System.Collections.Generic;
using System.Text;

namespace Osma.Mobile.App.Services.Interfaces
{
    public interface IImageCompressionService
    {
        byte[] CompressImage(byte[] imageData, string destinationPath, int compressionPercentage);
    }
}
