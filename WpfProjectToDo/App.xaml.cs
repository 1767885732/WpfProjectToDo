using Example;
using MyToDo.Service;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;
using WpfProjectToDo.Common;
using WpfProjectToDo.Service;
using WpfProjectToDo.ViewModels;
using WpfProjectToDo.ViewModels.Dialogs;
using WpfProjectToDo.Views;
using WpfProjectToDo.Views.Dialogs;

namespace WpfProjectToDo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {

        protected override Window CreateShell()
        {
            return Container.Resolve<MainView>();
        }

        public static void LoginOut(IContainerProvider containerProvider)
        {
            Current.MainWindow.Hide();
            var dialog = containerProvider.Resolve<IDialogHostService>();
            dialog.ShowDialog("LoginView", callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    Environment.Exit(0);
                    return;
                }
                Current.MainWindow.Show();
                // 重新登录成功后，刷新MainViewModel的UserName
                var mainViewModel = Current.MainWindow.DataContext as MainViewModel;
                mainViewModel?.Configure(); // 调用Configure重新设置UserName
            });
        }

        protected override void OnInitialized()
        {
            var dialog = Container.Resolve<IDialogHostService>();
            dialog.ShowDialog("LoginView", callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    Application.Current.Shutdown();
                    return;
                }
                var service = App.Current.MainWindow.DataContext as IConfigureService;
                if (service != null) { service.Configure(); }
                base.OnInitialized();
            });
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册HttpClient并设置BaseAddress
            containerRegistry.RegisterSingleton<HttpClient>(() =>
            {
                var client = new HttpClient();
                // 设置API基础地址（你的API根地址）
                client.BaseAddress = new Uri("http://localhost:5228/");
                return client;
            });
            containerRegistry.Register<IToDoService, ToDoService>();
            containerRegistry.Register<IMemoService, MemoService>();
            containerRegistry.Register<IDialogHostService, DialogHostService>();
            containerRegistry.Register<ILoginService, LoginService>();

            containerRegistry.RegisterDialog<LoginView, LoginViewModel>();

            containerRegistry.RegisterForNavigation<AddToDoView, AddToDoViewModel>();
            containerRegistry.RegisterForNavigation<AddMemoView, AddMemoViewModel>();

            containerRegistry.RegisterForNavigation<IndexView, IndexViewModel>();
            containerRegistry.RegisterForNavigation<MemoView, MemoViewModel>();
            containerRegistry.RegisterForNavigation<SettingView, SettingViewModel>();
            containerRegistry.RegisterForNavigation<ToDoView, ToDoViewModel>();
            containerRegistry.RegisterForNavigation<SkinView, SkinViewModel>();
            containerRegistry.RegisterForNavigation<AboutView>();
            containerRegistry.RegisterForNavigation<MsgView, MsgViewModel>();
        }


    }

}
