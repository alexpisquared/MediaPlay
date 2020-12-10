using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace EF7UwpPoc.Model
{
    public class DadosContext : DbContext
    {
        string dbfile = "";

        public DbSet<Pessoa> Pessoa { get; set; }
        public string DbPath => dbfile;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder); //??

            dbfile = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "Db.SQLite");

            optionsBuilder.UseSqlite($"Data source={dbfile}");

            Debug.WriteLine(dbfile);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //??

            modelBuilder.Entity<Pessoa>().Property(b => b.Name).IsRequired(); // make Blog.URL required
        }
    }
}