namespace JWT.Entites;

public class AppUser
{
    public int IdAppUser { get; set; }

    public string Login { get; set; }

    public string Password { get; set; }

    public string Salt { get; set; }

    public string RefreshToken { get; set; }

    public DateTime? RefreshTokenExp { get; set; }
}