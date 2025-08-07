using MyToDo.Shared.Dtos;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WpfProjectToDo.Common;
using WpfProjectToDo.Common.Events;
using WpfProjectToDo.Common.Models;
using WpfProjectToDo.Extensions;
using WpfProjectToDo.Service;

namespace WpfProjectToDo.ViewModels
{
    public class IndexViewModel : NavigationViewModel
    {
        private readonly IToDoService toDoService;
        private readonly IMemoService memoService;
        private readonly IDialogHostService dialog;
        private readonly IRegionManager regionManager;


        public IndexViewModel(IDialogHostService dialog, IContainerProvider provider) : base(provider)
        {
            Title = $"您好，{AppSession.UserName} {DateTime.Now.ToString("yyyy-MM-dd: ddd")}";
            eventAggregator.GetEvent<LoginStatusChangedEvent>().Subscribe(OnUserNameUpdated);

            CreatTaskBars(); 
            ExecuteCommand = new DelegateCommand<string>(Excute);
            this.toDoService = provider.Resolve<IToDoService>();
            this.memoService = provider.Resolve<IMemoService>();
            this.dialog = dialog;
            this.regionManager = provider.Resolve<IRegionManager>();
            EditToDoCommand = new DelegateCommand<ToDoDto>(AddToDo);
            EditMemoCommand = new DelegateCommand<MemoDto>(AddMemo);
            ToDoCompltedCommand = new DelegateCommand<ToDoDto>(completed);
            NavigateCommand = new DelegateCommand<TaskBar>(Navigate);
        }
        private void OnUserNameUpdated(string newUserName)
        {
            Title = $"您好，{newUserName} {DateTime.Now.ToString("yyyy-MM-dd: ddd")}";
        }
        private void Navigate(TaskBar bar)
        {
            if (string.IsNullOrWhiteSpace(bar.Target))
            {
                return;
            }
            NavigationParameters param = new NavigationParameters();
            if (bar.Title == "已完成")
            {
                param.Add("value", 2);
            }
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(bar.Target, param);

        }

        private async void completed(ToDoDto dto)
        {
            try
            {
                UpdateLoading(true);
                var updateResult = await toDoService.UpdateAsync(dto);
                if (updateResult.Status)
                {
                    var todo = summary.ToDoList.FirstOrDefault(t => t.Id.Equals(dto.Id));
                    if (todo != null)
                    {
                        summary.ToDoList.Remove(todo);
                        summary.CompletedCount += 1;
                        summary.CompletedRatio = (summary.CompletedCount / (double)summary.Sum).ToString("0%");
                        Refresh();
                    }
                    eventAggregator.SendMessage("已完成");
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                UpdateLoading(false);
            }

        }

        public DelegateCommand<ToDoDto> ToDoCompltedCommand { get; set; }
        public DelegateCommand<ToDoDto> EditToDoCommand { get; set; }
        public DelegateCommand<MemoDto> EditMemoCommand { get; set; }
        public DelegateCommand<string> ExecuteCommand { get; set; }

        public DelegateCommand<TaskBar> NavigateCommand { get; set; }

        #region 属性

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }


        private ObservableCollection<TaskBar> taskBars;

        public ObservableCollection<TaskBar> TaskBars
        {
            get { return taskBars; }
            set { taskBars = value; RaisePropertyChanged(); }
        }

        //private ObservableCollection<ToDoDto> toDoDtos;

        //public ObservableCollection<ToDoDto> ToDoDtos
        //{
        //    get { return toDoDtos; }
        //    set { toDoDtos = value; RaisePropertyChanged(); }
        //}
        //private ObservableCollection<MemoDto> memoDtos;


        //public ObservableCollection<MemoDto> MemoDtos
        //{
        //    get { return memoDtos; }
        //    set { memoDtos = value; RaisePropertyChanged(); }
        //}

        private SummaryDto summary;
        /// <summary>
        /// 首页统计
        /// </summary>
        public SummaryDto Summary
        {
            get { return summary; }
            set { summary = value; RaisePropertyChanged(); }
        }

