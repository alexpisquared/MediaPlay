using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using DDJ.DB.Old.Models.Mapping;

namespace DDJ.DB.Old.Models
{
    public partial class DDJContext : DbContext
    {
        static DDJContext()
        {
            Database.SetInitializer<DDJContext>(null);
        }

        public DDJContext()
            : base("Name=DDJContext")
        {
        }

        public DbSet<AudioPiece> AudioPieces { get; set; }
        public DbSet<AudioPieces_Tombstone> AudioPieces_Tombstone { get; set; }
        public DbSet<dtproperty> dtproperties { get; set; }
        public DbSet<FileSysFile> FileSysFiles { get; set; }
        public DbSet<FileSysFiles_Tombstone> FileSysFiles_Tombstone { get; set; }
        public DbSet<LikedHated> LikedHateds { get; set; }
        public DbSet<LikedHated_Tombstone> LikedHated_Tombstone { get; set; }
        public DbSet<lkuGenre> lkuGenres { get; set; }
        public DbSet<lkuProximityState> lkuProximityStates { get; set; }
        public DbSet<PrevTime> PrevTimes { get; set; }
        public DbSet<PrevTimes_Tombstone> PrevTimes_Tombstone { get; set; }
        public DbSet<sysdiagram> sysdiagrams { get; set; }
        public DbSet<xRef_MediaProximityUser> xRef_MediaProximityUser { get; set; }
        public DbSet<xRef_MediaProximityUser_Tombstone> xRef_MediaProximityUser_Tombstone { get; set; }
        public DbSet<View_AudioPiecesUsedTimes> View_AudioPiecesUsedTimes { get; set; }
        public DbSet<View_Last33Listened> View_Last33Listened { get; set; }
        public DbSet<View_UsageSortedUndeleted> View_UsageSortedUndeleted { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new AudioPieceMap());
            modelBuilder.Configurations.Add(new AudioPieces_TombstoneMap());
            modelBuilder.Configurations.Add(new dtpropertyMap());
            modelBuilder.Configurations.Add(new FileSysFileMap());
            modelBuilder.Configurations.Add(new FileSysFiles_TombstoneMap());
            modelBuilder.Configurations.Add(new LikedHatedMap());
            modelBuilder.Configurations.Add(new LikedHated_TombstoneMap());
            modelBuilder.Configurations.Add(new lkuGenreMap());
            modelBuilder.Configurations.Add(new lkuProximityStateMap());
            modelBuilder.Configurations.Add(new PrevTimeMap());
            modelBuilder.Configurations.Add(new PrevTimes_TombstoneMap());
            modelBuilder.Configurations.Add(new sysdiagramMap());
            modelBuilder.Configurations.Add(new xRef_MediaProximityUserMap());
            modelBuilder.Configurations.Add(new xRef_MediaProximityUser_TombstoneMap());
            modelBuilder.Configurations.Add(new View_AudioPiecesUsedTimesMap());
            modelBuilder.Configurations.Add(new View_Last33ListenedMap());
            modelBuilder.Configurations.Add(new View_UsageSortedUndeletedMap());
        }
    }
}
