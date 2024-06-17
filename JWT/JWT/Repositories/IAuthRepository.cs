using JWT.DTO;

namespace JWT.Repositories;

public interface IAuthRepository
{
    public Task<int> Register(RegisterRequestDto registerRequestDto, CancellationToken cancellationToken);

    public Task<TokensReponseDto> Login(LoginRequestDto loginRequestDto, CancellationToken cancellationToken);

    public Task<TokensReponseDto> RefreshAppUserToken(RefreshTokenRequestDto tokenRequestRequest,
        CancellationToken cancellationToken);
}