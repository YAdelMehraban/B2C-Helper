namespace B2C.GraphAPI.Helper.Models
{
  public class ExtensionProperty
  {

    public string AppDisplayName { get; set; }
    public string Name { get; set; }
    public string DataType { get; set; }
    public string Value { get; set; }

    public string GetDisplayName()
    {
      if (string.IsNullOrEmpty(Name))
      {
        return null;
      }
      var lastUnderscore = Name.LastIndexOf('_');
      return Name.Substring(lastUnderscore + 1);
    }

    public ExtensionProperty Clone()
    {
      return new ExtensionProperty
      {
        AppDisplayName = AppDisplayName,
        Name = Name,
        DataType = DataType,
        Value = Value
      };
    }
  }
}
