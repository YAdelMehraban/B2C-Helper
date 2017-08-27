using System;
using System.Net;

namespace B2C.GraphAPI.Helper
{
  public class HttpClientException : Exception
  {
    public HttpStatusCode StatusCode { get; set; }

    public HttpClientException(string message) : base(message)
    {
    }
    public HttpClientException(HttpStatusCode statusCode, string message) : base(message)
    {
      StatusCode = statusCode;
    }
  }
}
