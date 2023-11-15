using System;
using System.Data.SQLite;

namespace Orlen_Fuel_Prices
{
    public class SQLiteDatabaseHandler
    {
        private readonly string connectionString;

        public SQLiteDatabaseHandler(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void SaveScrapedData(string name, int price, string date)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var countCommand = new SQLiteCommand("SELECT COUNT(*) FROM ScrapedData WHERE date = @date", connection))
                    {
                        countCommand.Parameters.AddWithValue("@date", date);
                        int rowCount = Convert.ToInt32(countCommand.ExecuteScalar());

                        if (rowCount < 5)
                        {

                            using (var insertCommand = new SQLiteCommand("INSERT INTO ScrapedData (name, price, date) VALUES (@name, @price, @date)", connection))
                            {
                                insertCommand.Parameters.AddWithValue("@name", name);
                                insertCommand.Parameters.AddWithValue("@price", price);
                                insertCommand.Parameters.AddWithValue("@date", date);
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {

                            Console.WriteLine("Cannot insert more than 5 records with the same date.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error saving data to SQLite database: " + ex.Message);
            }
        }
    }
}