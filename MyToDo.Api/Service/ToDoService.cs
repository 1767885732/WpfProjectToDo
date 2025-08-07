using AutoMapper;
using MyToDo.Api.Context;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Parameters;
using System.Collections.ObjectModel; 
using System.Linq;

namespace MyToDo.Api.Service
{
    /// <summary>
    /// 待办事项的实现
    /// </summary>
    public class ToDoService : IToDoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper mapper;

        public ToDoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            this.mapper = mapper;
        }
        public async Task<ApiResponse> AddAsync(ToDoDto entity)
        {
            try
            {
                var todo = mapper.Map<ToDo>(entity);
                await _unitOfWork.GetRepository<ToDo>().InsertAsync(todo);
                if (await _unitOfWork.SaveChangesAsync() > 0) { return new ApiResponse(true, todo); }
                return new ApiResponse(false, "添加数据失败");
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }

        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            try
            {
                var repository = _unitOfWork.GetRepository<ToDo>();
                var todo = await repository.GetFirstOrDefaultAsync(predicate: p => p.Id == id);

                repository.Delete(todo);
                if (await _unitOfWork.SaveChangesAsync() > 0) { return new ApiResponse(true, ""); }
                return new ApiResponse("删除数据失败");
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> GetSingleAsync(int id)
        {
            try
            {
                var repository = _unitOfWork.GetRepository<ToDo>();
                var todo = await repository.GetFirstOrDefaultAsync(predicate: p => p.Id.Equals(id));

                return new ApiResponse(true, todo);

            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> GetAllAsync(QueryParameter parameter)
        {
            try
            {
                var repository = _unitOfWork.GetRepository<ToDo>();
                var todos = await repository.GetPagedListAsync(predicate:
                  p => string.IsNullOrWhiteSpace(parameter.Search) ? true : p.Title.Contains(parameter.Search),
                  pageIndex: parameter.PageIndex,
                  pageSize: parameter.PageSize,
                  orderBy: source => source.OrderByDescending(q => q.CreateDate));
                return new ApiResponse(true, todos);

            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateAsync(ToDoDto entity)
        {
            try
            {
                var dbtodo = mapper.Map<ToDo>(entity);
                var repository = _unitOfWork.GetRepository<ToDo>();
                var todo = await repository.GetFirstOrDefaultAsync(predicate: p => p.Id.Equals(dbtodo.Id));

                todo.Title = dbtodo.Title;
                todo.Content = dbtodo.Content;
                todo.Status = dbtodo.Status;
                todo.UpdateDate = DateTime.Now;
                repository.Update(todo);
                if (await _unitOfWork.SaveChangesAsync() > 0) { return new ApiResponse(true, todo); }
                return new ApiResponse(false, "更新数据失败");
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> GetAllAsync(ToDoParameter parameter)
        {
            try
            {
                var repository = _unitOfWork.GetRepository<ToDo>();
                var todos = await repository.GetPagedListAsync(predicate:
                  p => (string.IsNullOrWhiteSpace(parameter.Search) ? true : p.Title.Contains(parameter.Search))
                  &&parameter.Status == null?true:p.Status.Equals(parameter.Status),
                  pageIndex: parameter.PageIndex,
                  pageSize: parameter.PageSize,
                  orderBy: source => source.OrderByDescending(q => q.CreateDate));
                return new ApiResponse(true, todos);

            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
        }

        public async Task<ApiResponse> Summary()
        {
            try
            {
                //待办事项结果
                var todos = await _unitOfWork.GetRepository<ToDo>().GetAllAsync(
                    orderBy:source=> source.OrderByDescending(t=>t.CreateDate));
               
                //备忘录结果
                var memos = await _unitOfWork.GetRepository<Memo>().GetAllAsync(
                    orderBy:source=> source.OrderByDescending(t=>t.CreateDate));

                SummaryDto summary = new SummaryDto();
                summary.Sum = todos.Count(); //汇总待办事项数量
                summary.CompletedCount = todos.Where(t => t.Status == 1).Count(); //统计完成数量
                summary.CompletedRatio = (summary.CompletedCount / (double)summary.Sum).ToString("0%"); //统计完成率
                summary.MemoeCount = memos.Count();  //汇总备忘录数量
                summary.ToDoList = new ObservableCollection<ToDoDto>(mapper.Map<List<ToDoDto>>(todos.Where(t => t.Status == 0)));
                summary.MemoList = new ObservableCollection<MemoDto>(mapper.Map<List<MemoDto>>(memos));

                return new ApiResponse(true, summary);
            }
            catch (Exception)
            { 
                return new ApiResponse(false, "");
            }
        }
    }
}
