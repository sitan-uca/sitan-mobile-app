using Osma.Mobile.App.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Osma.Mobile.App.Services
{
    public class NotificationService
    {
        public static void TriggerNotification(string title, string message)
        {
            INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.SendNotification(title, message);
        }
    }
}
