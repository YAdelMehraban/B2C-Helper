using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace B2C.GraphAPI.Helper.Models
{
  public class Group : DirectoryObject
  {
    public Group()
    {
      NonDeclaredProperties = new Dictionary<string, JToken>();
      Members = new List<string>();
    }

    public Group(ICollection<ExtensionProperty> extensionProperties) : this()
    {
      if (extensionProperties == null)
      {
        throw new ArgumentNullException(nameof(extensionProperties));
      }

      foreach (var ep in extensionProperties)
      {
        NonDeclaredProperties.Add(ep.Name, null);
      }
    }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    [JsonIgnore]
    public List<string> Members { get; set; }

    public string MailNickname => "MailNickNameIsNotRequired";
    public bool MailEnabled => false;
    public bool SecurityEnabled => true;

    [JsonIgnore]
    public string CIDN
    {
      get { return GetExtensionValue("groupCIDN"); }
      set { SetExtensionValue("groupCIDN", value); }
    }
  }
}
