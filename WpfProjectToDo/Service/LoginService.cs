using MyToDo.Shared.Contact;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WpfProjectToDo.Service
{
    public class LoginService : ILoginService
    {
        private readonly HttpClient httpClient;
        //private readonly string serviceName = "Login";
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // 忽略JSON属性名的大小写
        };
        public LoginService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<ApiResponse<UserDto>> LoginAsync(UserDto userDto)
        {
            try
            {
                // 登录请求数据处理：确保UserName存在但为空
                var loginData = new UserDto
                {
                    Account = userDto.Account,
                    PassWord = userDto.PassWord,
                    UserName = "" // 避免服务端UserName验证错误
                };

                var json = JsonSerializer.Serialize(loginData, jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/Login/Login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // 构造错误响应（严格使用属性赋值）
                    return new ApiResponse<UserDto>
                    {
                        Status = false,
                        Message = $"登录失败 ({(int)response.StatusCode}): {responseContent}",
                        Result = null
                    };
                }

                // 反序列化服务端返回的成功响应
                return JsonSerializer.Deserialize<ApiResponse<UserDto>>(responseContent, jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<UserDto>
                {
                    Status = false,
                    Message = $"网络请求失败：{ex.Message}",
                    Result = null
                };
            }
            catch (JsonException ex)
            {
                return new ApiResponse<UserDto>
                {
                    Status = false,
                    Message = $"数据解析错误：{ex.Message}",
                    Result = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>
                {
                    Status = false,
                    Message = $"登录处理异常：{ex.Message}",
                    Result = null
                };
            }
        }

        public async Task<ApiResponse> RegisterAsync(UserDto userDto)
        {
            //BaseRequest request = new BaseRequest();
            //request.Method = "POST";
            //request.Route = $"api/{serviceName}/Register";
            //request.Parameter = userDto;
            //return await ExecuteAsync(request);
            try
            {
                // 注册前客户端验证
                if (string.IsNullOrWhiteSpace(userDto.UserName))
                {
                    return new ApiResponse
                    {
                        Status = false,
                        Message = "用户名为必填项",
                        Result = null
                    };
                }

                if (string.IsNullOrWhiteSpace(userDto.Account))
                {
                    return new ApiResponse
                    {
                        Status = false,
                        Message = "账号为必填项",
                        Result = null
                    };
                }

                if (string.IsNullOrWhiteSpace(userDto.PassWord))
                {
                    return new ApiResponse
                    {
                        Status = false,
                        Message = "密码为必填项",
                        Result = null
                    };
                }

                var json = JsonSerializer.Serialize(userDto, jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/Login/Register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse
                    {
                        Status = false,
                        Message = $"注册失败 ({(int)response.StatusCode}): {responseContent}",
                        Result = null
                    };
                }

                return JsonSerializer.Deserialize<ApiResponse>(responseContent, jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse
                {
                    Status = false,
                    Message = $"网络请求失败：{ex.Message}",
                    Result = null
                };
            }
            catch (JsonException ex)
            {
                return new ApiResponse
                {
                    Status = false,
                    Message = $"数据解析错误：{ex.Message}",
                    Result = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Status = false,
                    Message = $"注册处理异常：{ex.Message}",
                    Result = null
                };
            }
        } 
    }
}
