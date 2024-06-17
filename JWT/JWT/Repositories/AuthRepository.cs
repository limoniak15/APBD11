using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JWT.DTO;
using JWT.Entites;
using JWT.Entities;
using JWT.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JWT.Repositories;

public class AuthRepository : IAuthRepository
{

    private readonly AppUserDbContext _context = new();
    
    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

    private IAuthRepository _authRepositoryImplementation;

    public AuthRepository(IAuthRepository authRepositoryImplementation)
    {
        _authRepositoryImplementation = authRepositoryImplementation;
    }

    public async Task<int> Register(RegisterRequestDto registerRequestDto, CancellationToken cancellationToken)
    {
        if (await DoesUserExistBasedOnLogin(registerRequestDto.Login, cancellationToken))
        {
            return -1;
        }

        var hashedPasswordAndSalt = SecurityHelper.GetHashedPasswordAndSalt(registerRequestDto.Password);

        var user = new AppUser
        {
            Login = registerRequestDto.Login,
            Password = hashedPasswordAndSalt.Item1,
            Salt = hashedPasswordAndSalt.Item2,
            RefreshToken = SecurityHelper.GenerateRefreshToken(),
            RefreshTokenExp = DateTime.Now.AddDays(1),
        };

        await _context
            .AppUsers
            .AddAsync(user, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return 1;
    }


    public async Task<TokensReponseDto> Login(LoginRequestDto loginRequestDto, CancellationToken cancellationToken)
    {
        if (await DoesUserExistBasedOnLogin(loginRequestDto.Login!, cancellationToken) == false)
        {
            return new TokensReponseDto() { StatusCode = -1 };
        }

        if (await IsTheProvidedPasswordRight(loginRequestDto, cancellationToken) == false)
        {
            return new TokensReponseDto() { StatusCode = -2 };
        }

        var user = await _context
            .AppUsers
            .Where(aa => aa.Login == loginRequestDto.Login)
            .FirstOrDefaultAsync(cancellationToken);

        var userClaim = new[]
        {
            new Claim(ClaimTypes.Name, loginRequestDto.Login!),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: userClaim,
            expires: DateTime.Now.AddMinutes(10),
            signingCredentials: credentials
        );

        user!.RefreshToken = SecurityHelper.GenerateRefreshToken();
        user.RefreshTokenExp = DateTime.Now.AddDays(1);

        await _context.SaveChangesAsync(cancellationToken);

        return new TokensReponseDto()
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = user.RefreshToken
        };
    }

    public async Task<TokensReponseDto> RefreshAppUserToken(RefreshTokenRequestDto tokenRequestRequest,
        CancellationToken cancellationToken)
    {
        if (await DoesUserExistBasedOnRefreshToken(tokenRequestRequest.RefreshToken, cancellationToken) == false)
        {
            return new TokensReponseDto()
            {
                StatusCode = -1
            };
        }

        if (await HasRefreshTokenExpired(tokenRequestRequest.RefreshToken, cancellationToken))
        {
            return new TokensReponseDto
            {
                StatusCode = -2
            };
        }

        var user = await _context
            .AppUsers
            .Where(aa => aa.RefreshToken == tokenRequestRequest.RefreshToken)
            .FirstOrDefaultAsync(cancellationToken);

        var userClaim = new[]
        {
            new Claim(ClaimTypes.Name, user!.Login),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: userClaim,
            expires: DateTime.Now.AddMinutes(10),
            signingCredentials: credentials
        );

        user.RefreshToken = SecurityHelper.GenerateRefreshToken();
        user.RefreshTokenExp = DateTime.Now.AddDays(1);

        await _context.SaveChangesAsync(cancellationToken);

        return new TokensReponseDto()
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = user.RefreshToken,
            StatusCode = 1
        };
    }


    private async Task<bool> DoesUserExistBasedOnLogin(string login, CancellationToken cancellationToken)
    {
        var res = await _context
            .AppUsers
            .Where(aa => aa.Login == login)
            .FirstOrDefaultAsync(cancellationToken);

        return res != null;
    }

    private async Task<bool> IsTheProvidedPasswordRight(LoginRequestDto loginRequestDto,
        CancellationToken cancellationToken)
    {
        var user = await _context
            .AppUsers
            .Where(aa => aa.Login == loginRequestDto.Login)
            .FirstOrDefaultAsync(cancellationToken);

        var passwordFromDatabase = user!.Password;
        var requestHashedPasswordWithSalt =
            SecurityHelper.GetHashedPasswordWithSalt(loginRequestDto.Password!, user.Salt);

        return passwordFromDatabase == requestHashedPasswordWithSalt;
    }


    private async Task<bool> DoesUserExistBasedOnRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        var res = await _context
            .AppUsers
            .Where(aa => aa.RefreshToken == refreshToken)
            .FirstOrDefaultAsync(cancellationToken);

        return res != null;
    }

    private async Task<bool> HasRefreshTokenExpired(string refreshToken, CancellationToken cancellationToken)
    {
        var res = await _context
            .AppUsers
            .Where(aa => aa.RefreshToken == refreshToken)
            .FirstOrDefaultAsync(cancellationToken);

        return res!.RefreshTokenExp < DateTime.Now;
    }
    
}