using System.Threading.Tasks;

namespace B2C.GraphAPI.Helper
{
  public interface IB2CClient
  {
    Task<string> GetAllUsersAsync(string query);
    Task<string> GetAllGroupsAsync(string query);
    Task<string> GetGroupByObjectIdAsync(string objectId);
    Task<string> GetGroupMembersAsync(string objectId);
    Task<string> DeleteGroupByObjectIdAsync(string objectId);
    Task<string> GetExtensionPropertiesAsync(string applicationId);
    Task<string> GetUserByObjectIdAsync(string objectId);
    Task<string> GetUserGroupsByObjectIdAsync(string objectId);
    Task<string> GetUserByEmailAsync(string emailAddress);
    Task<string> GetB2CApplicationAsync();
    Task<string> AddNewUserAsync(string json);
    Task<string> AddNewGroupAsync(string json);
    Task<string> UpdateGroupAsync(string objectId, string json);
    Task<string> UpdateUserAsync(string objectId, string json);
    Task<string> DeleteUserByObjectIdAsync(string objectId);
    Task<string> AddGroupMemberAsync(string objectId, string memberId);
    Task<string> DeleteGroupMemberAsync(string objectId, string memberId);
  }
}
