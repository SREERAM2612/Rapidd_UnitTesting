using APIPractice.Models.DTO;
using APIPractice.Repository.IRepository;
using APIPractice.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Moq;
using System.Security.Authentication;

namespace Rapidd_UnitTesting;

public class Login_UnitTesting
{
    private AuthService _authService;

    private Mock<UserManager<IdentityUser>> GetMockUserManager()
    {
        var userStore = new Mock<IUserStore<IdentityUser>>();
        return new Mock<UserManager<IdentityUser>>(userStore.Object, null, null, null, null, null, null, null, null);
    }

    [Test]
    public void LoginCustomer_InvalidPassword_ThrowsAuthenticationException()
    {
        Mock<IRegisterUserRepository> mockUserRepository = new Mock<IRegisterUserRepository>();
        Mock<ITokenRepository> tokenRepository = new Mock<ITokenRepository>();
        Mock<UserManager<IdentityUser>> mockUserManager = GetMockUserManager();

        mockUserManager
        .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
        .ReturnsAsync(new IdentityUser { UserName = "sreeram@rapidd.com" });


        mockUserManager.Setup(m => m.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).Returns<IdentityUser, string>((user, password) =>
        {
            if(password == "1234")
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        });

        _authService = new AuthService(mockUserManager.Object, tokenRepository.Object, mockUserRepository.Object);

        var request = new LoginRequest
        {
            UserName = "sreeram@rapidd.com",
            Password = "password"
        };

        Assert.ThrowsAsync<AuthenticationException>(() => _authService.LoginUser(request));

    }
}
