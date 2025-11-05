namespace Stump.Server.Tools.DatabaseManager.Services
{
    public class ServiceResult
    {
        private ServiceResult(bool success, string message = null)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; }

        public string Message { get; }

        public static ServiceResult Ok(string message = null)
        {
            return new ServiceResult(true, message);
        }

        public static ServiceResult Failed(string message)
        {
            return new ServiceResult(false, message);
        }
    }
}
