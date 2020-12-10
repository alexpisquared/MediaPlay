using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Models.Mapping
{
    public class vwDuplicateMap : EntityTypeConfiguration<vwDuplicate>
    {
        public vwDuplicateMap()
        {
            // Primary Key
            HasKey(t => t.ID);

            // Properties
            Property(t => t.ID)
                .IsRequired()
                .HasMaxLength(4000);

            Property(t => t.FileName)
                .HasMaxLength(4000);

            // Table & Column Mappings
            ToTable("vwDuplicate");
            Property(t => t.ID).HasColumnName("ID");
            Property(t => t.FileName).HasColumnName("FileName");
            Property(t => t.Qnt).HasColumnName("Qnt");
        }
    }
}
