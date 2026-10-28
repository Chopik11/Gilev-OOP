using System.IO;
using System.Data.SQLite;

public static class DatabaseInitializer
{
    private const string ConnectionString = "Data Source=enterprise.db;Version=3;";

    public static void Initialize()
    {
        using (var connection = new SQLiteConnection(ConnectionString))
        {
            connection.Open();

            string createTables = @"
                CREATE TABLE IF NOT EXISTS Categories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    DiscountRate REAL NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    CategoryId INTEGER NOT NULL,
                    Price REAL NOT NULL,
                    Stock INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Sales (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductId INTEGER NOT NULL,
                    CustomerName TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    SaleDate TEXT NOT NULL,
                    Channel TEXT NOT NULL
                );
            ";

            using (var command = new SQLiteCommand(createTables, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM Categories", connection))
            {
                long count = (long)checkCmd.ExecuteScalar();
                if (count == 0)
                {
                    string seedData = @"
                        INSERT INTO Categories (Name, DiscountRate) VALUES ('Электроника', 0.1);
                        INSERT INTO Categories (Name, DiscountRate) VALUES ('Одежда', 0.15);
                        INSERT INTO Categories (Name, DiscountRate) VALUES ('Продукты', 0.05);
                        INSERT INTO Categories (Name, DiscountRate) VALUES ('Книги', 0.2);

                        INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES ('Смартфон', 1, 300000, 10);
                        INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES ('Ноутбук', 1, 500000, 5);
                        INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES ('Куртка', 2, 45000, 15);
                        INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES ('Джинсы', 2, 20000, 25);
                        INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES ('Молоко', 3, 500, 100);
                        INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES ('Хлеб', 3, 300, 200);
                        INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES ('C# Programming', 4, 12000, 8);
                        INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES ('SQL Guide', 4, 9000, 12);
                        INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES ('Наушники', 1, 25000, 30);
                        INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES ('Футболка', 2, 7000, 50);

                        INSERT INTO Sales (ProductId, CustomerName, Quantity, SaleDate, Channel) VALUES (1, 'Иван Петров', 1, '2026-03-01', 'Online');
                        INSERT INTO Sales (ProductId, CustomerName, Quantity, SaleDate, Channel) VALUES (3, 'Анна Смирнова', 2, '2026-03-02', 'Retail');
                        INSERT INTO Sales (ProductId, CustomerName, Quantity, SaleDate, Channel) VALUES (5, 'Олег Иванов', 5, '2026-03-02', 'Online');
                        INSERT INTO Sales (ProductId, CustomerName, Quantity, SaleDate, Channel) VALUES (7, 'Мария Сидорова', 1, '2026-03-03', 'Retail');
                        INSERT INTO Sales (ProductId, CustomerName, Quantity, SaleDate, Channel) VALUES (2, 'Иван Петров', 1, '2026-03-04', 'Online');
                        INSERT INTO Sales (ProductId, CustomerName, Quantity, SaleDate, Channel) VALUES (4, 'Дмитрий Ковалев', 3, '2026-03-05', 'Retail');
                        INSERT INTO Sales (ProductId, CustomerName, Quantity, SaleDate, Channel) VALUES (6, 'Елена Васильева', 4, '2026-03-06', 'Online');
                        INSERT INTO Sales (ProductId, CustomerName, Quantity, SaleDate, Channel) VALUES (8, 'Анна Смирнова', 2, '2026-03-07', 'Retail');
                        INSERT INTO Sales (ProductId, CustomerName, Quantity, SaleDate, Channel) VALUES (9, 'Олег Иванов', 1, '2026-03-08', 'Online');
                        INSERT INTO Sales (ProductId, CustomerName, Quantity, SaleDate, Channel) VALUES (10, 'Мария Сидорова', 2, '2026-03-09', 'Retail');
                    ";
                    using (var seedCmd = new SQLiteCommand(seedData, connection))
                    {
                        seedCmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}