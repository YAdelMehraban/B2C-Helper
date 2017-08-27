using Newtonsoft.Json;

namespace B2C.GraphAPI.Helper.Models
{
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class AzureAdResponseMessage<T> where T : class
  {
    [JsonProperty(PropertyName = "odata.metadata")]
    public string MetaData { get; set; }
    public T Value { get; set; }
  }
}
