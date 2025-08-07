using MyToDo.Shared.Dtos;
using MyToDo.Shared.Parameters;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfProjectToDo.Common;
using WpfProjectToDo.Extensions;
using WpfProjectToDo.Service;

namespace WpfProjectToDo.ViewModels
{
    public class ToDoViewModel : NavigationViewModel
    {
        public readonly IDialogHostService dialogHost;
        public ToDoViewModel(IToDoService toDoService, IContainerProvider provider) : base(provider)
        {
            ToDoDtos = new ObservableCollection<ToDoDto>();
            ExecutCommand = new DelegateCommand<string>(Execute);
            SelectedCommand = new DelegateCommand<ToDoDto>(Selected);
            DeleteCommand = new DelegateCommand<ToDoDto>(Delete);
            dialogHost = provider.Resolve<IDialogHostService>();
            this.toDoService = toDoService;

        }

        private async void Delete(ToDoDto dto)
        {
            try
            {
                var dialogResult = await dialogHost.Question("温馨提示", $"确认删除待办事项：{dto.Title}?");

                if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK) return;

                UpdateLoading(true);
                var deleteResult = await toDoService.DeleteAsync(dto.Id);
                if (deleteResult.Status)
                {
                    var model = ToDoDtos.FirstOrDefault(t => t.Id.Equals(dto.Id));
                    if (model != null)
                    {
                        ToDoDtos.Remove(model);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                UpdateLoading(false);
            }
        }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "新增": Add(); break;
                case "查询": GetDataAsync(); break;
                case "保存": Save(); break;
            }
        }

        private int selectedIndex;
        /// <summary>
        /// 下拉列表选择值
        /// </summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { selectedIndex = value; RaisePropertyChanged(); GetDataAsync(); }
        }


        private string search;
        /// <summary>
        /// 搜素条件
        /// </summary>
        public string Search
        {
            get { return search; }
            set { search = value; }
        }


        private bool isRightDrawerOpen;
        /// <summary>
        /// 右侧编辑窗口是否展开
        /// </summary>
        public bool IsRightDrawerOpen
        {
            get { return isRightDrawerOpen; }
            set { isRightDrawerOpen = value; RaisePropertyChanged(); }
        }

        private ToDoDto currentDto;
        /// <summary>
        /// 编辑选中对象/新增对象
        /// </summary>
        public ToDoDto CurrentDto
        {
            get { return currentDto; }
            set { currentDto = value; RaisePropertyChanged(); }
        }


        /// <summary>
        /// 添加待办
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void Add()
        {
            CurrentDto = new ToDoDto();
            IsRightDrawerOpen = true;
        }

        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(CurrentDto.Title) || string.IsNullOrWhiteSpace(CurrentDto.Content))
            { return; }
            UpdateLoading(true);
            try
            {
                if (CurrentDto.Id > 0)
                {
                    var updateResult = await toDoService.UpdateAsync(CurrentDto);
                    if (updateResult.Status)
                    {
                        var todo = ToDoDtos.FirstOrDefault(t => t.Id == CurrentDto.Id);
                        if (todo != null)
                        {
                            todo.Title = CurrentDto.Title;
                            todo.Content = CurrentDto.Content;
                            todo.Status = CurrentDto.Status;
                        }
                    }
                    IsRightDrawerOpen = false;
                }
                else
                {
                    var addResult = await toDoService.AddAsync(CurrentDto);
                    if (addResult.Status)
                    {
                        ToDoDtos.Add(addResult.Result);
                        IsRightDrawerOpen = false;
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                UpdateLoading(false);
            }

        }

        private async void Selected(ToDoDto dto)
        {
            try
            {
                UpdateLoading(true);
                var todoResult = await toDoService.GetFirstOrDefaultAsync(dto.Id);
                if (todoResult.Status)
                {
                    CurrentDto = todoResult.Result;
                    IsRightDrawerOpen = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                UpdateLoading(false);
            }

        }

        public DelegateCommand<string> ExecutCommand { get; set; }
        public DelegateCommand<ToDoDto> SelectedCommand { get; set; }
        public DelegateCommand<ToDoDto> DeleteCommand { get; set; }

        private ObservableCollection<ToDoDto> toDoDtos;
        private readonly IToDoService toDoService;

        public ObservableCollection<ToDoDto> ToDoDtos
        {
            get { return toDoDtos; }
            set { toDoDtos = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        async void GetDataAsync()
        {
            UpdateLoading(true);
            try
            {
                int? Status = SelectedIndex == 0 ? null : SelectedIndex == 2 ? 1 : 0;
                var todoResult = await toDoService.GetAllFilterAsync(new ToDoParameter()
                {
                    PageIndex = 0,
                    PageSize = 100,
                    Search = search,
                    Status = Status,
                });
                if (todoResult.Status && todoResult.Result?.Items != null)
                {
                    ToDoDtos.Clear();
                    foreach (var item in todoResult.Result.Items)
                    {
                        ToDoDtos.Add(item);
                    }
                }
                else
                {
                    // 数据加载失败处理（如提示用户）
                    System.Diagnostics.Debug.WriteLine("待办数据加载失败");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载异常: {ex.Message}");
            }
            UpdateLoading(false);
        }


        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            if(navigationContext.Parameters.ContainsKey("value"))
            {
                SelectedIndex = navigationContext.Parameters.GetValue<int>("value");
            }
            else
            {
                SelectedIndex = 0;
            }
            GetDataAsync();
        }



    }
}
