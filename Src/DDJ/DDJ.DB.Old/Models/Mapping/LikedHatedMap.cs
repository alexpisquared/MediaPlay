using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class LikedHatedMap : EntityTypeConfiguration<LikedHated>
    {
        public LikedHatedMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.EvaluatedBy)
                .HasMaxLength(512);

            // Table & Column Mappings
            this.ToTable("LikedHated");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.File8bSignature).HasColumnName("File8bSignature");
            this.Property(t => t.IsGood).HasColumnName("IsGood");
            this.Property(t => t.EvaluatedAt).HasColumnName("EvaluatedAt");
            this.Property(t => t.EvaluatedBy).HasColumnName("EvaluatedBy");
            this.Property(t => t.LikedAt).HasColumnName("LikedAt");
            this.Property(t => t.HatedAt).HasColumnName("HatedAt");
            this.Property(t => t.LastEditDate).HasColumnName("LastEditDate");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");

            // Relationships
            this.HasRequired(t => t.AudioPiece)
                .WithMany(t => t.LikedHateds)
                .HasForeignKey(d => d.File8bSignature);

        }
    }
}
