using Newtonsoft.Json;

namespace SampleService
{
    internal class GlobalErrorDetails
    {
  
        public int StatusCodeContext { get; internal set; }
        public string Message { get; internal set; }
        public int StatusCode { get; internal set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}