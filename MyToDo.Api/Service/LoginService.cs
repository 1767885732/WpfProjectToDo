using AutoMapper;
using MyToDo.Api.Context;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Extensions;

namespace MyToDo.Api.Service
{
    public class LoginService : ILoginService
    {
        private readonly IUnitOfWork work;
        private readonly IMapper mapper;

        public LoginService(IUnitOfWork work, IMapper mapper)
        {
            this.work = work;
            this.mapper = mapper;
        }
        public async Task<ApiResponse> LoginAsync(string Account, string Password)
        {
            try
            {
                Password = Password.GetMD5();
                var model = await work.GetRepository<User>().GetFirstOrDefaultAsync(predicate:
                    p => (p.Account.Equals(Account)) && (p.PassWord.Equals(Password)));
                if (model == null) { return new ApiResponse("账号或密码错误，请重试"); }
                return new ApiResponse(true, model);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, "登录失败！");
            }
        }

        public async Task<ApiResponse> Register(UserDto user)
        {
            try
            {
                var model = mapper.Map<User>(user);
                var respository = work.GetRepository<User>();

                var userModel = await respository.GetFirstOrDefaultAsync(predicate: p => p.Account.Equals(user.Account));

                if (userModel != null) { return new ApiResponse($"当前账号{model.Account}已存在"); }

                model.PassWord = model.PassWord.GetMD5();
                model.CreateDate = DateTime.Now;
                await respository.InsertAsync(model);
                if (await work.SaveChangesAsync() > 0) { return new ApiResponse(true, model); }
                return new ApiResponse("注册失败！请重试");
            }
            catch (Exception ex)
            {
                return new ApiResponse("注册账号失败！");
            }
        }
    }
}
