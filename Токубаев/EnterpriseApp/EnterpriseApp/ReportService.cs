using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;

public class ReportService
{
    private const string ConnectionString = "Data Source=enterprise.db;Version=3;";

    public List<Category> GetCategories()
    {
        var list = new List<Category>();
        using (var conn = new SQLiteConnection(ConnectionString))
        {
            conn.Open();
            using (var cmd = new SQLiteCommand("SELECT Id, Name, DiscountRate FROM Categories", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Category { Id = reader.GetInt32(0), Name = reader.GetString(1), DiscountRate = reader.GetDouble(2) > 0 ? (decimal)reader.GetDouble(2) : 0 });
                    }
                }
            }
        }
        return list;
    }

    public List<Product> GetProducts()
    {
        var list = new List<Product>();
        using (var conn = new SQLiteConnection(ConnectionString))
        {
            conn.Open();
            using (var cmd = new SQLiteCommand("SELECT Id, Name, CategoryId, Price, Stock FROM Products", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Product { Id = reader.GetInt32(0), Name = reader.GetString(1), CategoryId = reader.GetInt32(2), Price = (decimal)reader.GetDouble(3), Stock = reader.GetInt32(4) });
                    }
                }
            }
        }
        return list;
    }

    public List<Sale> GetSales()
    {
        var list = new List<Sale>();
        using (var conn = new SQLiteConnection(ConnectionString))
        {
            conn.Open();
            using (var cmd = new SQLiteCommand("SELECT Id, ProductId, CustomerName, Quantity, SaleDate, Channel FROM Sales", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Sale { Id = reader.GetInt32(0), ProductId = reader.GetInt32(1), CustomerName = reader.GetString(2), Quantity = reader.GetInt32(3), SaleDate = reader.GetString(4), Channel = reader.GetString(5) });
                    }
                }
            }
        }
        return list;
    }

    public List<SaleReportDto> GetSaleReports()
    {
        var sales = GetSales();
        var products = GetProducts();
        var categories = GetCategories();

        var query = from s in sales
                    join p in products on s.ProductId equals p.Id
                    join c in categories on p.CategoryId equals c.Id
                    select new SaleReportDto
                    {
                        CustomerName = s.CustomerName,
                        ProductName = p.Name,
                        CategoryName = c.Name,
                        DiscountedPrice = p.Price * (1 - c.DiscountRate),
                        TotalSum = (p.Price * (1 - c.DiscountRate)) * s.Quantity
                    };
        return query.ToList();
    }

    public List<SaleReportDto> GetFilteredAndSortedReports(string categoryFilter, string channelFilter, int pageNumber, int pageSize)
    {
        var sales = GetSales();
        var products = GetProducts();
        var categories = GetCategories();

        var query = from s in sales
                    join p in products on s.ProductId equals p.Id
                    join c in categories on p.CategoryId equals c.Id
                    select new { s, p, c };

        if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "Все")
        {
            query = query.Where(x => x.c.Name == categoryFilter);
        }

        var sortedQuery = query
            .OrderBy(x => x.c.Name)
            .ThenByDescending(x => (x.p.Price * (1 - x.c.DiscountRate)) * x.s.Quantity);

        var pagedData = sortedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SaleReportDto
            {
                CustomerName = x.s.CustomerName,
                ProductName = x.p.Name,
                CategoryName = x.c.Name,
                DiscountedPrice = x.p.Price * (1 - x.c.DiscountRate),
                TotalSum = (x.p.Price * (1 - x.c.DiscountRate)) * x.s.Quantity
            });

        return pagedData.ToList();
    }

    public (object[] groupedData, string auditString) GetGroupedAnalytics()
    {
        var reports = GetSaleReports();

        var groupQuery = reports
            .GroupBy(r => r.CategoryName)
            .Select(g => new
            {
                Category = g.Key,
                DealCount = g.Count(),
                TotalRevenue = g.Sum(x => x.TotalSum),
                AverageCheck = g.Average(x => x.TotalSum),
                MinPurchase = g.Min(x => x.TotalSum),
                MaxPurchase = g.Max(x => x.TotalSum)
            })
            .ToArray();

        decimal grandTotal = reports.Aggregate(0m, (sum, next) => sum + next.TotalSum);
        string audit = $"Глобальный аудит: Общая выручка по всем категориям составляет {grandTotal:N0} тенге.";

        return (groupQuery, audit);
    }

    public List<Product> GetSetProducts(string operationType)
    {
        var sales = GetSales();
        var products = GetProducts();

        var onlineProductIds = sales.Where(s => s.Channel == "Online").Select(s => s.ProductId).Distinct();
        var retailProductIds = sales.Where(s => s.Channel == "Retail").Select(s => s.ProductId).Distinct();

        var onlineProducts = products.Where(p => onlineProductIds.Contains(p.Id)).ToList();
        var retailProducts = products.Where(p => retailProductIds.Contains(p.Id)).ToList();

        if (operationType == "Except")
        {
            return onlineProducts.Except(retailProducts).ToList();
        }
        else if (operationType == "Intersect")
        {
            return onlineProducts.Intersect(retailProducts).ToList();
        }
        else
        {
            return onlineProducts.Union(retailProducts).ToList();
        }
    }
}