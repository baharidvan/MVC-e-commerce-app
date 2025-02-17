﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Abc.MvcWebUI.Entity
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public double Total { get; set; }
        public DateTime OrderDate { get; set; }
        public EnumOrderState OrderState { get; set; }


        //Bir sipariş verildiğinde siparişin adres bilgilerinin de Order tablosunda saklamak gerekiyor.
        public string Username { get; set; }
        public string AdresBasligi { get; set; }
        public string Adres { get; set; }
        public string Sehir { get; set; }
        public string Semt { get; set; }
        public string Mahalle { get; set; }
        public string PostaKodu { get; set; }

        public virtual List<OrderLine> Orderlines{ get; set; } //Lazy Loading için İlişkisel tüm verilerde Virtual keywordu kullanıldı.
    }

    public class OrderLine
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; } //Lazy Loading için İlişkisel tüm verilerde Virtual keywordu kullanıldı.

        public int Quantity { get; set; }

        public double Price { get; set; }

        //Her bir orderline da bir product bilgisi tutuyoruz
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } //Lazy Loading için İlişkisel tüm verilerde Virtual keywordu kullanıldı.
    }
}