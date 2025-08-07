using MyToDo.Shared.Contact;
using Newtonsoft.Json; // 仅保留Newtonsoft.Json引用
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WpfProjectToDo.Service;

namespace MyToDo.Service
{
    public class ApiHttpClient : IDisposable
    {
        private readonly string _baseApiUrl;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _jsonSettings; // 明确使用Newtonsoft的配置
        private bool _disposed;

        private static readonly Dictionary<Type, PropertyInfo[]> _propertyCache = new Dictionary<Type, PropertyInfo[]>();

        public ApiHttpClient(string baseApiUrl, HttpClient httpClient = null, JsonSerializerSettings jsonSettings = null)
        {
            _baseApiUrl = baseApiUrl ?? throw new ArgumentNullException(nameof(baseApiUrl));
            _httpClient = httpClient ?? new HttpClient();
            // 明确配置Newtonsoft序列化规则
            _jsonSettings = jsonSettings ?? new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            };
        }

        public async Task<ApiResponse> ExecuteAsync(BaseRequest baseRequest)
        {
            return await ExecuteCoreAsync<ApiResponse>(baseRequest);
        }

        public async Task<ApiResponse<T>> ExecuteAsync<T>(BaseRequest baseRequest)
        {
            return await ExecuteCoreAsync<ApiResponse<T>>(baseRequest);
        }

        private async Task<TResponse> ExecuteCoreAsync<TResponse>(BaseRequest baseRequest)
        {
            if (baseRequest == null)
                throw new ArgumentNullException(nameof(baseRequest));

            try
            {
                var requestUrl = CombineUrl(_baseApiUrl, baseRequest.Route);
                var httpMethod = new HttpMethod(baseRequest.Method.ToString());
                var request = new HttpRequestMessage(httpMethod, requestUrl);

                request.Headers.Accept.TryParseAdd("application/json");
                if (!string.IsNullOrEmpty(baseRequest.ContentType))
                {
                    request.Headers.TryAddWithoutValidation("Content-Type", baseRequest.ContentType);
                }

                await SetRequestParametersAsync(request, httpMethod, baseRequest.Parameter);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return string.IsNullOrWhiteSpace(responseContent)
                        ? CreateErrorResponse<TResponse>("响应内容为空")
                        // 明确使用Newtonsoft反序列化
                        : Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(responseContent, _jsonSettings);
                }

                var errorMsg = $"HTTP错误: {(int)response.StatusCode} {response.StatusCode}";
                return CreateErrorResponse<TResponse>($"{errorMsg} - {responseContent}");
            }
            catch (HttpRequestException ex)
            {
                return CreateErrorResponse<TResponse>($"网络请求失败: {ex.Message}");
            }
            // 明确捕获Newtonsoft的JsonException
            catch (Newtonsoft.Json.JsonException ex)
            {
                return CreateErrorResponse<TResponse>($"JSON解析失败: {ex.Message}");
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<TResponse>($"请求处理异常: {ex.Message}");
            }
        }

        private string CombineUrl(string baseUrl, string route)
        {
            if (string.IsNullOrEmpty(route))
                return baseUrl;

            baseUrl = baseUrl.TrimEnd('/');
            route = route.TrimStart('/');
            return $"{baseUrl}/{route}";
        }

        private async Task SetRequestParametersAsync(HttpRequestMessage request, HttpMethod method, object parameter)
        {
            if (parameter == null)
                return;

            if (method == HttpMethod.Get || method == HttpMethod.Head)
            {
                var queryParams = GetQueryParameters(parameter);
                if (!string.IsNullOrEmpty(queryParams))
                {
                    var uriBuilder = new UriBuilder(request.RequestUri);
                    uriBuilder.Query = string.IsNullOrEmpty(uriBuilder.Query)
                        ? queryParams
                        : $"{uriBuilder.Query.Substring(1)}&{queryParams}";
                    request.RequestUri = uriBuilder.Uri;
                }
            }
            else
            {
                // 明确使用Newtonsoft序列化
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(parameter, _jsonSettings);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                await Task.CompletedTask;
            }
        }

        private string GetQueryParameters(object parameter)
        {
            var type = parameter.GetType();
            if (!_propertyCache.TryGetValue(type, out var properties))
            {
                properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .ToArray();
                _propertyCache[type] = properties;
            }

            var queryParams = new List<string>();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(parameter);
                if (value == null)
                    continue;

                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var list = (System.Collections.IEnumerable)value;
                    foreach (var item in list)
                    {
                        if (item != null)
                            queryParams.Add($"{prop.Name}={Uri.EscapeDataString(item.ToString())}");
                    }
                }
                else
                {
                    queryParams.Add($"{prop.Name}={Uri.EscapeDataString(value.ToString())}");
                }
            }

            return string.Join("&", queryParams);
        }

        private TResponse CreateErrorResponse<TResponse>(string message)
        {
            if (typeof(TResponse) == typeof(ApiResponse))
            {
                return (TResponse)(object)new ApiResponse
                {
                    Status = false,
                    Message = message,
                    Result = null
                };
            }

            var genericType = typeof(ApiResponse<>).MakeGenericType(typeof(TResponse).GetGenericArguments()[0]);
            var errorInstance = Activator.CreateInstance(genericType);

            genericType.GetProperty("Status").SetValue(errorInstance, false);
            genericType.GetProperty("Message").SetValue(errorInstance, message);
            genericType.GetProperty("Result").SetValue(errorInstance, null);

            return (TResponse)errorInstance;
        }

        #region 资源释放
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _httpClient?.Dispose();
            }

            _disposed = true;
        }

        ~ApiHttpClient()
        {
            Dispose(false);
        }
        #endregion
    }
}