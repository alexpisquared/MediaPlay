using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class View_Last33ListenedMap : EntityTypeConfiguration<View_Last33Listened>
    {
        public View_Last33ListenedMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.UsedBy)
                .HasMaxLength(512);

            this.Property(t => t.PathFileName)
                .HasMaxLength(512);

            // Table & Column Mappings
            this.ToTable("View_Last33Listened");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.DateTimePlayed).HasColumnName("DateTimePlayed");
            this.Property(t => t.UsedBy).HasColumnName("UsedBy");
            this.Property(t => t.PathFileName).HasColumnName("PathFileName");
        }
    }
}
