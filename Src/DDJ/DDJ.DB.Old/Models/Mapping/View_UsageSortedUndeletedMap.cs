using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class View_UsageSortedUndeletedMap : EntityTypeConfiguration<View_UsageSortedUndeleted>
    {
        public View_UsageSortedUndeletedMap()
        {
            // Primary Key
            this.HasKey(t => new { t.File8bSignature, t.nGenre, t.LastTimePlayed });

            // Properties
            this.Property(t => t.File8bSignature)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.PathFileName)
                .HasMaxLength(512);

            this.Property(t => t.nGenre)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("View_UsageSortedUndeleted");
            this.Property(t => t.File8bSignature).HasColumnName("File8bSignature");
            this.Property(t => t.PathFileName).HasColumnName("PathFileName");
            this.Property(t => t.nGenre).HasColumnName("nGenre");
            this.Property(t => t.TimesPlayed).HasColumnName("TimesPlayed");
            this.Property(t => t.LastTimePlayed).HasColumnName("LastTimePlayed");
            this.Property(t => t.Liked).HasColumnName("Liked");
            this.Property(t => t.Hated).HasColumnName("Hated");
        }
    }
}
