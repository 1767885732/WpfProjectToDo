
using MyToDo.Shared.Dtos;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfProjectToDo.Common;
using WpfProjectToDo.Common.Events;
using WpfProjectToDo.Extensions;
using WpfProjectToDo.Service;

namespace WpfProjectToDo.ViewModels.Dialogs
{
    public class LoginViewModel : BindableBase, IDialogAware
    {
        public LoginViewModel(ILoginService loginService, IEventAggregator eventAggregator)
        {
            UserDto = new RegisterUserDto();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            this.loginService = loginService;
            this.eventAggregator = eventAggregator;
        }

        public string Title { get; set; } = "ToDo";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            LoginOut();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        #region Login

        private int selectIndex;

        public int SelectIndex
        {
            get { return selectIndex; }
            set { selectIndex = value; RaisePropertyChanged(); }
        }


        public DelegateCommand<string> ExecuteCommand { get; private set; }

        private string account;

        public string Account
        {
            get { return account; }
            set { account = value;  RaisePropertyChanged(); }
        }


        private string userName;

        public string UserName
        {
            get { return userName; }
            set { userName = value; RaisePropertyChanged(); }
        }

        private string passWord;
        private readonly ILoginService loginService;
        private readonly IEventAggregator eventAggregator;

        public string PassWord
        {
            get { return passWord; }
            set { passWord = value; RaisePropertyChanged(); }
        }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "Login": Login(); break;
                case "LoginOut": LoginOut(); break;
                case "Resgiter": Register(); break;
                case "ResgiterPage": SelectIndex = 1; break;
                case "Return": SelectIndex = 0; break;
            }
        }

        private RegisterUserDto userDto;

        public RegisterUserDto UserDto
        {
            get { return userDto; }
            set { userDto = value; RaisePropertyChanged(); }
        }

        async void Login()
        {
            if (string.IsNullOrWhiteSpace(Account) ||
                string.IsNullOrWhiteSpace(PassWord))
            {
                return;
            }

            var loginResult = await loginService.LoginAsync(new MyToDo.Shared.Dtos.UserDto()
            {
                Account = Account,
                PassWord = PassWord,
               
            });

            if (loginResult != null && loginResult.Status)
            {
                AppSession.UserName = loginResult.Result.UserName;
                //AppSession.UserId = loginResult.Result.Id;
                // 发布事件：通知其他ViewModel用户名已更新
                eventAggregator.GetEvent<LoginStatusChangedEvent>().Publish(loginResult.Result.UserName);
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            }
            else
            {
                //登录失败提示...
                eventAggregator.SendMessage(loginResult.Message, "Login");
            }
        }

        private async void Register()
        {
            if (string.IsNullOrWhiteSpace(UserDto.Account) ||
                string.IsNullOrWhiteSpace(UserDto.UserName) ||
                string.IsNullOrWhiteSpace(UserDto.PassWord) ||
                string.IsNullOrWhiteSpace(UserDto.NewPassWord))
            {
                eventAggregator.SendMessage("请输入完整的注册信息！", "Login");
                return;
            }

            if (UserDto.PassWord != UserDto.NewPassWord)
            {
                eventAggregator.SendMessage("密码不一致,请重新输入！", "Login");
                return;
            }

            var resgiterResult = await loginService.RegisterAsync(new MyToDo.Shared.Dtos.UserDto()
            {
                Account = UserDto.Account,
                UserName = UserDto.UserName,
                PassWord = UserDto.PassWord
            });

            if (resgiterResult != null && resgiterResult.Status)
            {
                eventAggregator.SendMessage("注册成功", "Login");
                //注册成功,返回登录页页面
                SelectIndex = 0;
            }
            else {
                eventAggregator.SendMessage(resgiterResult.Message, "Login");
            }
           
        }

        void LoginOut()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.No));
        }

        #endregion
    }
}
