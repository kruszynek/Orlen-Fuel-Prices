using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Orlen_Fuel_Prices
{
    public class DataScraper
    {
        private MainWindow mainWindow;
        private List<string> scrapedData = new List<string>();
        string date;
        public DataScraper(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            scrapedData = new List<string>();
        }

        public List<string> GetScrapedData()
        {
            return scrapedData;
        }

        public void Scrapedata(string url, ChromeOptions options)
        {
            scrapedData.Clear();
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            using (var driver = new ChromeDriver(driverService, options))
            {
                driver.Navigate().GoToUrl(url);
                try
                {
                    Thread.Sleep(5000);
                    IWebElement dateElement = driver.FindElement(By.XPath("//p[@class='hcp__content-date']/STRONG"));
                    Debug.WriteLine("Connection Status:  Connection went sucsessfully");
                    if (dateElement != null)
                    {
                        string content = dateElement.Text;
                        date = content;
                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            mainWindow.DateText.Visibility = Visibility.Collapsed;
                            mainWindow.RowsStackPanel.Children.Clear();
                            var textBlock = new TextBlock
                            {
                                Text = "Paliwa Orlen:\n\n" + "Ceny obowiązujące od dnia: " + content,
                                Margin = new Thickness(0, 0, 0, 10),
                                FontFamily = new FontFamily("Segoe UI"),
                                FontSize = 20,
                                FontWeight = FontWeights.Bold,
                                Foreground = Brushes.Black
                            };

                            mainWindow.RowsStackPanel.Children.Add(textBlock);
                        });

                        scrapedData.Add("Ceny obowiązujące od dnia: " + content);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Date element not found");
                    }


                    IWebElement tableElement = driver.FindElement(By.XPath("//table"));
                    if (tableElement != null)
                    {

                        IWebElement theadElement = tableElement.FindElement(By.XPath(".//thead"));

                        var headerCells = theadElement.FindElements(By.XPath(".//th"));
                        var headerRowData = new List<string>();

                        foreach (var headerCell in headerCells)
                        {
                            string headerCellContent = headerCell.Text.Trim();
                            headerRowData.Add(headerCellContent);
                        }

                        scrapedData.Add(string.Join("\t\t\t\t\t\t", headerRowData));

                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            var headerTextBlock = new TextBlock
                            {
                                Text = string.Join("\t\t\t\t\t\t", headerRowData),
                                Margin = new Thickness(0, 0, 0, 10),
                                FontFamily = new FontFamily("Segoe UI"),
                                FontSize = 20,
                                FontWeight = FontWeights.Bold,
                                Foreground = Brushes.Black
                            };

                            var headerBorder = new Border
                            {
                                BorderBrush = Brushes.Black,
                                BorderThickness = new Thickness(0, 0, 0, 2),
                                Padding = new Thickness(5),
                            };

                            headerBorder.Child = headerTextBlock;
                            mainWindow.RowsStackPanel.Children.Add(headerBorder);
                        });

                        // Scraping tbody
                        var rows = tableElement.FindElements(By.XPath(".//tr"));

                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            var rows = tableElement.FindElements(By.XPath(".//tr"));
                            int[] columnWidths = { 70, 10, 0 };

                            foreach (var row in rows)
                            {
                                string scrapedName = null;
                                int scrapedPrice = 0;
                                string scrapedDate = date;

                                var cells = row.FindElements(By.XPath(".//td"));
                                var rowData = new List<string>();

                                int columnIndex = 0;
                                foreach (var cell in cells)
                                {
                                    string cellContent = cell.Text.Trim();
                                    cellContent = cellContent.PadRight(columnWidths[columnIndex]);
                                    rowData.Add(cellContent);

                                    if (columnIndex == 0)
                                    {
                                        scrapedName = cellContent;
                                    }
                                    else if (columnIndex == 1)
                                    {
                                        cellContent = cellContent.Replace(" ", "");
                                        scrapedPrice = int.Parse(cellContent);
                                    }

                                    columnIndex++;
                                }
                                if (rowData.Count > 0)
                                {
                                    scrapedData.Add(string.Join("\t", rowData));
                                }
                                // Save data to SQLite database
                                var sqliteHandler = new SQLiteDatabaseHandler("Data Source=OrlenFuelPricesData.db;Version=3;");
                                sqliteHandler.SaveScrapedData(scrapedName, scrapedPrice, scrapedDate);
                                if (rowData.Count > 0)
                                {
                                    var textBlock = new TextBlock
                                    {

                                        Text = string.Join("\t", rowData.Where(s => !string.IsNullOrEmpty(s))),
                                        Margin = new Thickness(0, 0, 0, 10),
                                        FontFamily = new FontFamily("Segoe UI"),
                                        FontSize = 20,
                                        Foreground = Brushes.Black

                                    };

                                    var border = new Border
                                    {
                                        BorderBrush = Brushes.Black,
                                        BorderThickness = new Thickness(0, 0, 0, 1),
                                        Padding = new Thickness(5),
                                    };

                                    border.Child = textBlock;
                                    mainWindow.RowsStackPanel.Children.Add(border);
                                }
                            }
                        });
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Table element not found");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Connection status: Failed, reason: " + ex);
                    MessageBoxResult result = System.Windows.MessageBox.Show("Failed to download data. Try again?",
                        "Error",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error);


                    if (result == MessageBoxResult.Yes)
                    {
                        Scrapedata(url, options);
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        mainWindow.Dispatcher.Invoke(() =>
                        {
                            mainWindow.DateText.Text = "Błąd w pobieraniu danych, możesz zamknąć aplikacje";
                        });
                    }
                }
            }
        }
    }
}