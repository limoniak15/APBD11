
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JWT.Entites;

public class AppUserEfConfig : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder
            .HasKey(au => au.IdAppUser)
            .HasName("AppUser_pk");

        builder
            .Property(au => au.IdAppUser)
            .ValueGeneratedOnAdd();

        builder
            .Property(au => au.Login)
            .IsRequired();

        builder
            .Property(au => au.Password)
            .IsRequired();

        builder
            .Property(au => au.Salt)
            .IsRequired();

        builder
            .Property(au => au.RefreshToken)
            .IsRequired();

        builder
            .Property(au => au.RefreshTokenExp)
            .IsRequired();

        builder
            .ToTable("AppUser");
    }
}