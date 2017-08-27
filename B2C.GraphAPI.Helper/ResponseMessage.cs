using System.Net;

namespace B2C.GraphAPI.Helper
{
  public class ResponseMessage<T>
  {
    public HttpStatusCode HttpStatusCode { get; set; }
    public string ErrorMessage { get; set; }
    public T Data { get; set; }
  }
}
