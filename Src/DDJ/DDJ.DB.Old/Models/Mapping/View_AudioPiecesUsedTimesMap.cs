using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class View_AudioPiecesUsedTimesMap : EntityTypeConfiguration<View_AudioPiecesUsedTimes>
    {
        public View_AudioPiecesUsedTimesMap()
        {
            // Primary Key
            this.HasKey(t => new { t.File8bSignature, t.nGenre });

            // Properties
            this.Property(t => t.File8bSignature)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.PathFileName)
                .HasMaxLength(512);

            this.Property(t => t.nGenre)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.FilePath)
                .HasMaxLength(512);

            this.Property(t => t.FileName)
                .HasMaxLength(512);

            // Table & Column Mappings
            this.ToTable("View_AudioPiecesUsedTimes");
            this.Property(t => t.File8bSignature).HasColumnName("File8bSignature");
            this.Property(t => t.PathFileName).HasColumnName("PathFileName");
            this.Property(t => t.nGenre).HasColumnName("nGenre");
            this.Property(t => t.Flags).HasColumnName("Flags");
            this.Property(t => t.UsedTimes).HasColumnName("UsedTimes");
            this.Property(t => t.FilePath).HasColumnName("FilePath");
            this.Property(t => t.FileName).HasColumnName("FileName");
        }
    }
}
