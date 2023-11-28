using System;
using System.Linq;
using App.Views.Pages;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace App.Views.Windows;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        NavFrame.Navigate(
            NavView.MenuItems
                .Cast<NavigationViewItem>()
                .Select(x => (x.Tag as Type)!)
                .FirstOrDefault(typeof(StartPage)),
            null, 
            null
            );
    }

    private void NavView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e) {
        if (e.SelectedItem is NavigationViewItem { Tag: Type type }) {
            NavFrame.Navigate(type, null, new SlideNavigationTransitionInfo() {
                Effect = SlideNavigationTransitionEffect.FromBottom
            });
        }
    }
}