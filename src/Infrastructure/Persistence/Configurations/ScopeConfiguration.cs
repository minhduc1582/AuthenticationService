using Domain.AppRegistrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ScopeConfiguration : IEntityTypeConfiguration<Scope>
{
    public void Configure(EntityTypeBuilder<Scope> builder)
    {
        builder.ToTable("Scopes");

        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.Name).IsUnique();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(250);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.HasData(
            new Scope(Guid.Parse("a2a55c37-3fcb-4f28-9e57-52ab4c0c4e01"), "api.read", "Read access to API resources", true),
            new Scope(Guid.Parse("e74a4df1-aeb6-4b68-86ce-d1a99cd3ad36"), "api.write", "Write access to API resources"));
    }
}
