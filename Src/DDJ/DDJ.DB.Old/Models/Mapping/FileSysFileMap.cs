using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class FileSysFileMap : EntityTypeConfiguration<FileSysFile>
    {
        public FileSysFileMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.PathFileName)
                .IsRequired()
                .HasMaxLength(512);

            // Table & Column Mappings
            this.ToTable("FileSysFiles");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.PathFileName).HasColumnName("PathFileName");
            this.Property(t => t.AddetAt).HasColumnName("AddetAt");
            this.Property(t => t.Crc).HasColumnName("Crc");
            this.Property(t => t.LastEditDate).HasColumnName("LastEditDate");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
        }
    }
}
