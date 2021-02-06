using System;

namespace VPC.Models
{
	public class FilePath
	{
		public int ID { get; set; }
		public string Path { get; set; }
	}
	public class Machine
	{
		public int ID { get; set; }
		public string MachineName { get; set; }
	}
	public class MediaUnit_FilePath
	{
		public int ID { get; set; }
		public int MediaUnitID { get; set; }
		public int MachineID { get; set; }
		public int FilePathID { get; set; }
	}
	public class MuAudition
	{
		public int ID { get; set; }
		//public CrntMU CrntMU { get; set; }
		public bool PartyMode { get; set; }
		public DateTime DoneAt { get; set; }
		public string DoneBy { get; set; }
	}
	//public class MuBookmark
	//{
	//	public int ID { get; set; }
	//	public object CrntMU { get; set; }
	//	public double PositionSec { get; set; }
	//	public string Note { get; set; }
	//	public DateTime AddedAt { get; set; }
	//	public string AddedBy { get; set; }
	//}
	public class MuRateHist
	{
		public int ID { get; set; }
		public MediaUnit MediaUnit { get; set; }
		public int Like { get; set; }
		public DateTime AddedAt { get; set; }
		public string AddedBy { get; set; }
	}
	public class LkuGenre       /**/ { public int ID { get; set; } public string Name { get; set; } public string Desc { get; set; } }

	//public class DdjEf4DB : DbContext
	//{
	//	public DbSet<LkuGenre>            /**/  Genres { get; set; }

	//	public DbSet<CrntMU> MediaUnits { get; set; }
	//	public DbSet<MuAudition> MuAuditions { get; set; }
	//	public DbSet<MuRateHist> MuRateHists { get; set; }
	//	public DbSet<MuBookmark> MuBookmarks { get; set; }

	//	void foo()		{		}

	//	//needs redoing DS: 
	//	protected override void OnModelCreating(DbModelBuilder modelBuilder)
	//	{
	//		modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();
	//	}

	//}
}
