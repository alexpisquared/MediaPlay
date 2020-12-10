using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class PrevTimes_TombstoneMap : EntityTypeConfiguration<PrevTimes_Tombstone>
    {
        public PrevTimes_TombstoneMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("PrevTimes_Tombstone");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.DeletionDate).HasColumnName("DeletionDate");
        }
    }
}
