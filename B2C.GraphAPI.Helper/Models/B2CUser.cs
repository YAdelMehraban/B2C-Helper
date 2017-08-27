using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace B2C.GraphAPI.Helper.Models
{
  public class B2CUser : DirectoryObject
  {
    public B2CUser()
    {
      SignInNames = new List<SignInName>();
      OtherMails = new List<string>();
      Groups = new List<Group>();
      NonDeclaredProperties = new Dictionary<string, JToken>();
    }

    public B2CUser(ICollection<ExtensionProperty> extensionProperties)
      : this()
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


    public bool AccountEnabled { get; set; }
    public List<SignInName> SignInNames { get; set; }
    public string CreationType => "LocalAccount";
    public string DisplayName { get; set; }
    public PasswordProfile PasswordProfile { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string StreetAddress { get; set; }
    public string PostalCode { get; set; }
    public string GivenName { get; set; }
    public string Surname { get; set; }
    public string Mail { get; set; }
    public string Mobile { get; set; }
    public List<string> OtherMails { get; set; }
    [JsonIgnore]
    public List<Group> Groups { get; set; }
    public string TelephoneNumber { get; set; }
    public string FacsimileTelephoneNumber { get; set; }
    public string PreferredLanguage { get; set; }

    

    [JsonIgnore]
    public string CIDN
    {
      get { return GetExtensionValue("cidn"); }
      set { SetExtensionValue("cidn", value); }
    }

    [JsonIgnore]
    public string CustomerName
    {
      get { return GetExtensionValue("CustomerName"); }
      set { SetExtensionValue("CustomerName", value); }
    }

    
  }

}
