using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

/// <summary>
/// EF Core configuration for SaleItem entity
/// </summary>
public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    /// <summary>
    /// Configures the SaleItem entity for EF Core
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(si => si.Id);
        builder.Property(si => si.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(si => si.SaleId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(si => si.ProductId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(si => si.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(si => si.Quantity)
            .IsRequired();

        builder.Property(si => si.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(si => si.DiscountPercentage)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(si => si.IsCancelled)
            .IsRequired()
            .HasDefaultValue(false);

        // Ignore calculated property
        builder.Ignore(si => si.TotalAmount);

        // Ignore navigation property to avoid circular reference issues
        builder.Ignore(si => si.Sale);
    }
}