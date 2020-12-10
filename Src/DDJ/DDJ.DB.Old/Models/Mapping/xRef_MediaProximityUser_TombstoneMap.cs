using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class xRef_MediaProximityUser_TombstoneMap : EntityTypeConfiguration<xRef_MediaProximityUser_Tombstone>
    {
        public xRef_MediaProximityUser_TombstoneMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("xRef_MediaProximityUser_Tombstone");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.DeletionDate).HasColumnName("DeletionDate");
        }
    }
}
