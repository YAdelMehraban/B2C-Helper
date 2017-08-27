using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using B2C.GraphAPI.Helper.Models;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace B2C.GraphAPI.Helper.Tests
{
  public class B2CServiceTests
  {
    private IB2CClient b2cClient;
    private B2CService sut;

    public B2CServiceTests()
    {
      b2cClient = Substitute.For<IB2CClient>();
      SetupApplicationAndExtensionsJson();
      sut = B2CService.CreateInstance(b2cClient).Result;
     }

    [Fact]
    public async Task GetAllUserAsync_ShouldReturnTwoUser()
    {
      var jsonToReturn = GetJson("AllUsersFromAzureAd.json");
      sut.b2cClient.GetAllUsersAsync(Arg.Any<string>()).Returns(jsonToReturn);

      var responseMessage = await sut.GetAllUsersAysnc();

      responseMessage.Data.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetAllUserAsync_GivenAnInvalidResponse_ResponseMessageAsAppropiateStatusCode()
    {
      b2cClient.GetAllUsersAsync(Arg.Any<string>()).Throws(new HttpClientException(HttpStatusCode.BadRequest, "Invalid request"));

      var responseMessage = await sut.GetAllUsersAysnc();

      responseMessage.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task GetUserAsync_GivenAValidObjectId_ThenReturnsExpectedUser()
    {
      var jsonToReturn = GetJson("testUser.json");
      b2cClient.GetUserByObjectIdAsync(Arg.Any<string>()).Returns(jsonToReturn);
      var objectId = "f5109c45-f812-4854-899b-89f055c518d0";

      var result = await sut.GetUserAsync(objectId);

      result.DisplayName.Should().Be("User Test");
    }

    [Fact]
    public async Task GetUserAsync_GivenAValidObjectId_AndExtensionAttributeForCiDn_ThenReturnsCIDNValue()
    {
      SetupApplicationAndExtensionsJson();
      var jsonToReturn = GetJson("testUser.json");
      b2cClient.GetUserByObjectIdAsync(Arg.Any<string>()).Returns(jsonToReturn);
      var objectId = "f5109c45-f812-4854-899b-89f055c518d0";

      var result = await sut.GetUserAsync(objectId);

      result.CIDN.Should().Be("112122323233232");
    }

    [Fact]
    public async Task GetUserAsync_GivenAValidObjectId_AndExtensionAttributeForCompanyName_ThenReturnsCompanyValueValue()
    {
      SetupApplicationAndExtensionsJson();
      var jsonToReturn = GetJson("testUser.json");
      b2cClient.GetUserByObjectIdAsync(Arg.Any<string>()).Returns(jsonToReturn);
      var objectId = "f5109c45-f812-4854-899b-89f055c518d0";

      var result = await sut.GetUserAsync(objectId);

      result.CustomerName.Should().Be("Test Company");
    }

    [Fact]
    public async Task AddNewUserAsync_givenAValidB2CUser_ReturnsExpectedUser()
    {
      b2cClient.AddNewUserAsync(Arg.Any<string>())
        .ReturnsForAnyArgs(x => x.Arg<string>());

      var newUser = sut.GetBlankNewB2CUser();

      newUser.AccountEnabled = true;
      newUser.CIDN = "5555555";
      newUser.CustomerName = "Acme";
      newUser.DisplayName = "Ted Dancer";
      newUser.PasswordProfile =
        new PasswordProfile { ForceChangePasswordNextLogin = false, Password = "test password" };
      newUser.SignInNames = new List<SignInName> { new SignInName { Type = "Email", Value = "ted.dancer@acme.com" } };
      newUser.Mail = "ted.dance@acme.com";


      var result = await sut.AddNewUserAsync(newUser);

      result.Data.DisplayName.Should().Be(newUser.DisplayName);
      result.Data.CIDN.Should().Be(newUser.CIDN);
    }

    [Fact]
    public async Task GetApplicationObjectIdAsync_ReturnsExpectedObjectId()
    {
      var appJson = GetJson("applicationResponse.json");
      b2cClient.GetB2CApplicationAsync().Returns(appJson);
      var expectedObjectId = "506ee052-5f01-44ff-bf02-610584a5f6a0";

      var actualObjectId = await sut.GetApplicationObjectIdAsync();

      actualObjectId.Should().Be(expectedObjectId);
    }

    [Fact]
    public async Task GetExtensionPropertiesAsync_GivenAnObjectId_ReturnsTwoExtensionProperties()
    {
      var objectId = "506ee052-5f01-44ff-bf02-610584a5f6a0";
      var extensionJson = GetJson("extensionAttributesResponse.json");
      b2cClient.GetExtensionPropertiesAsync(Arg.Is(objectId)).Returns(extensionJson);

      var extensionProperties = await sut.GetExtensionPropertiesAsync(objectId);

      extensionProperties.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetNewUser_ShouldContainTwoExtensionProperties()
    {
      var appJson = GetJson("applicationResponse.json");
      b2cClient.GetB2CApplicationAsync().Returns(appJson);
      var extensionJson = GetJson("extensionAttributesResponse.json");
      b2cClient.GetExtensionPropertiesAsync(Arg.Any<string>()).Returns(extensionJson);

      var newUser = sut.GetBlankNewB2CUser();

      newUser.NonDeclaredProperties.Keys.Count.Should().Be(2);
    }


    private string GetJson(string filename)
    {
      
      var assembly = Assembly.GetExecutingAssembly();
      using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.SampleData.{filename}"))
      {

        var stream = new StreamReader(resource);
        return stream.ReadToEnd();
      }
    }

    private void SetupApplicationAndExtensionsJson()
    {
      var appJson = GetJson("applicationResponse.json");
      b2cClient.GetB2CApplicationAsync().Returns(appJson);
      var extensionJson = GetJson("extensionAttributesResponse.json");
      b2cClient.GetExtensionPropertiesAsync(Arg.Any<string>()).Returns(extensionJson);
    }
  }
}
