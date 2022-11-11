﻿using X.PagedList;
using System.Collections.Generic;
using Webx.Web.Data.Entities;
using System.Linq;

namespace Webx.Web.Models
{
    public class ShopViewModel
    {
        public IPagedList<Product> PagedListProduct { get; set; }

        public string SelectedCategory { get; set; }

        public List<string> BrandsTags { get; set; }

        public int NumberOfProductsFound { get; set; }             

        public int ResultsPerPage { get; set; }

        public List<Category> Categories { get; set; }

        public List<Brand> Brands { get; set; }

        public decimal MostExpensiveProductPrice { get; set; }
        
        public Product Product { get; set; }

        public List<CartViewModel> Cart { get; set; }

        public int TotalProductsInCart => Cart.Sum(p => p.Quantity);

        public string CartGrandTotal => Cart.Sum(p => p.Product.Price * p.Quantity).ToString("C2");       

        public bool CookieConsent { get; set; }

    }
}