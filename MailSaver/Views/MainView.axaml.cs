using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace MailSaver.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
    public void SaveMail(object source, RoutedEventArgs args)
    {
        Debug.WriteLine("Click!");
    }
}
