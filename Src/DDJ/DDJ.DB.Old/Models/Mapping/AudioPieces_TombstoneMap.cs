using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class AudioPieces_TombstoneMap : EntityTypeConfiguration<AudioPieces_Tombstone>
    {
        public AudioPieces_TombstoneMap()
        {
            // Primary Key
            this.HasKey(t => t.File8bSignature);

            // Properties
            this.Property(t => t.File8bSignature)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("AudioPieces_Tombstone");
            this.Property(t => t.File8bSignature).HasColumnName("File8bSignature");
            this.Property(t => t.DeletionDate).HasColumnName("DeletionDate");
        }
    }
}
