using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfProjectToDo.Common.Models;
using WpfProjectToDo.Extensions;

namespace WpfProjectToDo.ViewModels
{
   public class SettingViewModel:BindableBase
    {
        public SettingViewModel(IRegionManager regionManager)
        {
            MenuBars = new ObservableCollection<MenuBar>();
            this.regionManager = regionManager;
            NavigateCommand = new DelegateCommand<MenuBar>(Navigate);
            CreatMenuBar();
        }
        private void Navigate(MenuBar bar)
        {
            if (bar == null || string.IsNullOrWhiteSpace(bar.NameSpace))
                return;
            regionManager.Regions[PrismManager.SettingsViewRegionName].RequestNavigate(bar.NameSpace);

        }
        public DelegateCommand<MenuBar> NavigateCommand { get; private set; }
        private ObservableCollection<MenuBar> menuBars;
        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;

        public ObservableCollection<MenuBar> MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; }
        }

        void CreatMenuBar()
        {
            menuBars.Add(new MenuBar() { Icon = "Palette", Title = "个性化", NameSpace = "SkinView" });
            menuBars.Add(new MenuBar() { Icon = "Cog", Title = "系统设置", NameSpace = "" });
            menuBars.Add(new MenuBar() { Icon = "Information", Title = "关于更多", NameSpace = "AboutView" });
        }
    }

}
