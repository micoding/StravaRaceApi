using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StravaRaceAPI;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Services;

namespace StravaRaceAPITests.Services;

public class TestUserService
{
    private AuthenticationOptions _authOptions;
    private ApiDBContext _context;
    private DbContextOptions<ApiDBContext> _options;
    private UserService _serviceUnderTest;

    [SetUp]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<ApiDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApiDBContext(_options);
        _authOptions = new AuthenticationOptions
            { JwtIssuer = "https://this.api.com", JwtKey = "secret_jwt_key_do_not_share_or_else", JwtExpireDays = 1 };
        _serviceUnderTest = new UserService(_context, _authOptions);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private async Task<User> AddUser(int id = 1)
    {
        var user = PreapreObject.CreateUser(id);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    [Test]
    public async Task SignInWithStrava_NoUserInDb_CreateUser()
    {
        var userToCreate = PreapreObject.CreateUser(1);
        await _serviceUnderTest.SignInWithStrava(userToCreate);
        var createdUser = _context.Users.SingleOrDefault(u => u.Id == userToCreate.Id);
        Assert.That(createdUser, Is.Not.Null);
    }

    [Test]
    public async Task SignInWithStrava_UserInDb_RefreshUser()
    {
        var userToCreate = await AddUser();

        userToCreate.FirstName = "ChangedName";
        userToCreate.LastName = "ChangedLastName";
        userToCreate.Email = "ChangedEmail";

        await _serviceUnderTest.SignInWithStrava(userToCreate);
        var createdUser = _context.Users.SingleOrDefault(u => u.Id == userToCreate.Id);
        Assert.That(createdUser, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createdUser.FirstName, Is.EqualTo(userToCreate.FirstName));
            Assert.That(createdUser.LastName, Is.EqualTo(userToCreate.LastName));
            Assert.That(createdUser.Email, Is.EqualTo(userToCreate.Email));
        });
    }

    [Test]
    public async Task SignInWithStrava_ReturnsValidJwtToken()
    {
        var userToCreate = PreapreObject.CreateUser(1);
        var token = await _serviceUnderTest.SignInWithStrava(userToCreate);
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        Assert.DoesNotThrow(() => jwtTokenHandler.ValidateToken(token,
            new TokenValidationParameters
            {
                ValidIssuer = "https://this.api.com",
                ValidAudience = "https://this.api.com",
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret_jwt_key_do_not_share_or_else"))
            }, out var st));
    }

    [Test]
    public async Task Delete_UserExist_DeletesUser()
    {
        const int userId = 1;
        await AddUser();
        await _serviceUnderTest.Delete(userId);

        Assert.That(_context.Users.SingleOrDefault(u => u.Id == userId), Is.Null);
    }

    [Test]
    public async Task Delete_UserNotExist_Throws404NotFoundException()
    {
        const int userId = 1;
        const int idToDelete = 2;
        await AddUser();

        Assert.ThrowsAsync<NotFoundException>(() => _serviceUnderTest.Delete(idToDelete));
    }
}