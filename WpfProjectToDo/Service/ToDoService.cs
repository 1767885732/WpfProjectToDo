using MyToDo.Shared.Contact;
using MyToDo.Shared.Dtos;
using MyToDo.Shared.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;



namespace WpfProjectToDo.Service
{
    public class ToDoService : BaseService<ToDoDto>, IToDoService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // 忽略JSON属性名的大小写
        };
        public ToDoService(HttpClient httpClient) : base(httpClient, "ToDo")
        {
            this.httpClient = httpClient;
        }

        public async Task<ApiResponse<PagedList<ToDoDto>>> GetAllFilterAsync(ToDoParameter parameter)
        {

            var baseUrl = $"api/ToDo/GetAll?PageIndex={parameter.PageIndex}&PageSize={parameter.PageSize}&status={parameter.Status}";

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
            return await ExecuteAsync<PagedList<ToDoDto>>(request);

        }
        // 核心执行方法，处理各种类型的BaseRequest
        public async Task<ApiResponse<TResult>> ExecuteAsync<TResult>(BaseRequest request)
        {
            HttpRequestMessage httpRequest = CreateHttpRequestMessage(request);
            var response = await httpClient.SendAsync(httpRequest);
            return await ProcessResponse<ApiResponse<TResult>>(response);
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

        public async Task<ApiResponse<SummaryDto>> GetSummaryAsync()
        {
            BaseRequest request = new BaseRequest();
            request.Route = "api/ToDo/Summary";
            request.Method = "GET";

            return await ExecuteAsync<SummaryDto>(request);
        }
    }
}
