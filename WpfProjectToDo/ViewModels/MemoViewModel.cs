using MaterialDesignThemes.Wpf;
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
    public class MemoViewModel: NavigationViewModel
    {
        public readonly IDialogHostService dialogHost;
        public MemoViewModel(IMemoService memoService, IContainerProvider provider) : base(provider)
        {
            MemoDtos = new ObservableCollection<MemoDto>();
            ExecutCommand = new DelegateCommand<string>(Execute);
            SelectedCommand = new DelegateCommand<MemoDto>(Selected);
            DeleteCommand = new DelegateCommand<MemoDto>(Delete);
            dialogHost = provider.Resolve<IDialogHostService>();
            this.memoService = memoService;

        }

        private async void Delete(MemoDto dto)
        {
            try
            {
                var dialogResult = await dialogHost.Question("温馨提示", $"确认删除备忘录：{dto.Title}?");

                if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK) return;

                UpdateLoading(true);
                var deleteResult = await memoService.DeleteAsync(dto.Id);
                if (deleteResult.Status)
                {
                    var model = MemoDtos.FirstOrDefault(t => t.Id.Equals(dto.Id));
                    if (model != null)
                    {
                        MemoDtos.Remove(model);
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

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "新增": Add(); break;
                case "查询": GetDataAsync(); break;
                case "保存": Save(); break;
            }
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

        private MemoDto currentDto;
        /// <summary>
        /// 编辑选中对象/新增对象
        /// </summary>
        public MemoDto CurrentDto
        {
            get { return currentDto; }
            set { currentDto = value; RaisePropertyChanged(); }
        }


        /// <summary>
        /// 添加备忘录
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void Add()
        {
            CurrentDto = new MemoDto();
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
                    var updateResult = await memoService.UpdateAsync(CurrentDto);
                    if (updateResult.Status)
                    {
                        var todo = MemoDtos.FirstOrDefault(t => t.Id == CurrentDto.Id);
                        if (todo != null)
                        {
                            todo.Title = CurrentDto.Title;
                            todo.Content = CurrentDto.Content; 
                        }
                    }
                    IsRightDrawerOpen = false;
                }
                else
                {
                    var addResult = await memoService.AddAsync(CurrentDto);
                    if (addResult.Status)
                    {
                        MemoDtos.Add(addResult.Result);
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

        private async void Selected(MemoDto dto)
        {
            try
            {
                UpdateLoading(true);
                var memoResult = await memoService.GetFirstOrDefaultAsync(dto.Id);
                if (memoResult.Status)
                {
                    CurrentDto = memoResult.Result;
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
        public DelegateCommand<MemoDto> SelectedCommand { get; set; }
        public DelegateCommand<MemoDto> DeleteCommand { get; set; }

        private ObservableCollection<MemoDto> memoDtos;
        private readonly IMemoService memoService;

        public ObservableCollection<MemoDto> MemoDtos
        {
            get { return memoDtos; }
            set { memoDtos = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        async void GetDataAsync()
        {
            UpdateLoading(true);
            try
            {
                var memoResult = await memoService.GetAllAsync(new QueryParameter()
                {
                    PageIndex = 0,
                    PageSize = 100,
                    Search = search,
                });
                if (memoResult.Status && memoResult.Result?.Items != null)
                {
                    MemoDtos.Clear();
                    foreach (var item in memoResult.Result.Items)
                    {
                        MemoDtos.Add(item);
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
            GetDataAsync();
        }
    }
}
