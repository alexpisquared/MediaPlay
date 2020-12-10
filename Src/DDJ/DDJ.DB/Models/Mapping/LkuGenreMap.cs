using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Models.Mapping
{
    public class LkuGenreMap : EntityTypeConfiguration<LkuGenre>
    {
        public LkuGenreMap()
        {
            // Primary Key
            HasKey(t => t.ID);

            // Properties
            Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(64);

            Property(t => t.Desc)
                .IsRequired()
                .HasMaxLength(512);

            // Table & Column Mappings
            ToTable("LkuGenre");
            Property(t => t.ID).HasColumnName("ID");
            Property(t => t.Name).HasColumnName("Name");
            Property(t => t.Desc).HasColumnName("Desc");
        }
    }
}
