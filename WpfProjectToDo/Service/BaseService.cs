using MyToDo.Shared.Contact;
using MyToDo.Shared.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WpfProjectToDo.Service
{
    public class BaseService<TEntity>:IBaseService<TEntity> where TEntity : class
    {
        private readonly HttpClient httpClient;
        private readonly string serviceName;
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // 忽略JSON属性名的大小写
        };

        public BaseService(HttpClient httpClient, string serviceName)
        {
            this.httpClient = httpClient;
            this.serviceName = serviceName;
        }

        public async Task<ApiResponse<TEntity>> AddAsync(TEntity entity)
        {
            //var response = await httpClient.PostAsJsonAsync($"api/{serviceName}/Add", entity);
            //return await ProcessResponse<ApiResponse<TEntity>>(response);
            var request = new BaseRequest
            {
                Method = "POST",
                Route = $"api/{serviceName}/Add",
                Parameter = entity
            };
            return await ExecuteAsync<TEntity>(request);
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            //var response = await httpClient.DeleteAsync($"api/{serviceName}/Delete?id={id}");
            //return await ProcessResponse<ApiResponse>(response);
            var request = new BaseRequest
            {
                Method = "DELETE",
                Route = $"api/{serviceName}/Delete?id={id}"
            };
            return await ExecuteAsync(request);
        }

        public async Task<ApiResponse<PagedList<TEntity>>> GetAllAsync(QueryParameter parameter)
        {
            //var url = $"api/{serviceName}/GetAll?pageIndex={parameter.PageIndex}" +
            //           $"&pageSize={parameter.PageSize}" +
            //           $"&search={Uri.EscapeDataString(parameter.Search ?? string.Empty)}";

            //var response = await httpClient.GetAsync(url);
            //return await ProcessResponse<ApiResponse<PagedList<TEntity>>>(response);
            // 构建基础URL（不含Search参数）
            var baseUrl = $"api/{serviceName}/GetAll?PageIndex={parameter.PageIndex}&PageSize={parameter.PageSize}";

            // 仅当Search不为null且不为空字符串时，才添加Search参数
            var url = baseUrl;
            if (!string.IsNullOrWhiteSpace(parameter.Search))
            {
                url += $"&Search={Uri.EscapeDataString(parameter.Search)}";
            }

            var request = new BaseRequest
            {
                Method = "GET",
                Route = url
            };
            return await ExecuteAsync<PagedList<TEntity>>(request);
          
        }

        public async Task<ApiResponse<TEntity>> GetFirstOrDefaultAsync(int id)
        {
            //var response = await httpClient.GetAsync($"api/{serviceName}/Get?id={id}");
            //return await ProcessResponse<ApiResponse<TEntity>>(response);
            var request = new BaseRequest
            {
                Method = "GET",
                Route = $"api/{serviceName}/Get?id={id}"
            };
            return await ExecuteAsync<TEntity>(request);
        }

        public async Task<ApiResponse<TEntity>> UpdateAsync(TEntity entity)
        {
            //var response = await httpClient.PutAsJsonAsync($"api/{serviceName}/Update", entity);
            //return await ProcessResponse<ApiResponse<TEntity>>(response);
            var request = new BaseRequest
            {
                Method = "PUT",
                Route = $"api/{serviceName}/Update",
                Parameter = entity
            };
            return await ExecuteAsync<TEntity>(request);
        }

        // 核心执行方法，处理各种类型的BaseRequest
        public async Task<ApiResponse<TResult>> ExecuteAsync<TResult>(BaseRequest request)
        {
            HttpRequestMessage httpRequest = CreateHttpRequestMessage(request);
            var response = await httpClient.SendAsync(httpRequest);
            return await ProcessResponse<ApiResponse<TResult>>(response);
        }

        // 无泛型结果的执行方法
        public async Task<ApiResponse> ExecuteAsync(BaseRequest request)
        {
            HttpRequestMessage httpRequest = CreateHttpRequestMessage(request);
            var response = await httpClient.SendAsync(httpRequest);
            return await ProcessResponse<ApiResponse>(response);
        }

        // 创建HttpRequestMessage
        private HttpRequestMessage CreateHttpRequestMessage(BaseRequest request)
        {
            var httpRequest = new HttpRequestMessage(
                new HttpMethod(request.Method),
                request.Route
            );

            // 如果有参数且不是GET/HEAD请求，添加请求内容
            if (request.Parameter != null &&
                request.Method != "GET" &&
                request.Method != "HEAD")
            {
                var json = JsonSerializer.Serialize(request.Parameter, jsonOptions);
                httpRequest.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    request.ContentType
                );
            }

            return httpRequest;
        }

        /// <summary>
        /// 处理HTTP响应，反序列化并验证状态
        /// </summary>
        private async Task<T> ProcessResponse<T>(HttpResponseMessage response)
        {
            // 确保确保HTTP请求成功
            response.EnsureSuccessStatusCode();

            // 读取并反序列化响应内容
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, jsonOptions);
        }
    }
}
