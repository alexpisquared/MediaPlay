using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Models.Mapping
{
    public class MuAuditionMap : EntityTypeConfiguration<MuAudition>
    {
        public MuAuditionMap()
        {
            // Primary Key
            HasKey(t => t.ID);

            // Properties
            Property(t => t.DoneBy)
                .IsRequired()
                .HasMaxLength(64);

            // Table & Column Mappings
            ToTable("MuAudition");
            Property(t => t.ID).HasColumnName("ID");
            Property(t => t.MediaUnitID).HasColumnName("MediaUnitID");
            Property(t => t.PartyMode).HasColumnName("PartyMode");
            Property(t => t.DoneAt).HasColumnName("DoneAt");
            Property(t => t.DoneBy).HasColumnName("DoneBy");

            // Relationships
            HasRequired(t => t.MediaUnit)
                .WithMany(t => t.MuAuditions)
                .HasForeignKey(d => d.MediaUnitID);

        }
    }
}
