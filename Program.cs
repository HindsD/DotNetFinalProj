using System;
using NLog.Web;
using System.IO;
using System.Linq;
using NorthWindConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NorthWindConsole
{
    class Program
    {

        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();        
        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Display Category and related products");
                    Console.WriteLine("4) Display all Categories and their related products");
                    Console.WriteLine("5) Add new product");
                    Console.WriteLine("6) Edit a product");
                    Console.WriteLine("7) Display products");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();

                    logger.Info($"Option {choice} selected");
                    if (choice == "1")
                    {
                        var db = new Northwind_88_DWHContext();
                        var query = db.Categories.OrderBy(p => p.CategoryName);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName} - {item.Description}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (choice == "2")
                    {
                        Category category = new Category();
                        Console.WriteLine("Enter Category Name:");
                        category.CategoryName = Console.ReadLine();
                        Console.WriteLine("Enter the Category Description:");
                        category.Description = Console.ReadLine();
                        ValidationContext context = new ValidationContext(category, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(category, context, results, true);
                        if (isValid)
                        {
                            var db = new Northwind_88_DWHContext();
                            // check for unique name
                            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                // TODO: save category to db
                                if (category != null)
                                {
                                    db.AddCategory(category);
                                    logger.Info("Category added - {name}", category.CategoryName);
                                }
                                
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "3")
                    {
                        var db = new Northwind_88_DWHContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category whose products you want to display:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");
                        foreach (Product p in category.Products)
                        {
                            Console.WriteLine(p.ProductName);
                        }
                    }
                    else if (choice == "4")
                    {
                        var db = new Northwind_88_DWHContext();
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName}");
                            foreach (Product p in item.Products)
                            {
                                Console.WriteLine($"\t{p.ProductName}");
                            }
                        }
                    }
                    else if (choice == "5")
                    {
                        var db = new Northwind_88_DWHContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category in which you'd like to add a product:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        // Displays the categories for the user

                        Product product = new Product();
                        product.CategoryId = id;
                        Console.WriteLine("Enter Product Name:");
                        product.ProductName = Console.ReadLine();

                        if (product != null)
                        {
                            db.AddProduct(product);
                            logger.Info("Product added - {name}", product.ProductName);
                        }

                    }
                    else if (choice == "6")
                    {
                        Console.WriteLine("Choose the product to edit:");
                        var db = new Northwind_88_DWHContext();
                        var product = GetProduct(db);
                        if (product != null)
                        {
                            Product updatedProduct = InputProduct(db);
                            if (updatedProduct != null)
                            {
                                updatedProduct.ProductId = product.ProductId;
                                db.EditProduct(updatedProduct);
                                logger.Info($"Product (id: {product.ProductId}) updated");
                            }
                        }
                    }
                    else if(choice == "7")
                    {
                        string productChoice;
                        var db = new Northwind_88_DWHContext();
                        do{
                            Console.WriteLine("Which type of product would you like to view? (Discontinued products will be colored red)");
                            Console.WriteLine("1) All products");
                            Console.WriteLine("2) Active products");
                            Console.WriteLine("3) Discontinued products");
                            Console.WriteLine("4) Display a specific product and it's details");
                            Console.WriteLine("\"q\" to quit");
                            productChoice = Console.ReadLine();
                            Console.Clear();

                            if (productChoice == "1")
                            {
                                var products = db.Products.OrderBy(b => b.ProductId);
                                foreach (Product b in products)
                                {
                                    if (b.Discontinued == true){
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"{b.ProductName}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                    else{
                                       Console.WriteLine($"{b.ProductName}"); 
                                    }
                                }
                                Console.WriteLine();

                            }
                            else if (productChoice == "2")
                            {
                                Console.WriteLine();
                                var products = db.Products.OrderBy(b => b.ProductId);
                                foreach (Product b in products)
                                {
                                    if (b.Discontinued == false){
                                        Console.WriteLine($"{b.ProductName}");
                                    }
                                }
                                Console.WriteLine();

                            }
                            else if (productChoice == "3")
                            {
                                Console.WriteLine();
                                var products = db.Products.OrderBy(b => b.ProductId);
                                foreach (Product b in products)
                                {
                                    if (b.Discontinued == true){
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"{b.ProductName}");
                                    }
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine();

                            }
                            else if (productChoice == "4")
                            {
                                Console.WriteLine("Choose the product number to display: ");
                                var products = db.Products.OrderBy(b => b.ProductId);
                                foreach (Product b in products)
                                {
                                    Console.WriteLine($"{b.ProductId}) {b.ProductName}");
                                }
                                int productIDNum = int.Parse(Console.ReadLine());
                                Console.Clear();

                                foreach (Product b in products)
                                {
                                    if (productIDNum == b.ProductId)
                                    {
                                        var price = String.Format("{0:C}", b.UnitPrice);
                                        Console.WriteLine($"{b.ProductName}");
                                        Console.WriteLine("---------------------");
                                        Console.WriteLine($"Quantity per unit: {b.QuantityPerUnit}");
                                        Console.WriteLine(price + " per unit");
                                        Console.WriteLine($"Units in stock: {b.UnitsInStock}");
                                        Console.WriteLine($"Units on order: {b.UnitsOnOrder}");
                                        Console.WriteLine($"Reorder level: {b.ReorderLevel}");
                                        if (b.Discontinued == true){
                                            Console.WriteLine("This product is discontinued and no longer available");
                                        }
                                        else{
                                            Console.WriteLine("This product is still available");
                                        }
                                    }
                                }
                                

                            }

                            Console.WriteLine();

                        }while (productChoice.ToLower() != "q");

                    }
                    
                    Console.WriteLine();


                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");

        }

        public static Product GetProduct(Northwind_88_DWHContext db)
        {
            // display all blogs
            var products = db.Products.OrderBy(b => b.ProductId);
            foreach (Product b in products)
            {
                Console.WriteLine($"{b.ProductId}: {b.ProductName}");
            }
            if (int.TryParse(Console.ReadLine(), out int ProductId))
            {
                Product product = db.Products.FirstOrDefault(b => b.ProductId == ProductId);
                if (product != null)
                {
                    return product;
                }
            }
            logger.Error("Invalid Product Id");
            return null;
        }

        public static Product InputProduct(Northwind_88_DWHContext db)
        {
            Product product = new Product();
            Console.WriteLine("Enter the Product name");
            product.ProductName = Console.ReadLine();

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Products.Any(b => b.ProductName == product.ProductName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Product name exists", new string[] { "ProductName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
                return null;
            }
            return product;
            
        }
    }
}
