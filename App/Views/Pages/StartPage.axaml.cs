using App.ViewModels.Pages;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace App.Views.Pages; 

public partial class StartPage : UserControl {
    public StartPage() {
        InitializeComponent();
        DataContext = new StartPageViewModel();
    }
}