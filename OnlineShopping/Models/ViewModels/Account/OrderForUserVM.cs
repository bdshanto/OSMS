using System;
using System.Collections.Generic;

namespace OnlineShopping.Models.ViewModels.Account
{
    public class OrderForUserVM
    {
        public int  OrderNumber { get; set; }
        public string UserName { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string,int> ProductAndQty { get; set; }
        public DateTime CratedAt  { get; set; }
    }
}