        #endregion

        private void Excute(string obj)
        {
            switch (obj)
            {
                case "新增待办": AddToDo(null); break;
                case "新增备忘录": AddMemo(null); break;
            }
        }
        /// <summary>
        /// 添加备忘录
        /// </summary>
        /// <returns></returns>
        private async void AddMemo(MemoDto model)
        {
            DialogParameters param = new DialogParameters();
            if (model != null) { param.Add("value", model); }

            var dialogResult = await dialog.ShowDialog("AddMemoView", param);
            if (dialogResult.Result == ButtonResult.OK)
            {
                var memo = dialogResult.Parameters.GetValue<MemoDto>("value");
                if (memo.Id > 0)
                {
                    var updateResult = await memoService.UpdateAsync(memo);
                    if (updateResult.Status)
                    {
                        var memoModel = summary.MemoList.FirstOrDefault(t => t.Id.Equals(memo.Id));
                        if (memoModel != null)
                        {
                            memoModel.Title = memo.Title;
                            memoModel.Content = memo.Content;
                        }
                    }
                }
                else
                {
                    var addResult = await memoService.AddAsync(memo);
                    if (addResult.Status)
                    {
                        summary.MemoList.Add(addResult.Result);
                        summary.MemoeCount += 1;
                        Refresh();
                    }
                }
            }
        }
        /// <summary>
        /// 添加待办事项
        /// </summary>
        private async void AddToDo(ToDoDto model)
        {
            DialogParameters param = new DialogParameters();
            if (model != null) { param.Add("value", model); }

            var dialogResult = await dialog.ShowDialog("AddToDoView", param);
            if (dialogResult.Result == ButtonResult.OK)
            {
                try
                {
                    UpdateLoading(true);
                    var todo = dialogResult.Parameters.GetValue<ToDoDto>("value");
                    if (todo.Id > 0)
                    {
                        var updateResult = await toDoService.UpdateAsync(todo);
                        if (updateResult.Status)
                        {
                            var todoModel = summary.ToDoList.FirstOrDefault(t => t.Id.Equals(todo.Id));
                            if (todoModel != null)
                            {
                                todoModel.Title = todo.Title;
                                todoModel.Content = todo.Content;
                            }
                        }
                    }
                    else
                    {
                        var addResult = await toDoService.AddAsync(todo);
                        if (addResult.Status)
                        {
                            summary.Sum += 1;
                            summary.ToDoList.Add(addResult.Result);
                            summary.CompletedRatio = (summary.CompletedCount / (double)summary.Sum).ToString("0%");
                            Refresh();
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    UpdateLoading(false);
                }
                
            }
        }

        void CreatTaskBars()
        {
            TaskBars = new ObservableCollection<TaskBar>();
            TaskBars.Add(new TaskBar() { Icon = "ClockFast", Title = "汇总", Color = "#FF0CA0FF", Target = "ToDoView" });
            TaskBars.Add(new TaskBar() { Icon = "ClockCheckOutline", Title = "已完成", Color = "#FF1ECA3A", Target = "ToDoView" });
            TaskBars.Add(new TaskBar() { Icon = "ChartLineVariant", Title = "完成率", Color = "#FF02C6DC", Target = "" });
            TaskBars.Add(new TaskBar() { Icon = "PlaylistStar", Title = "备忘录", Color = "#FFFFA000", Target = "MemoView" });
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            var summaryResult = await toDoService.GetSummaryAsync();
            if (summaryResult.Status)
            {
                Summary = summaryResult.Result;
                Refresh();
            }
            base.OnNavigatedFrom(navigationContext);
        }

        void Refresh()
        {
            TaskBars[0].Content = summary.Sum.ToString();
            TaskBars[1].Content = summary.CompletedCount.ToString();
            TaskBars[2].Content = summary.CompletedRatio;
            TaskBars[3].Content = summary.MemoeCount.ToString();
        }
    }
}
