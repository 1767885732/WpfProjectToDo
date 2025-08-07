using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfProjectToDo.Common.Events
{
    public class MessageEvent:PubSubEvent<MessageModel>
    {
    }
    public class MessageModel
    {
        public string Message { get; set; }
        public string Filter { get; set; }
    }
}
