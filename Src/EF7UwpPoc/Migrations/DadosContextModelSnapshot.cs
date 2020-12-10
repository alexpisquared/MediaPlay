using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using EF7UwpPoc.Model;

namespace EF7UwpPoc.Migrations
{
    [DbContext(typeof(DadosContext))]
    partial class DadosContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("EF7UwpPoc.Model.Pessoa", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Dscr");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");
                });
        }
    }
}
