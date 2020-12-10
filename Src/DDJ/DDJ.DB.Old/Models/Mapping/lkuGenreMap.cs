using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class lkuGenreMap : EntityTypeConfiguration<lkuGenre>
    {
        public lkuGenreMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Note)
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("lkuGenre");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Note).HasColumnName("Note");
        }
    }
}
