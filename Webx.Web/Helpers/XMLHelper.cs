using Syncfusion.EJ2.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Webx.Web.Data.Entities;

namespace Webx.Web.Helpers
{
    public class XMLHelper : IXMLHelper
    {
        public string GenerateXML(List<Stock> products, string user)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlElement studentDataNode = doc.CreateElement("OrderData");
            doc.AppendChild(studentDataNode);

            XmlNode headerNode = doc.CreateElement("Header");
            studentDataNode.AppendChild(headerNode);

            XmlNode companyNameNode = doc.CreateElement("CompanyName");
            companyNameNode.AppendChild(doc.CreateTextNode("WebX - Company"));
            headerNode.AppendChild(companyNameNode);

            XmlNode orderDateNode = doc.CreateElement("OrderDate");
            orderDateNode.AppendChild(doc.CreateTextNode($"{DateTime.Today.ToString("dd/MM/yyyy")}"));
            headerNode.AppendChild(orderDateNode);

            XmlNode shopAssistantNode = doc.CreateElement("ProductManager");
            shopAssistantNode.AppendChild(doc.CreateTextNode($"{user}"));
            headerNode.AppendChild(shopAssistantNode);

            XmlNode productsNode = doc.CreateElement("Products");
            doc.DocumentElement.AppendChild(productsNode);

            XmlElement StoreNode1 = doc.CreateElement("Store");
            if (products.Where(p => p.StoreId == 1).Any())
            {
                var temp = products.Where(p => p.StoreId == 1).FirstOrDefault();
                (StoreNode1).SetAttribute("Name", $"{temp.Store.Name}");
                (StoreNode1).SetAttribute("Address", $"{temp.Store.Address}");
                (StoreNode1).SetAttribute("Phone", $"{temp.Store.PhoneNumber}");
                (StoreNode1).SetAttribute("Email", $"{temp.Store.Email}");
                productsNode.AppendChild(StoreNode1);
            }

            XmlElement StoreNode2 = doc.CreateElement("Store");
            if (products.Where(p => p.StoreId == 2).Any())
            {
                var temp = products.Where(p => p.StoreId == 2).FirstOrDefault();
                (StoreNode2).SetAttribute("Name", $"{temp.Store.Name}");
                (StoreNode2).SetAttribute("Address", $"{temp.Store.Address}");
                (StoreNode2).SetAttribute("Phone", $"{temp.Store.PhoneNumber}");
                (StoreNode2).SetAttribute("Email", $"{temp.Store.Email}");
                productsNode.AppendChild(StoreNode2);
            }

            XmlElement StoreNode3 = doc.CreateElement("Store");
            if (products.Where(p => p.StoreId == 3).Any())
            {
                var temp = products.Where(p => p.StoreId == 3).FirstOrDefault();
                (StoreNode3).SetAttribute("Name", $"{temp.Store.Name}");
                (StoreNode3).SetAttribute("Address", $"{temp.Store.Address}");
                (StoreNode3).SetAttribute("Phone", $"{temp.Store.PhoneNumber}");
                (StoreNode3).SetAttribute("Email", $"{temp.Store.Email}");
                productsNode.AppendChild(StoreNode3);
            }

            foreach (var stock in products)
            {
                if (stock.Product.IsService)
                {
                    continue;
                }

                XmlNode productNode = doc.CreateElement("Product");
                if (stock.StoreId == 1)
                {
                    StoreNode1.AppendChild(productNode);
                }
                else if (stock.StoreId == 2)
                {
                    StoreNode2.AppendChild(productNode);
                }
                else if (stock.StoreId == 3)
                {
                    StoreNode3.AppendChild(productNode);
                }

                XmlNode productNameNode = doc.CreateElement("Name");
                productNameNode.AppendChild(doc.CreateTextNode($"{stock.Product.Name}"));
                productNode.AppendChild(productNameNode);

                XmlNode productBrandNode = doc.CreateElement("Brand");
                productBrandNode.AppendChild(doc.CreateTextNode($"{stock.Product.Brand.Name}"));
                productNode.AppendChild(productBrandNode);

                XmlNode productTypeNode = doc.CreateElement("Category");
                productTypeNode.AppendChild(doc.CreateTextNode($"{stock.Product.Category.Name}"));
                productNode.AppendChild(productTypeNode);

                XmlNode priceNode = doc.CreateElement("Price");
                priceNode.AppendChild(doc.CreateTextNode($"{stock.Product.Price}"));
                productNode.AppendChild(priceNode);

                XmlNode quantityNode = doc.CreateElement("Quantity");
                quantityNode.AppendChild(doc.CreateTextNode($"{stock.MinimumQuantity - stock.Quantity}"));
                productNode.AppendChild(quantityNode);
            }

            var savePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\stockAlert\");

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            var guid = Guid.NewGuid().ToString("N");
            var newFileName = string.Format("{0}{1}", guid, ".xml");
            doc.Save(savePath + newFileName);
            return newFileName;
        }
    }
}
