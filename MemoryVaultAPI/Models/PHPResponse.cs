namespace MemoryVaultAPI.Models
{
    public class PHPResponse
    {
        public int Status { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }

        public PHPResponse(int status, object data, string message)
        {
            Status = status;
            Data = data;
            Message = message;
        }
    }
}
