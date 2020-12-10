using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class PrevTimeMap : EntityTypeConfiguration<PrevTime>
    {
        public PrevTimeMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.UsedBy)
                .HasMaxLength(512);

            // Table & Column Mappings
            this.ToTable("PrevTimes");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.File8bSignature).HasColumnName("File8bSignature");
            this.Property(t => t.DateTimePlayed).HasColumnName("DateTimePlayed");
            this.Property(t => t.PartyMode).HasColumnName("PartyMode");
            this.Property(t => t.UsedBy).HasColumnName("UsedBy");
            this.Property(t => t.LastEditDate).HasColumnName("LastEditDate");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");

            // Relationships
            this.HasRequired(t => t.AudioPiece)
                .WithMany(t => t.PrevTimes)
                .HasForeignKey(d => d.File8bSignature);

        }
    }
}
