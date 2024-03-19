﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoIntegradorIfood.ClassesAuxiliares;

public class PedidoCompleto
{
    public string? id { get; set; }
    public string? displayId { get; set; }
    public string? createdAt { get; set; }
    public string? orderTiming { get; set; }
    public string? orderType { get; set; }
    public Delivery delivery { get; set; } = new Delivery(); //tabela nova //não inserir no banco inicialmente
    public string? preparationStartDateTime { get; set; }
    public bool isTest { get; set; }
    public string salesChannel { get; set; }
    public Merchant merchant { get; set; } = new Merchant(); //tabela nova //não inserir no banco inicialmente
    public Customer customer { get; set; } = new Customer(); //tabela nova //não inserir no banco inicialmente
    public List<Items> items { get; set; } = new List<Items>(); // tabela nova  //não inserir no banco inicialmente
    public Total total { get; set; } = new Total();  // tabela nova //não inserir no banco inicialmente
    public Payments payments { get; set; } = new Payments(); // tabela nova //não inserir no banco inicialmente
    public AdditionalInfo additionalInfo { get; set; } = new AdditionalInfo(); // tabela nova //não inserir no banco inicialmente

    public PedidoCompleto(){}
}

public class pedidocompleto //Classe para inserir na tabela pedido completo no banco de dados, está dando erro caso tentarmos fazer com a classe PedidoCompleto 
{
    public string? id { get; set; }
    [Column("displayid")]
    public string? displayId { get; set; }
    [Column("createdat")]
    public string? createdAt { get; set; }
    [Column("ordertiming")]
    public string? orderTiming { get; set; }
    [Column("ordertype")]
    public string? orderType { get; set; }
    [Column("preparationstartdatetime")]
    public string? preparationStartDateTime { get; set; }
    [Column("istest")]
    public bool isTest { get; set; }
    [Column("saleschannel")]
    public string salesChannel { get; set; }

    public pedidocompleto() { }
}

//classe iniciando com letra minúscula para conseguirmos desserializar o json que vem 

public class Delivery
{
    public int  id { get; set; }
    public string? id_pedido {  get; set; }
    public string? mode { get; set; }
    [Column("deliveredby")]
    public string? deliveredBy { get; set; }
    [Column("deliverydatetime")]
    public string? deliveryDateTime { get; set; }
    public string? observations { get; set; }
    [NotMapped]
    public DeliveryAddress deliveryAddress { get; set; } = new DeliveryAddress();
    [Column("pickupcode")]
    public string pickupCode { get; set; }  

    public Delivery(){}

}

[Table("deliveryaddress")]
public class DeliveryAddress
{
    public int id { get; set; } 
    public int id_delivery {  get; set; }
    public string? id_pedido { get; set; }
    [Column("streetname")]
    public string? streetName { get; set; }
    [Column("streetnumber")]

    public string? streetNumber { get; set; }
    [Column("formattedaddress")]

    public string? formattedAddress { get; set; }
    public string? neighborhood { get; set; }
    public string? complement { get; set; }
    [Column("postalcode")]

    public string? postalCode { get; set; }
    public string? city { get; set; }
    public string? reference { get; set; }
    [NotMapped]
    public Coordinates coordinates { get; set; } = new Coordinates();

    public DeliveryAddress(){}
}

[Table("coordinates")]
public class Coordinates {

    public int id { get; set; }
    [Column("id_deliveryaddress")]
    public int id_DeliveryAddress { get; set; }
    public string? id_pedido {  get; set; }
    public float latitude { get; set; }
    public float longitude { get; set; }

    public Coordinates(){}
}

[Table("merchant")]
public class Merchant
{
    public string id_pedido { get; set; }
    public string? id { get; set; }
    public string? name { get; set; }

    public Merchant(){}
}

[Table("customer")]
public class Customer
{
    [Column("id")]
    public int id_customer { get; set; }
    [Column("id_customer")]
    public string? id { get; set; }  
    public string? id_pedido { get; set; }  
    public string? name { get; set; }
    [Column("documentnumber")]
    public string? documentNumber { get; set; }
    [NotMapped]
    public Phone? phone { get; set; }
    public string? segmentation { get; set; }
    

    public Customer() { }

}

[Table("phone")]
public class Phone
{
    public int id { get; set; }
    public int id_customer{ get; set; }
    public string? id_pedido { get; set; }
    public string? number { get; set; }
    public string? localizer { get; set; }
    [Column("localizerexpiration")]
    public string? localizerExpiration { get; set; }
    public Phone() { }
}

public class Items
{
    public int item_id { get; set; }
    public string? id_pedido { get; set; }
    public int index { get; set; }
    public string? id { get; set; }
    public string? uniqueId { get; set; }
    public string? name { get; set; }
    public int quantity { get; set; }
    public string? unit { get; set; }
    public float unitPrice { get; set; }
    public float optionsPrice { get; set; }
    public float totalPrice { get; set; }
    public float price { get; set; }
    
    public Items() { }
}

public class Total
{
    public int id { get; set; }
    public string? id_pedido { get; set; }
    public float additionalFees { get; set; }
    public float subTotal { get; set; }
    public float deliveryFee { get; set; }
    public int benefits { get; set; }
    public float orderAmount { get; set; }

    public Total(){}
}

public class Payments
{
    public int id { get; set; }
    public string? id_pedido { get; set; }
    public float prepaid { get; set; }
    public int pending { get; set; }
    public List<Methods> methods { get; set; } = new List<Methods>();
}

public class Methods
{
    public int id { get; set; }
    public int payments_id { get; set; }
    public string? id_pedido { get; set; }
    public float value { get; set; }
    public string? currency { get; set; }
    public string? method { get; set; }
    public bool prepaid { get; set; }
    public string type { get; set; }
    public Card card { get; set; } = new Card();

    public Methods() { }
}

public class Card
{
    public int id { get; set; }
    public string methods_id { get; set; }
    public string? id_pedido { get; set; }
    public string brand { get; set; }

    public Card() { }
}

public class AdditionalInfo
{
    public int id { get; set; }
    public string? id_pedido { get; set; }
    public metadata metadata { get; set; } = new metadata();    
}

public class metadata
{
    public int id { get; set; }
    public int id_additionalInfo { get; set; }
    public string? id_pedido { get; set; }
    public string? developerId { get; set; } 
   public string? customerEmail { get; set; } 
   public string? developerEmail { get; set; }

    public metadata() { }
}