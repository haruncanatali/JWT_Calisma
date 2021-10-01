using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWT_Calisma.Model
{
    public class ProductMap : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(c => c.Id);
            builder.ToTable("Tbl_Product");
            builder.Property(c => c.Id).HasColumnName("Id");
            builder.Property(c => c.UrunAdi).HasColumnName("UrunAdi");
            builder.Property(c => c.UrunFiyati).HasColumnName("UrunFiyati");
        }
    }
}
