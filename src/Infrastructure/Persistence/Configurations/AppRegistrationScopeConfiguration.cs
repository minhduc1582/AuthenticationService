using Domain.AppRegistrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AppRegistrationScopeConfiguration : IEntityTypeConfiguration<AppRegistrationScope>
{
    public void Configure(EntityTypeBuilder<AppRegistrationScope> builder)
    {
        builder.ToTable("AppRegistrationScopes");
        builder.HasKey(s => new { s.AppRegistrationId, s.ScopeId });

        builder.Property(s => s.ScopeName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(s => s.Scope)
            .WithMany()
            .HasForeignKey(s => s.ScopeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
