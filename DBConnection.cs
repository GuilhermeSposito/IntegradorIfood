using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjetoIntegradorIfood.ClassesAuxiliares;
using  ProjetoIntegradorIfood.data;
namespace ProjetoIntegradorIfood;

public class ApplicationDbContext : DbContext
{
    public DbSet<Produto> produtos { get; set; }
    public DbSet<Pulling> pulling { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ifood;Username=postgres;Password=69063360");
        }
    }
}