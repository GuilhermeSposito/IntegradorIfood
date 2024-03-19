﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProjetoIntegradorIfood.ClassesAuxiliares;
using  ProjetoIntegradorIfood.data;
namespace ProjetoIntegradorIfood;

public class ApplicationDbContext : DbContext
{
    public DbSet<Produto> produtos { get; set; }
    public DbSet<Pulling> pulling { get; set; }
    public DbSet<pedidocompleto> pedidocompleto { get; set; }
    public DbSet<Delivery> delivery { get; set; }
    public DbSet<DeliveryAddress> deliveryaddress { get; set; }
    public DbSet<Coordinates> coordinates { get; set; }
    public DbSet<Merchant> merchant { get; set; }
    public DbSet<Customer> customer { get; set; }
    public DbSet<Phone> phone { get; set; } 

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ifood;Username=postgres;Password=69063360");
        }
    }
}