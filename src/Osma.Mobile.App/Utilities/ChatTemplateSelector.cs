using Osma.Mobile.App.Views.Connections;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Osma.Mobile.App.Utilities
{
    public class ChatTemplateSelector : DataTemplateSelector
    {
        DataTemplate incomingDataTemplate;
        DataTemplate outgoingDataTemplate;

        public class Message
        {
            public string Text { get; set; }
            public string User { get; set; }
        }

        public ChatTemplateSelector()
        {
            this.incomingDataTemplate = new DataTemplate(typeof(IncommingMsgViewCell));
            this.outgoingDataTemplate = new DataTemplate(typeof(OutgoingMsgViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var messageVm = item as Message;
            if (messageVm == null)
                return null;


            return (messageVm.User == "Me") ? outgoingDataTemplate : incomingDataTemplate;
            //throw new NotImplementedException();
        }
    }
}
