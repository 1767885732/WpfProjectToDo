using AutoMapper;
using MyToDo.Api.Context;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Parameters;
using System.Reflection.Metadata;

namespace MyToDo.Api.Service
{
    /// <summary>
    /// 备忘录的实现
    /// </summary>
    public class MemoService : IMemoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper mapper;

        public MemoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            this.mapper = mapper;
        }
        public async Task<ApiResponse> AddAsync(MemoDto entity)
        {
            try
            {
                var memo = mapper.Map<Memo>(entity);
                await _unitOfWork.GetRepository<Memo>().InsertAsync(memo);
                if (await _unitOfWork.SaveChangesAsync() > 0) { return new ApiResponse(true, memo); }
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
                var repository = _unitOfWork.GetRepository<Memo>();
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
                var repository = _unitOfWork.GetRepository<Memo>();
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
                var repository = _unitOfWork.GetRepository<Memo>();
                var todos = await repository.GetPagedListAsync(predicate:
                  p => string.IsNullOrWhiteSpace(parameter.Search) ? true : p.Title.Equals(parameter.Search),
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

        public async Task<ApiResponse> UpdateAsync(MemoDto entity)
        {
            try
            {
                var dbtodo = mapper.Map<Memo>(entity);
                var repository = _unitOfWork.GetRepository<Memo>();
                var todo = await repository.GetFirstOrDefaultAsync(predicate: p => p.Id.Equals(dbtodo.Id));

                todo.Title = dbtodo.Title;
                todo.Content = dbtodo.Content;
               
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

    }
}
