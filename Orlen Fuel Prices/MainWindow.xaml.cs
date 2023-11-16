using Hardcodet.Wpf.TaskbarNotification;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Orlen_Fuel_Prices
{
    public partial class MainWindow : Window
    {

        private TaskbarIcon notifyIcon;
        private DispatcherTimer timer;
        private DataScraper dataScraper;
        private SplashScreen splashScreen;



        public MainWindow()
        {
            splashScreen = new SplashScreen();
            splashScreen.Show();
            this.Hide();
            this.dataScraper = new DataScraper(this);
            InitializeComponent();
            TenMinuteTimer();
            SetupNotifyIcon();
            ScrapedataAsync();
            
            DateText.Text = "Pobieranie danych...";
            
            
        }
        
        private void SetupNotifyIcon()
        {
            notifyIcon = new TaskbarIcon();
            notifyIcon.Visibility = Visibility.Visible;
            notifyIcon.Icon = new System.Drawing.Icon("crude-oil.ico");

            notifyIcon.ToolTipText = "Orlen Fuel Prices\n" + "Left click to show prices. " + "Double left click to open app";
            notifyIcon.TrayMouseDoubleClick += NotifyIcon_DoubleClick;
            notifyIcon.TrayLeftMouseUp += NotifyIcon_LeftClick;


        }
        private void TenMinuteTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(10);
            timer.Tick += (sender, e) =>
            {
                RowsStackPanel.Children.Clear();
                DateText.Visibility = Visibility.Visible;
                DateText.Text = "Aktualizacja w toku..";
                ScrapedataAsync();
            };
            timer.Start();




        }

        string scrapedDataAsString;
        private void NotifyIcon_LeftClick(object sender, RoutedEventArgs e)
        {



            ShowCustomBalloon(dataScraper.GetScrapedData());
        }

        private void ShowCustomBalloon(List<string> message)
        {
            var customBalloon = new CustomBalloon(message, notifyIcon);
            notifyIcon.ShowCustomBalloon(customBalloon, PopupAnimation.Scroll, null);
        }



        private void NotifyIcon_DoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }



        private async Task ScrapedataAsync()
        {
            await Task.Run(() =>
            {
                Scrapedata();
            });
            Dispatcher.Invoke(() =>
            splashScreen.Close()
            
            ) ;

            this.Show();

        }
        private void Scrapedata()
        {
            string url = "https://www.orlen.pl/pl/dla-biznesu/hurtowe-ceny-paliw?fbclid=IwAR0JYK4fmnVGlG6MtKgd9AKW_ziZVEe7hJOUZaXYA2R3gVFZOCVQjmurk48";
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--enable-gpu", "--enable-background-tracing", "--window-size=1920,1080", "f'user-agent={agent}'", "headless");
            options.PageLoadStrategy = PageLoadStrategy.Normal;
            dataScraper.Scrapedata(url, options);

        }
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();
            base.OnStateChanged(e);
        }
    }
}
