using APIPractice.Exceptions;
using APIPractice.Models.DTO;
using APIPractice.Repository.IRepository;
using APIPractice.Services;
using APIPractice.Services.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Moq;
using System.Reflection.Metadata.Ecma335;

namespace Rapidd_UnitTesting;

public class RegisterCustomer_UnitTesting
{
    private Mock<UserManager<IdentityUser>> GetMockUserManager()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        return new Mock<UserManager<IdentityUser>>(
            store.Object, null, null, null, null, null, null, null, null
        );
    }
    // Simulate role assignment failure
    private readonly string[] validRoles = new[] { "Customer", "Manager", "Employee", "Admin" };


    //[Test]
    //[TestCase("Sreeram", "1234", "1234567890", "Wrong Role","sreeram@rapidd.com")]
    //public void RegisterCustomer_InvalidCredentials(string name, string password, string phone, string role, string username)
    //{
    //    // Setup
    //    var registerCustomerRequest = new RegisterCustomerRequest { Name = name, Password = password, Phone = phone, Role = role, UserName = username };

    //    Assert.ThrowsAsync<Exception>(async () => await _authService.RegisterCustomer(registerCustomerRequest));
    //}
    [Test]
    public void RegisterCustomer_InvalidRole_ThrowsNotFoundException()
    {
        // Arrange
        var mockUserManager = GetMockUserManager();
        var mockTokenRepo = new Mock<ITokenRepository>();
        var mockUserRepo = new Mock<IRegisterUserRepository>();

        // Simulate successful user creation
        mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        

        mockUserManager
            .Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .Returns<IdentityUser, string>((user, role) =>
            {
                // Simulate success only for valid roles
                if (validRoles.Contains(role))
                    return Task.FromResult(IdentityResult.Success);
                else
                    return Task.FromResult(IdentityResult.Failed());
            });


        var authService = new AuthService(
            mockUserManager.Object,
            mockTokenRepo.Object,
            mockUserRepo.Object
        );

        var request = new RegisterCustomerRequest
        {
            UserName = "test@demo.com",
            Password = "1234",
            Role = "InvalidRole",
            Name = "test",
            Phone = "1234567890"
        };

        // Act + Assert
        Assert.ThrowsAsync<NotFoundException>(async () => await authService.RegisterCustomer(request));
    }

    [Test]
    public void RegisterCustomer_DuplicateUserName_ThrowsConflictException()
    {
        Mock<ITokenRepository> mockTokenRepository = new Mock<ITokenRepository>();
        Mock<IRegisterUserRepository> mockUserRepository= new Mock<IRegisterUserRepository>();
        Mock<UserManager<IdentityUser>> mockUserManager = GetMockUserManager();
        

        mockUserManager.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).Returns<IdentityUser,string>((identityUser,password) =>
        {
            if(identityUser.UserName == "test@demo.com")
            {
                return Task.FromResult(IdentityResult.Failed());
            }
            else
            {
                return Task.FromResult(IdentityResult.Success);
            }
        });

        mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).Returns<IdentityUser, string>((identityUser, role) =>
        {
            if (validRoles.Contains(role))
            {
                return Task.FromResult(IdentityResult.Failed());
            }
            else
            {
                return Task.FromResult(IdentityResult.Success);
            }
        });

        var _authService = new AuthService(mockUserManager.Object, mockTokenRepository.Object, mockUserRepository.Object);

        var request = new RegisterCustomerRequest
        {
            UserName = "test@demo.com",
            Password = "1234",
            Role = "InvalidRole",
            Name = "test",
            Phone = "1234567890"
        };

        Assert.ThrowsAsync<ConflictException>( async () => await _authService.RegisterCustomer(request));
    }

}
