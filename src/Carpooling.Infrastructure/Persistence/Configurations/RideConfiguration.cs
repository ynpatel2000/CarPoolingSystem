using Carpooling.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carpooling.Infrastructure.Persistence.Configurations;

public class RideConfiguration : IEntityTypeConfiguration<Ride>
{
    public void Configure(EntityTypeBuilder<Ride> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.FromCity, x.ToCity, x.RideDate });

        builder.Property(x => x.FromCity).HasMaxLength(100);
        builder.Property(x => x.ToCity).HasMaxLength(100);
    }
}
