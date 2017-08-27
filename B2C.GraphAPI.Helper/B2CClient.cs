using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace B2C.GraphAPI.Helper
{
  public class B2CClient : IB2CClient, IDisposable
  {
    private readonly AuthenticationContext _authContext;
    private readonly ClientCredential _credential;
    private readonly HttpClient _httpClient;
    private readonly string tenantId;

    private const string GraphBaseUrl = "https://graph.windows.net/";
    private const string AzureAdBaseUrl = "https://login.microsoftonline.com/";
    private const string GraphApiVersion = "api-version=1.6";

    public B2CClient(string tenantId, string clientId, string clientSecret)
    {
      _authContext = new AuthenticationContext(AzureAdBaseUrl + tenantId);
      _credential = new ClientCredential(clientId, clientSecret);
      _httpClient = new HttpClient();
      this.tenantId = tenantId;
    }
    public async Task<string> GetUserByObjectIdAsync(string objectId)
    {
      return await SendGraphGetRequest(Constants.Endpoints.GetUser + objectId, null);
    }

    public async Task<string> GetUserByEmailAsync(string emailAddress)
    {
      return await SendGraphGetRequest(Constants.Endpoints.AllUsers, $"$filter=signInNames/any(x:x/value eq '{emailAddress}')");
    }

    public async Task<string> GetUserGroupsByObjectIdAsync(string objectId)
    {
      return await SendGraphGetRequest($"{Constants.Endpoints.AllUsers}/{objectId}/memberOf", null);
    }

    public async Task<string> GetGroupMembersAsync(string objectId)
    {
      return await SendGraphGetRequest($"{Constants.Endpoints.AllGroups}/{objectId}/$links/members", null);
    }
    public async Task<string> GetB2CApplicationAsync()
    {
      return await SendGraphGetRequest(Constants.Endpoints.ApplicationWithQuery,
        "$filter=startswith(displayName, 'b2c-extensions-app')");
    }
    public async Task<string> AddNewUserAsync(string json)
    {
      return await SendGraphPostRequest(Constants.Endpoints.CreateUser, json);
    }

    public async Task<string> AddNewGroupAsync(string json)
    {
      return await SendGraphPostRequest(Constants.Endpoints.AllGroups, json);
    }

    public async Task<string> UpdateGroupAsync(string objectId, string json)
    {
      return await SendGraphPatchRequest($"{Constants.Endpoints.AllGroups}/{objectId}", json);
    }
    public async Task<string> UpdateUserAsync(string objectId, string json)
    {
      return await SendGraphPatchRequest(Constants.Endpoints.UpdateUser + objectId, json);
    }
    public async Task<string> GetExtensionPropertiesAsync(string applicationId)
    {
      return await SendGraphGetRequest($"{Constants.Endpoints.ApplicationWithQuery}/{applicationId}{Constants.Endpoints.ExtensionProperties}", null);
    }
    public async Task<string> GetAllUsersAsync(string query)
    {
      return await SendGraphGetRequest(Constants.Endpoints.AllUsers, query);
    }

    public async Task<string> GetAllGroupsAsync(string query)
    {
      return await SendGraphGetRequest(Constants.Endpoints.AllGroups, query);
    }
    public async Task<string> DeleteUserByObjectIdAsync(string objectId)
    {
      return await SendGraphDeleteRequest(Constants.Endpoints.DeleteUser + objectId);
    }
    
    public async Task<string> GetGroupByObjectIdAsync(string objectId)
    {
      return await SendGraphGetRequest($"{Constants.Endpoints.AllGroups}/{objectId}", null);
    }

    public async Task<string> DeleteGroupByObjectIdAsync(string objectId)
    {
      return await SendGraphDeleteRequest($"{Constants.Endpoints.AllGroups}/{objectId}");
    }
    public async Task<string> AddGroupMemberAsync(string objectId, string memberId)
    {
      var json = $"{{\"url\":\"{GraphBaseUrl}{tenantId}{Constants.Endpoints.DirectoryObjects}{memberId}\"}}";
      return await SendGraphPostRequest($"{Constants.Endpoints.AllGroups}/{objectId}/$links/members", json);
    }
    public async Task<string> DeleteGroupMemberAsync(string objectId, string memberId)
    {
      return await SendGraphDeleteRequest($"{Constants.Endpoints.AllGroups}/{objectId}/$links/members/{memberId}");
    }
    private async Task<string> SendGraphGetRequest(string api, string query)
    {
      var request = await GetHttpRequestMessage(HttpMethod.Get, api, query);

      return await ProcessRequest(request);
    }
    private async Task<string> SendGraphPostRequest(string api, string json)
    {
      var request = await GetHttpRequestMessage(HttpMethod.Post, api);
      request.Content = new StringContent(json, Encoding.UTF8, "application/json");

      return await ProcessRequest(request);
    }
    private async Task<string> SendGraphPatchRequest(string api, string json)
    {
      var request = await GetHttpRequestMessage(new HttpMethod("PATCH"), api);
      request.Content = new StringContent(json, Encoding.UTF8, "application/json");

      return await ProcessRequest(request);
    }
    private async Task<string> SendGraphDeleteRequest(string api)
    {
      var request = await GetHttpRequestMessage(HttpMethod.Delete, api);      
      return await ProcessRequest(request);
    }
    private async Task<string> ProcessRequest(HttpRequestMessage request)
    {
      var response = await _httpClient.SendAsync(request);

      if (response.IsSuccessStatusCode)
        return await response.Content.ReadAsStringAsync();

      //add logging
      var error = await response.Content.ReadAsStringAsync();
      var formatted = JsonConvert.DeserializeObject(error);
      throw new HttpClientException(response.StatusCode,
        JsonConvert.SerializeObject(formatted, Formatting.Indented));
    }
    private async Task<HttpRequestMessage> GetHttpRequestMessage(HttpMethod httpMethod, string api, string query = null)
    {
      var token = await _authContext.AcquireTokenAsync(GraphBaseUrl, _credential);
      var url = $"{GraphBaseUrl}{tenantId}{api}?{GraphApiVersion}";

      if (!string.IsNullOrEmpty(query))
      {
        url += $"&{query}";
      }

      var request = new HttpRequestMessage(httpMethod, url);
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
      return request;
    }
    public void Dispose()
    {
      _httpClient?.Dispose();
    }
  }
}
