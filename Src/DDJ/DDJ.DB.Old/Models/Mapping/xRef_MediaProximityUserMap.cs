using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class xRef_MediaProximityUserMap : EntityTypeConfiguration<xRef_MediaProximityUser>
    {
        public xRef_MediaProximityUserMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.UserName)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("xRef_MediaProximityUser");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.File8bSignature).HasColumnName("File8bSignature");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.ProximityStateId).HasColumnName("ProximityStateId");
            this.Property(t => t.LastEditDate).HasColumnName("LastEditDate");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");

            // Relationships
            this.HasRequired(t => t.AudioPiece)
                .WithMany(t => t.xRef_MediaProximityUser)
                .HasForeignKey(d => d.File8bSignature);
            this.HasRequired(t => t.lkuProximityState)
                .WithMany(t => t.xRef_MediaProximityUser)
                .HasForeignKey(d => d.ProximityStateId);

        }
    }
}
