using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using B2C.GraphAPI.Helper.Models;
using System.Linq;
using System;

namespace B2C.GraphAPI.Helper
{
  public class B2CService
  {
    public readonly IB2CClient b2cClient;
    private readonly JsonSerializerSettings jsonSerializerSettings;
    public static List<ExtensionProperty> ExtensionProperties { get; private set; }
    public static string AppObjectId { get; private set; }

    private B2CService(IB2CClient client)
    {
      this.b2cClient = client;
      ExtensionProperties = new List<ExtensionProperty>();
      jsonSerializerSettings = new JsonSerializerSettings
      {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
      };
    }
    public static async Task<B2CService> CreateInstance(IB2CClient client)
    {
      var service = new B2CService(client);
      AppObjectId = await service.GetApplicationObjectIdAsync();
      ExtensionProperties = await service.GetExtensionPropertiesAsync(AppObjectId);

      return service;
    }

    public async Task<string> GetApplicationObjectIdAsync()
    {
      var json = await b2cClient.GetB2CApplicationAsync();
      dynamic appObject = JObject.Parse(json);

      return appObject.value[0].objectId.Value;

    }

    public string GetGroupExtensionPropertyName()
    {
      return ExtensionProperties.First(x=> x.Name.Contains("group")).Name;
    }

    public string GetUserExtensionPropertyName()
    {
      return ExtensionProperties.First(x => x.Name.EndsWith("_cidn", StringComparison.OrdinalIgnoreCase)).Name;
    }

    public async Task<List<ExtensionProperty>> GetExtensionPropertiesAsync(string objectId)
    {
      var json = await b2cClient.GetExtensionPropertiesAsync(objectId);
      dynamic extensions = JObject.Parse(json);

      return JsonConvert.DeserializeObject<List<ExtensionProperty>>(extensions.value.ToString());
    }


    /// <summary>
    /// Returns a blank instance of the B2CUser with any extension properties required added to the 
    /// NonDeclaredProperties collection.
    /// </summary>
    /// <returns>An instance of B2CUser</returns>
    public B2CUser GetBlankNewB2CUser()
    {
      var newUser = new B2CUser(ExtensionProperties.Where(x=> !x.Name.Contains("group")).ToList());

      return newUser;
    }

    /// <summary>
    /// Created a blank group with extension properties
    /// </summary>
    /// <returns>Group</returns>
    public Group GetBlankGroup()
    {
      var newGroup = new Group(ExtensionProperties.Where(x=> x.Name.Contains("group")).ToList());

      return newGroup;
    }

    public async Task<ResponseMessage<List<B2CUser>>> GetAllUsersAysnc(string query = null, bool loadGroup = false)
    {
      var responseMessage = new ResponseMessage<List<B2CUser>>();

      try
      {
        var json = await b2cClient.GetAllUsersAsync(query);

        var adResponse = JsonConvert.DeserializeObject<AzureAdResponseMessage<List<B2CUser>>>(json);

        if (loadGroup)
        {
          foreach (var user in adResponse.Value)
          {
            var group = await GetUserGroups(user.ObjectId);
            user.Groups = group.Data;
          }
        }

        responseMessage.Data = adResponse.Value;
        responseMessage.HttpStatusCode = HttpStatusCode.OK;
      }
      catch (HttpClientException hce)
      {
        responseMessage.ErrorMessage = hce.Message;
        responseMessage.HttpStatusCode = hce.StatusCode;
      }

      return responseMessage;
    }

    public async Task<ResponseMessage<List<Group>>> GetAllGroups(string query = null)
    {
      var responseMessage = new ResponseMessage<List<Group>>();
      try
      {
        var json = await b2cClient.GetAllGroupsAsync(query);
        var adResponse = JsonConvert.DeserializeObject<AzureAdResponseMessage<List<Group>>>(json);

        responseMessage.Data = adResponse.Value;

        foreach (var group in responseMessage.Data)
        {
          json = await b2cClient.GetGroupMembersAsync(group.ObjectId);
          var response = JsonConvert.DeserializeObject<AzureAdResponseMessage<List<DirectoryObjectLink>>>(json);
          group.Members = response.Value.Select(x => x.Url).ToList();
        }

        responseMessage.HttpStatusCode = HttpStatusCode.OK;
      }
      catch (HttpClientException hce)
      {
        responseMessage.ErrorMessage = hce.Message;
        responseMessage.HttpStatusCode = hce.StatusCode;
      }

      return responseMessage;
    }

    public async Task<B2CUser> GetUserAsync(string objectId)
    {
      var json = await b2cClient.GetUserByObjectIdAsync(objectId);

      var user = GetB2CUserFromJson(json);
			
      return user;
    }

