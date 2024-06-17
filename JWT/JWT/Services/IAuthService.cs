using JWT.DTO;

namespace JWT.Services;

public interface IAuthService
{
    public Task<int> Register(RegisterRequestDto registerRequestDto, CancellationToken cancellationToken);

    public Task<TokensReponseDto> Login(LoginRequestDto loginRequestDto, CancellationToken cancellationToken);

    public Task<TokensReponseDto> RefreshAppUserToken(RefreshTokenRequestDto tokenRequestRequest,
        CancellationToken cancellationToken);
}