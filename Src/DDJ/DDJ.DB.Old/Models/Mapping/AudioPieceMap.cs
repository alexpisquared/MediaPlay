using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DDJ.DB.Old.Models.Mapping
{
    public class AudioPieceMap : EntityTypeConfiguration<AudioPiece>
    {
        public AudioPieceMap()
        {
            // Primary Key
            this.HasKey(t => t.File8bSignature);

            // Properties
            this.Property(t => t.File8bSignature)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.PathFileName)
                .HasMaxLength(512);

            this.Property(t => t.tagTitle)
                .HasMaxLength(512);

            this.Property(t => t.tagArtist)
                .HasMaxLength(512);

            this.Property(t => t.tagAlbum)
                .HasMaxLength(512);

            this.Property(t => t.tagComment)
                .HasMaxLength(1024);

            this.Property(t => t.tagGenre)
                .HasMaxLength(128);

            this.Property(t => t.AddedBy)
                .HasMaxLength(512);

            this.Property(t => t.FilePath)
                .HasMaxLength(512);

            this.Property(t => t.FileName)
                .HasMaxLength(512);

            // Table & Column Mappings
            this.ToTable("AudioPieces");
            this.Property(t => t.File8bSignature).HasColumnName("File8bSignature");
            this.Property(t => t.PathFileName).HasColumnName("PathFileName");
            this.Property(t => t.FileLengh).HasColumnName("FileLengh");
            this.Property(t => t.Flags).HasColumnName("Flags");
            this.Property(t => t.nGenre).HasColumnName("nGenre");
            this.Property(t => t.Genre).HasColumnName("Genre");
            this.Property(t => t.tagTitle).HasColumnName("tagTitle");
            this.Property(t => t.tagArtist).HasColumnName("tagArtist");
            this.Property(t => t.tagAlbum).HasColumnName("tagAlbum");
            this.Property(t => t.tagComment).HasColumnName("tagComment");
            this.Property(t => t.tagGenre).HasColumnName("tagGenre");
            this.Property(t => t.AddedAt).HasColumnName("AddedAt");
            this.Property(t => t.AddedBy).HasColumnName("AddedBy");
            this.Property(t => t.Nootes).HasColumnName("Nootes");
            this.Property(t => t.LastEditDate).HasColumnName("LastEditDate");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.FilePath).HasColumnName("FilePath");
            this.Property(t => t.FileName).HasColumnName("FileName");

            // Relationships
            this.HasOptional(t => t.lkuGenre)
                .WithMany(t => t.AudioPieces)
                .HasForeignKey(d => d.Genre);

        }
    }
}
