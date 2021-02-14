using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Models.Mapping
{
    public class MuRateHistMap : EntityTypeConfiguration<MuRateHist>
    {
        public MuRateHistMap()
        {
            // Primary Key
            HasKey(t => t.ID);

            // Properties
            Property(t => t.DoneBy)
                .IsRequired()
                .HasMaxLength(64);

            // Table & Column Mappings
            ToTable("MuRateHist");
            Property(t => t.ID).HasColumnName("ID");
            Property(t => t.MediaUnitID).HasColumnName("MediaUnitID");
            Property(t => t.LikeUnit).HasColumnName("LikeUnit");
            Property(t => t.DoneAt).HasColumnName("DoneAt");
            Property(t => t.DoneBy).HasColumnName("DoneBy");

            // Relationships
            HasRequired(t => t.MediaUnit)
                .WithMany(t => t.MuRateHists)
                .HasForeignKey(d => d.MediaUnitID);

        }
    }
}
