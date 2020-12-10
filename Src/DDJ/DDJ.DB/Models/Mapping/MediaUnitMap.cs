using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Models.Mapping
{
    public class MediaUnitMap : EntityTypeConfiguration<MediaUnit>
    {
        public MediaUnitMap()
        {
            // Primary Key
            HasKey(t => t.ID);

            // Properties
            Property(t => t.PathFileExtOrg)
                .IsRequired()
                .HasMaxLength(256);

            Property(t => t.PathName)
                .IsRequired()
                .HasMaxLength(240);

            Property(t => t.FileName)
                .IsRequired()
                .HasMaxLength(240);

            Property(t => t.Notes)
                .HasMaxLength(2048);

            // Table & Column Mappings
            ToTable("MediaUnit");
            Property(t => t.ID).HasColumnName("ID");
            Property(t => t.GenreID).HasColumnName("GenreID");
            Property(t => t.PathFileExtOrg).HasColumnName("PathFileExtOrg");
            Property(t => t.PathName).HasColumnName("PathName");
            Property(t => t.FileName).HasColumnName("FileName");
            Property(t => t.FileHashMD5).HasColumnName("FileHashMD5");
            Property(t => t.FileHashQck).HasColumnName("FileHashQck");
            Property(t => t.FileLength).HasColumnName("FileLength");
            Property(t => t.DurationSec).HasColumnName("DurationSec");
            Property(t => t.CurPositionSec).HasColumnName("CurPositionSec");
            Property(t => t.Notes).HasColumnName("Notes");
            Property(t => t.AddedAt).HasColumnName("AddedAt");
            Property(t => t.DeletedAt).HasColumnName("DeletedAt");

            // Relationships
            HasRequired(t => t.LkuGenre)
                .WithMany(t => t.MediaUnits)
                .HasForeignKey(d => d.GenreID);

        }
    }
}
