using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Utils;
using App.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;

namespace App.Views.Pages; 

public partial class CustomerPage : ReactiveUserControl<TableViewModelBase<Customer>> {
    public CustomerPage() {
        InitializeComponent();

        ViewModel = new(
            () => new AppDatabase().Customers.ToList(),
            OrderSelectors,
            DefaultOrderSelector,
            FilterSelectors,
            DefaultFilterSelector,
            EditItem,
            NewItem,
            RemoveItem
        );
    }

    private async void RemoveItem(Customer? i)
    {
        if (i is null)
        {
            return;
        }

        var result = await MessageBoxUtils.ShowYesNoDialog(
            "Подтверждение",
            $"Вы действительно хотите удалить запись"
        );
        if (result is not ContentDialogResult.Primary) return;
        await using var db = new AppDatabase();
        db.Customers.Remove(i);
        await db.SaveChangesAsync();
        ViewModel?.RemoveLocal(i);
    }

    private async Task NewItem()
    {
        var itemToEdit = new Customer();
        var stack = GenerateDialogPanel();

        var dialog = new ContentDialog()
        {
            Title = "Добавление записи",
            PrimaryButtonText = "Создать",
            CloseButtonText = "Закрыть",
            DataContext = itemToEdit,
            Content = stack,
            DefaultButton = ContentDialogButton.Primary,
            [!ContentDialog.PrimaryButtonCommandParameterProperty] = new Binding(".")
        };

        dialog.AddControlValidation<Customer>(stack.Children, async item =>
        {
            if (item is null) return;
            await using var db = new AppDatabase();
            db.Attach(item);
            db.Customers.Add(item);
            await db.SaveChangesAsync();
            ViewModel!.AddLocal(item);
        });
        await dialog.ShowAsync();
    }

    private async void EditItem(Customer? i)
    {
        if (i is null) return;
        var stack = GenerateDialogPanel();
        var dialog = new ContentDialog()
        {
            Title = "Изменение записи",
            PrimaryButtonText = "Изменить",
            CloseButtonText = "Закрыть",
            DataContext = i,
            Content = stack,
            DefaultButton = ContentDialogButton.Primary,
            [!ContentDialog.PrimaryButtonCommandParameterProperty] = new Binding(".")
        };

        dialog.AddControlValidation<Customer>(stack.Children, async item =>
        {
            if (item is null) return;
            await using var db = new AppDatabase();
            db.Attach(item);
            db.Customers.Update(item);
            await db.SaveChangesAsync();
            ViewModel!.ReplaceItem(i, item);
        });

        await dialog.ShowAsync();
    }

    private static readonly Dictionary<int, Func<Customer, object>> OrderSelectors = new()
    {
        { 1, it => it.Id },
        { 2, it => it.FullName },
        { 3, it => it.Address },
        { 4, it => it.Phone },
        { 5, it => it.Email },
    };

    private static readonly Dictionary<int, Func<string, Func<Customer, bool>>> FilterSelectors = new()
    {
        { 1, query => it => it.Id.ToString().Contains(query) },
        { 2, query => it => it.FullName.Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 3, query => it => it.Address.Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 4, query => it => it.Phone.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 5, query => it => it.Email.Contains(query, StringComparison.InvariantCultureIgnoreCase) },
    };

    private static object DefaultOrderSelector(Customer it) => it.Id;

    private static Func<Customer, bool> DefaultFilterSelector(string query)
        => it => it.Id.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.FullName.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Address.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Phone.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Email.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.InvariantCultureIgnoreCase);

    private StackPanel GenerateDialogPanel()
    {
        var stack = new StackPanel
        {
            Spacing = 15,
            Children =
            {
                new TextBox() {
                    Watermark = "ФИО",
                    [!TextBox.TextProperty] = new Binding("FullName")
                },
                new TextBox() {
                    Watermark = "Адрес",
                    [!TextBox.TextProperty] = new Binding("Address")
                },
                new NumericUpDown() {
                    Watermark = "Телефон",
                    ShowButtonSpinner = false,
                    Minimum = 0,
                    FormatString = "+0 (###) ###-####",
                    [!NumericUpDown.ValueProperty] = new Binding("Phone"),
                },
                new TextBox() {
                    Watermark = "Эл. почта",
                    [!TextBox.TextProperty] = new Binding("Email")
                },
            }
        };
        
        
        
        return stack;
    }
}