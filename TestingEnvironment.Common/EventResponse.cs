namespace TestingEnvironment.Common
{
    public class EventResponse
    {
        public enum ResponseType
        {
            Ok,
            Abort
        }

        public string Message { get; set; }
        public ResponseType Type { get; set; }
    }
}
