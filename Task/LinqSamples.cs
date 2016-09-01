// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

		[Category("Restriction Operators")]
		[Title("Where - Task 1")]
		[Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
		public void Linq1()
		{
			int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

			var lowNums =
				from num in numbers
				where num < 5
				select num;

			Console.WriteLine("Numbers < 5:");
			foreach (var x in lowNums)
			{
				Console.WriteLine(x);
			}
		}

		[Category("Restriction Operators")]
		[Title("Where - Task 2")]
		[Description("This sample return return all presented in market products")]

		public void Linq2()
		{
			var products =
				from p in dataSource.Products
				where p.UnitsInStock > 0
				select p;

			foreach (var p in products)
			{
				ObjectDumper.Write(p);
			}
		}

        [Category("Aggregate Operators")]
        [Title("SUM - Task 001")]
        [Description("This sample returns the list of clients with the sum of all orders bigger than X")]

        public void Linq001()
        {
            //--One value
            int x = 10000;
            var clients =
                from c in dataSource.Customers
                where c.Orders.Sum(o => o.Total) > x
                select c.CompanyName;

            foreach (var c in clients)
            {
                ObjectDumper.Write(c);
            }

            //--Many values
            //int[] values = {10000,100000};

            //for (int i = 0; i < values.Length; i++)
            //{
            //    var clients =
            //        from c in dataSource.Customers
            //        where c.Orders.Sum(o => o.Total) > values[i]
            //        select c.CompanyName;

            //    foreach (var c in clients)
            //    {
            //        ObjectDumper.Write(c);
            //    }
            //}

        }

        [Category("Join Operators")]
        [Title("Join - Task 002")]
        [Description("This sample returns the list of suppliers for each customer from the same city and the same country")]
        public void Linq002()
        {
            var custSupQuery =
                from supl in dataSource.Suppliers
                join cust in dataSource.Customers on new { supl.Country, supl.City } equals new { cust.Country, cust.City }
                select new { cust.CompanyName, supl.SupplierName};

            foreach (var c in custSupQuery)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 003")]
        [Description("This sample returns the list of clients with the orders bigger than X")]
        public void Linq003()
        {
            decimal X = 100;

            var customers = dataSource.Customers
                .Where(c => c.Orders.Where(z => z.Total > X) != null)
                .Select(w => w.CompanyName);

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Element Operators")]
        [Title("FirstOrDefault - Task 004")]
        [Description("This sample returns the list of clients , month and year of the first order")]
        public void Linq004()
        {
            var customers = (dataSource.Customers
                .Select(x => Tuple.Create(x.CompanyName, (x.Orders.Where(c => c != null).OrderBy(t => t.OrderDate)).Select(y => y.OrderDate).FirstOrDefault())))
                .Select(w=> new { CompanyName = w.Item1, Month = w.Item2.Month, Year = w.Item2.Year});

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Ordering Operators")]
        [Title("OrderBy - Task 005")]
        [Description("This sample returns the list sorted by year, month, turnover (from highest to lowest), the clients name")]
        public void Linq005()
        {
            var customers = dataSource.Customers
                            .Select(x =>
                                x.Orders.Select(y =>
                                new { CompanyName = x.CompanyName, Year = y.OrderDate.Year, Month = y.OrderDate.Month, Total = y.Total })
                            .OrderBy(w => w.Year)
                            .ThenBy(w => w.Month)
                            .ThenByDescending(w => w.Total)
                            .ThenBy(w => w.CompanyName)
                            );

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Element Operators")]
        [Title("ElementAt - Task 006")]
        [Description("This sample returns the list of clients who has nonnumeric PostalCode, no Region and no open brace at the begining of the Phone")]
        public void Linq006()
        {
            int temp;
            var customers = dataSource.Customers
                .Where(v => 
                    (v.Region == null || v.Region.Trim().Length == 0) 
                    &&(v.Phone == null || !v.Phone.ElementAt(0).Equals('('))
                    &&( v.PostalCode == null || !int.TryParse(v.PostalCode, out temp)))
                            .Select(w => new { Name = w.CompanyName, PostalCode = w.PostalCode, Phone = w.Phone, Region = w.Region });

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Grouping Operators")]
        [Title("Group by- Task 007")]
        [Description("This sample returns the list of Products grouped by Category, then by UnitsInStock, then by UnitPrice")]
        public void Linq007()
        {
            var productGroups =
                from prod in dataSource.Products
                group prod by prod.Category into prodGroupCategory
                select
                    new
                    {
                        Category = prodGroupCategory.Key,
                        Products =
                            from prod in prodGroupCategory
                            group prod by prod.UnitsInStock into prodGroupInStock
                            select
                                new
                                {
                                    UnitsInStock = prodGroupInStock.Key,
                                    prodGroupCategory =
                                        from prod in prodGroupInStock
                                        group prod by prod.UnitPrice into prodGroupPrice
                                        select new { UnitPrice = prodGroupPrice.Key, Products = prodGroupPrice }
                                }
                    };

            foreach (var c in productGroups)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Grouping Operators")]
        [Title("Group by- Task 008")]
        [Description("This sample returns the list of Products grouped by 'cheap', 'average', 'expensive'")]
        public void Linq008()
        {
            Dictionary<string, decimal> ranges = new Dictionary<string, decimal>
            {
                { "cheap",  10.0000M },
                { "average ", 50.0000M },
                { "expensive", 300.0000M  }
            };

            var priceGroups = dataSource.Products
                        .GroupBy(x => ranges.FirstOrDefault(r => r.Value > x.UnitPrice))
                        .Select(g => new { Price = g.Key.Key, Products = g })
                        .ToList()
                        ;

            foreach (var c in priceGroups)
            {
                ObjectDumper.Write(c);
            }
        }
       
    }
}
