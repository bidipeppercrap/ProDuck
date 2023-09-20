namespace ProDuck.Responses
{
    public class Pagination
    {
        public int Count { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }
    public class PaginatedResponse
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Payload { get; set; }
        public Pagination? Pagination { get; set; }

        public PaginatedResponse(Pagination pagination, object payload, string message = "", int statusCode = 200)
        {
            Code = statusCode;
            Message = message == string.Empty ? "Success" : message;
            Payload = payload;
            Pagination = pagination;
        }

        public PaginatedResponse(object payload, Pagination pagination)
        {
            Code = 200;
            Message = "Success";
            Payload = payload;
            Pagination = pagination;
        }

        public PaginatedResponse(object payload)
        {
            Code = 200;
            Payload = payload;
        }
    }
}
