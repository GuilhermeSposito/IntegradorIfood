using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoIntegradorIfood.ClassesAuxiliares;

internal class Pedido
{
    public string? id { get; set; }
    public string? code { get; set; }
    public string? fullCode { get; set;}
    public string? orderId { get; set;}
    public string? merchantId { get; set;}
    public string? createdAt { get; set;}

    public Metadata metadata { get; set; } = new Metadata();

    public Pedido()
    {
       
    }

}

public class Metadata
{
    public string? CLIENT_ID { get; set; }
    public string? ORIGIN { get; set; }
    public string? appName { get; set; }
    public string? ownerName { get; set; }

    public Metadata()
    {
        
    }

}