using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class FileSysFiles_TombstoneMap : EntityTypeConfiguration<FileSysFiles_Tombstone>
    {
        public FileSysFiles_TombstoneMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("FileSysFiles_Tombstone");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.DeletionDate).HasColumnName("DeletionDate");
        }
    }
}
