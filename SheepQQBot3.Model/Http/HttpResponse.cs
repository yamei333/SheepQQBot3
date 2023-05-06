namespace SheepQQBot3.Model
{
    public class HttpResponse<T>
        where T : class
    {
        public HttpResponseResult Result;

        public T Data { get; set; }

        public string ErrorMessage { get; set; }

        public string Source { get; set; }

        public HttpResponse(HttpResponseResult result, T data, string errorMessage, string source)
        {
            Result = result;
            Data = data;
            ErrorMessage = errorMessage;
            Source = source;
        }

        public HttpResponse(HttpResponseResult result, T data)
        {
            Result = result;
            Data = data;
        }
    }

    public enum HttpResponseResult
    {
        Successed,
        TimeOut,
        UnknownError,
    }
}