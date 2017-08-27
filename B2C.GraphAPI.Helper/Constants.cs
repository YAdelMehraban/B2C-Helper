namespace B2C.GraphAPI.Helper
{
  public class Constants
  {
    public class Endpoints
    {
      public const string AllUsers = "/users";
      public const string AllGroups = "/groups";
      public const string GetUser = "/users/";
      public const string CreateUser = "/users";
      public const string UpdateUser = "/users/";
      public const string Applications = "/applications/";
      public const string ApplicationWithQuery = "/applications";
      public const string ExtensionProperties = "/extensionProperties/";
      public const string DeleteUser = "/users/";
      public const string DirectoryObjects = "/directoryObjects/";
    }

    public static class SignType
    {
      public const string SignInTypeEmailAddress = "emailAddress";
      public const string SignInTypeUserName = "username";
    }
  }
}
