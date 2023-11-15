using Hardcodet.Wpf.TaskbarNotification;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public class CustomBalloon : UserControl
{
    private TaskbarIcon taskbarIcon;
    public CustomBalloon(List<string> messages, TaskbarIcon taskbarIcon)
    {
        InitializeComponent();


        this.taskbarIcon = taskbarIcon;


        for (int i = 0; i < messages.Count; i++)
        {
            var messageTextBlock = CreateMessageTextBlock(messages[i]);
            Grid.SetRow(messageTextBlock, i + 1);
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                Padding = new Thickness(5),
            };
            border.Child = messageTextBlock;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(border, i + 1);

            grid.Children.Add(border);
        }

        MouseLeftButtonDown += CustomBalloon_MouseLeftButtonDown;
    }

    private TextBlock CreateMessageTextBlock(string message)
    {
        var messageTextBlock = new TextBlock
        {
            FontSize = 16,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Left,
            Foreground = Brushes.Black,
            Text = message
        };


        return messageTextBlock;
    }

    private void InitializeComponent()
    {
        grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var balloonBorder = new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(5),
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(1),
            Child = grid
        };
        Content = balloonBorder;


    }

    private void CustomBalloon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        taskbarIcon?.CloseBalloon();
    }

    private Grid grid;
}