    public async Task<Group> GetGroupAsync(string objectId)
    {
      var json = await b2cClient.GetGroupByObjectIdAsync(objectId);

      var group = JsonConvert.DeserializeObject<Group>(json);

      return group;
    }

    public async Task DeleteGroupAsync(string objectId)
    {
      await b2cClient.DeleteGroupByObjectIdAsync(objectId);
    }

    public async Task<ResponseMessage<List<Group>>> GetUserGroups(string objectId)
    {
      var responseMessage = new ResponseMessage<List<Group>>();
      try
      {
        var json = await b2cClient.GetUserGroupsByObjectIdAsync(objectId);

        var adResponse = JsonConvert.DeserializeObject<AzureAdResponseMessage<List<Group>>>(json);
        responseMessage.Data = adResponse.Value;
        responseMessage.HttpStatusCode = HttpStatusCode.OK;
      }
      catch (HttpClientException hce)
      {
        responseMessage.ErrorMessage = hce.Message;
        responseMessage.HttpStatusCode = hce.StatusCode;
      }

      return responseMessage;
    }

    public async Task<B2CUser> GetUserByEmailAsync(string emailAddress)
    {
      var responseMessage = new ResponseMessage<List<B2CUser>>();

      try
      {
        var json = await b2cClient.GetUserByEmailAsync(emailAddress);

        var adResponse = JsonConvert.DeserializeObject<AzureAdResponseMessage<List<B2CUser>>>(json);
        responseMessage.Data = adResponse.Value;
        responseMessage.HttpStatusCode = HttpStatusCode.OK;
      }
      catch (HttpClientException hce)
      {
        responseMessage.ErrorMessage = hce.Message;
        responseMessage.HttpStatusCode = hce.StatusCode;
      }

      return responseMessage.Data?[0];
    }

    public async Task<ResponseMessage<B2CUser>> AddNewUserAsync(B2CUser newUser)
    {
      var json = JsonConvert.SerializeObject(newUser, jsonSerializerSettings);
      var responseMessage = new ResponseMessage<B2CUser>();

      try
      {
        var result = await b2cClient.AddNewUserAsync(json);
        responseMessage.Data = JsonConvert.DeserializeObject<B2CUser>(result);
        responseMessage.HttpStatusCode = HttpStatusCode.Created;
      }
      catch (HttpClientException hce)
      {
        responseMessage.HttpStatusCode = hce.StatusCode;
        responseMessage.ErrorMessage = hce.Message;
      }

      return responseMessage;
    }

    public async Task UpdateUserAsync(string objectId, B2CUserUpdate data)
    {
      var json = JsonConvert.SerializeObject(data, jsonSerializerSettings);
      await b2cClient.UpdateUserAsync(objectId, json);
    }

    public async Task<ResponseMessage<Group>> AddNewGroupAsync(Group newGroup)
    {
      var json = JsonConvert.SerializeObject(newGroup, jsonSerializerSettings);
      var responseMessage = new ResponseMessage<Group>();

      try
      {
        var result = await b2cClient.AddNewGroupAsync(json);
        responseMessage.Data = JsonConvert.DeserializeObject<Group>(result);
        responseMessage.HttpStatusCode = HttpStatusCode.OK;
      }
      catch (HttpClientException hce)
      {
        responseMessage.HttpStatusCode = hce.StatusCode;
        responseMessage.ErrorMessage = hce.Message;
      }

      return responseMessage;
    }

    public async Task<ResponseMessage<Group>> UpdateGroupAsync(string objectId, UpdateGroup group)
    {
      var json = JsonConvert.SerializeObject(group, jsonSerializerSettings);
      var responseMessage = new ResponseMessage<Group>();

      try
      {
        var result = await b2cClient.UpdateGroupAsync(objectId, json);
        responseMessage.Data = JsonConvert.DeserializeObject<Group>(result);
        responseMessage.HttpStatusCode = HttpStatusCode.OK;
      }
      catch (HttpClientException hce)
      {
        responseMessage.HttpStatusCode = hce.StatusCode;
        responseMessage.ErrorMessage = hce.Message;
      }

      return responseMessage;
    }

    public async Task DeleteUserAsync(string objectId)
    {
      await b2cClient.DeleteUserByObjectIdAsync(objectId);
    }

    public async Task AddGroupMemberAsync(string objectId, string memberId)
    {
      await b2cClient.AddGroupMemberAsync(objectId, memberId);
    }

    public async Task DeleteGroupMemberAsync(string objectId, string memberId)
    {
      await b2cClient.DeleteGroupMemberAsync(objectId, memberId);
    }

    private B2CUser GetB2CUserFromJson(string json)
    {
      var user = JsonConvert.DeserializeObject<B2CUser>(json);

      return user;
    }
  }
}
