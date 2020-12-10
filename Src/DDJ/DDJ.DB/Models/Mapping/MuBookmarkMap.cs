using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Models.Mapping
{
    public class MuBookmarkMap : EntityTypeConfiguration<MuBookmark>
    {
        public MuBookmarkMap()
        {
            // Primary Key
            HasKey(t => t.ID);

            // Properties
            Property(t => t.Note)
                .HasMaxLength(1024);

            Property(t => t.DoneBy)
                .IsRequired()
                .HasMaxLength(64);

            // Table & Column Mappings
            ToTable("MuBookmark");
            Property(t => t.ID).HasColumnName("ID");
            Property(t => t.MediaUnitID).HasColumnName("MediaUnitID");
            Property(t => t.PositionSec).HasColumnName("PositionSec");
            Property(t => t.Note).HasColumnName("Note");
            Property(t => t.DoneAt).HasColumnName("DoneAt");
            Property(t => t.DoneBy).HasColumnName("DoneBy");

            // Relationships
            HasRequired(t => t.MediaUnit)
                .WithMany(t => t.MuBookmarks)
                .HasForeignKey(d => d.MediaUnitID);

        }
    }
}
