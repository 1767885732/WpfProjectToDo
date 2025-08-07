using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfProjectToDo.Common;
using WpfProjectToDo.Common.Events;

namespace WpfProjectToDo.Extensions
{
    public static class DialogExtensions
    {
        /// <summary>
        /// 询问窗口
        /// </summary>
        /// <param name="dialogHost">指定的DialogHost会话主机</param>
        /// <param name="title">标题</param>
        /// <param name="content">询问内容</param>
        /// <param name="dialogHostName">会话主机名称（唯一）</param>
        /// <returns></returns>
        public static async Task<IDialogResult> Question(this IDialogHostService dialogHost,
            string title,string content,string dialogHostName = "Root")
        {
            DialogParameters param = new DialogParameters();
            param.Add("Title",title);
            param.Add("Content",content);   
            param.Add("dialogHostName", dialogHostName);   
            var dialogResult = await dialogHost.ShowDialog("MsgView",param,dialogHostName);
            return dialogResult;
        }

        /// <summary>
        /// 推送等待消息
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="updateModel"></param>
        public static void UpdateLoading(this IEventAggregator eventAggregator, UpdateModel updateModel)
        {
            eventAggregator.GetEvent<UpdateLoadingEvent>().Publish(updateModel);
        }

        /// <summary>
        /// 注册等待消息
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="action"></param>
        public static void Register(this IEventAggregator eventAggregator,Action<UpdateModel> action)
        {
            eventAggregator.GetEvent<UpdateLoadingEvent>().Subscribe(action);
        }

        /// <summary>
        /// 注册提示消息事件
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="action"></param>
        public static void RegisterMessage(this IEventAggregator eventAggregator, Action<MessageModel> action,string filterName = "Main")
        {
            eventAggregator.GetEvent<MessageEvent>().Subscribe(action,
                ThreadOption.PublisherThread, true, (m) =>
                {
                    return m.Filter.Equals(filterName);
                });
        }

        /// <summary>
        /// 发送提示消息
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="message"></param>
        public static void SendMessage(this IEventAggregator eventAggregator, string message, string filterName = "Main")
        {
            eventAggregator.GetEvent<MessageEvent>().Publish(new MessageModel()
            {
                Filter = filterName,
                Message = message
            });
        } 
    }
}
