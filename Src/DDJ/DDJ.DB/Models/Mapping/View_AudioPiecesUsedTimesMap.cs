using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Models.Mapping
{
    public class View_AudioPiecesUsedTimesMap : EntityTypeConfiguration<View_AudioPiecesUsedTimes>
    {
        public View_AudioPiecesUsedTimesMap()
        {
            // Primary Key
            HasKey(t => new { t.MediaUnitID, t.PathFileName, t.GenreID, t.FileName });

            // Properties
            Property(t => t.MediaUnitID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(t => t.PathFileName)
                .IsRequired()
                .HasMaxLength(481);

            Property(t => t.GenreID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(t => t.FileName)
                .IsRequired()
                .HasMaxLength(240);

            // Table & Column Mappings
            ToTable("View_AudioPiecesUsedTimes");
            Property(t => t.MediaUnitID).HasColumnName("MediaUnitID");
            Property(t => t.PathFileName).HasColumnName("PathFileName");
            Property(t => t.GenreID).HasColumnName("GenreID");
            Property(t => t.FileName).HasColumnName("FileName");
            Property(t => t.UsedTimesCount).HasColumnName("UsedTimesCount");
        }
    }
}
