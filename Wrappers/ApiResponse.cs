namespace minibank.Wrappers
{
    public class ApiResponse<T>
    {
        public bool Success{get;set;}
        public string Message{get;set;} = string.Empty;
        public T? Data{get;set;}

        public static ApiResponse<T> SuccessResponse(
            T data ,string message = "Success"
        )=>new ApiResponse<T>{Success=true,Data=data,Message=message};

        public static ApiResponse<T>FailureResponse(string message)
        =>new ApiResponse<T>{Success=false,Message=message};

    }
}