using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace B2C.GraphAPI.Helper.Models
{
  public class DirectoryObject
  {
    public DateTime? DeletionTimestamp { get; set; }
    public string ObjectId { get; set; }
    public string ObjectType { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> NonDeclaredProperties { get; set; }

    protected void SetExtensionValue(string propertyName, string value)
    {
      var keyValue = NonDeclaredProperties.FirstOrDefault(
        kp => kp.Key.EndsWith(propertyName, StringComparison.OrdinalIgnoreCase));
      if (!string.IsNullOrEmpty(keyValue.Key))
      {
        NonDeclaredProperties[keyValue.Key] = value;
      }


    }

    protected string GetExtensionValue(string propertyName)
    {
      var keyValue = NonDeclaredProperties.FirstOrDefault(
        kp => kp.Key.EndsWith(propertyName, StringComparison.OrdinalIgnoreCase));

      return string.IsNullOrEmpty(keyValue.Key) ? null : keyValue.Value.ToString();
    }
  }
}
