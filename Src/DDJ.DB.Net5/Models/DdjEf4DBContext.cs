using System.Data.Entity;
using DDJ.DB.Models.Mapping;

namespace DDJ.DB.Models
{
    public partial class DdjEf4DBContext : DbContext
    {
        static DdjEf4DBContext()
        {
            Database.SetInitializer<DdjEf4DBContext>(null);
        }

        public DdjEf4DBContext()
            : base(@"Data Source=.\sqlexpress;Initial Catalog=DdjEf4DB;Integrated Security=True;MultipleActiveResultSets=True")
        {
        }

        public DbSet<LkuGenre> LkuGenres { get; set; }
        public DbSet<MediaUnit> MediaUnits { get; set; }
        public DbSet<MuAudition> MuAuditions { get; set; }
        public DbSet<MuBookmark> MuBookmarks { get; set; }
        public DbSet<MuRateHist> MuRateHists { get; set; }
        public DbSet<sysdiagram> sysdiagrams { get; set; }
        public DbSet<View_AudioPiecesUsedTimes> View_AudioPiecesUsedTimes { get; set; }
        public DbSet<vwDuplicate> vwDuplicates { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new LkuGenreMap());
            modelBuilder.Configurations.Add(new MediaUnitMap());
            modelBuilder.Configurations.Add(new MuAuditionMap());
            modelBuilder.Configurations.Add(new MuBookmarkMap());
            modelBuilder.Configurations.Add(new MuRateHistMap());
            modelBuilder.Configurations.Add(new sysdiagramMap());
            modelBuilder.Configurations.Add(new View_AudioPiecesUsedTimesMap());
            modelBuilder.Configurations.Add(new vwDuplicateMap());
        }
    }
}
