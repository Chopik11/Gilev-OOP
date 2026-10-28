using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Pharmacy.DB
{
    public static class DatabaseHelper
    {
        private static string _dbPath = "pharmacy.db";
        private static string ConnectionString => $"Data Source={_dbPath};Version=3;";

        public static void InitializeDatabase()
        {
            bool isNew = !File.Exists(_dbPath);

            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();

                string sql = @"
                    CREATE TABLE IF NOT EXISTS Categories (
                        id   INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Medicines (
                        id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        name        TEXT    NOT NULL,
                        category_id INTEGER,
                        price       REAL    NOT NULL,
                        stock       INTEGER NOT NULL,
                        expiry_date TEXT    NOT NULL,
                        FOREIGN KEY (category_id) REFERENCES Categories(id)
                    );

                    CREATE TABLE IF NOT EXISTS Sales (
                        id    INTEGER PRIMARY KEY AUTOINCREMENT,
                        date  TEXT    NOT NULL,
                        total REAL    NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS SaleItems (
                        id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        sale_id     INTEGER NOT NULL,
                        medicine_id INTEGER NOT NULL,
                        quantity    INTEGER NOT NULL,
                        price       REAL    NOT NULL,
                        FOREIGN KEY (sale_id)     REFERENCES Sales(id),
                        FOREIGN KEY (medicine_id) REFERENCES Medicines(id)
                    );
                ";

                using (var cmd = new SQLiteCommand(sql, conn))
                    cmd.ExecuteNonQuery();

                if (isNew)
                    SeedData(conn);
            }
        }

        private static void SeedData(SQLiteConnection conn)
        {
            string cats = @"
                INSERT INTO Categories (name) VALUES ('Таблетки');
                INSERT INTO Categories (name) VALUES ('Сиропы');
                INSERT INTO Categories (name) VALUES ('Капли');
                INSERT INTO Categories (name) VALUES ('Мази');
                INSERT INTO Categories (name) VALUES ('Капсулы');
            ";
            using (var cmd = new SQLiteCommand(cats, conn))
                cmd.ExecuteNonQuery();

            string meds = @"
                INSERT INTO Medicines (name, category_id, price, stock, expiry_date)
                VALUES
                    ('Парацетамол 500мг',    1, 45.00, 120, '2026-08-01'),
                    ('Ибупрофен 400мг',      1, 89.00,  80, '2026-12-15'),
                    ('Амоксициллин 500мг',   5, 235.00,  50, '2025-05-01'),
                    ('Но-Шпа',               1, 130.00,  60, '2027-03-20'),
                    ('Бромгексин сироп',     2, 95.00,  40, '2026-06-10'),
                    ('Нафтизин капли',       3, 55.00,  75, '2026-01-25'),
                    ('Диклофенак гель',      4, 185.00,  35, '2026-09-30'),
                    ('Омепразол 20мг',       5, 210.00,  45, '2026-11-05');
            ";
            using (var cmd = new SQLiteCommand(meds, conn))
                cmd.ExecuteNonQuery();
        }

        public static SQLiteConnection GetConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static DataTable ExecuteQuery(string sql, SQLiteParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                var dt = new DataTable();
                using (var adapter = new SQLiteDataAdapter(cmd))
                    adapter.Fill(dt);
                return dt;
            }
        }

        public static int ExecuteNonQuery(string sql, SQLiteParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, SQLiteParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }
    }
}
