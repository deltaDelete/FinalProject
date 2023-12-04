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
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;

namespace App.Views.Pages;

public partial class ProductPage : ReactiveUserControl<TableViewModelBase<Product>>
{
    public ProductPage()
    {
        InitializeComponent();

        ViewModel = new(
            () => new AppDatabase().Products.ToList(),
            OrderSelectors,
            DefaultOrderSelector,
            FilterSelectors,
            DefaultFilterSelector,
            EditItem,
            NewItem,
            RemoveItem
        );
    }

    private async void RemoveItem(Product? i)
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
        db.Products.Remove(i);
        await db.SaveChangesAsync();
        ViewModel?.RemoveLocal(i);
    }

    private async Task NewItem()
    {
        var itemToEdit = new Product();
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

        dialog.AddControlValidation<Product>(stack.Children, async item =>
        {
            if (item is null) return;
            await using var db = new AppDatabase();
            db.Attach(item);
            db.Products.Add(item);
            await db.SaveChangesAsync();
            ViewModel!.AddLocal(item);
        });
        await dialog.ShowAsync();
    }

    private async void EditItem(Product? i)
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

        dialog.AddControlValidation<Product>(stack.Children, async item =>
        {
            if (item is null) return;
            await using var db = new AppDatabase();
            db.Attach(item);
            db.Products.Update(item);
            await db.SaveChangesAsync();
            ViewModel!.ReplaceItem(i, item);
        });

        await dialog.ShowAsync();
    }

    private static readonly Dictionary<int, Func<Product, object>> OrderSelectors = new()
    {
        { 1, it => it.Id },
        { 2, it => it.Name },
        { 3, it => it.Price },
    };

    private static readonly Dictionary<int, Func<string, Func<Product, bool>>> FilterSelectors = new()
    {
        { 1, query => it => it.Id.ToString().Contains(query) },
        { 2, query => it => it.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 3, query => it => it.Price.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.InvariantCultureIgnoreCase) },
    };

    private static object DefaultOrderSelector(Product it) => it.Id;

    private static Func<Product, bool> DefaultFilterSelector(string query)
        => it => it.Id.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Price.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.InvariantCultureIgnoreCase);

    private StackPanel GenerateDialogPanel()
    {
        var stack = new StackPanel
        {
            Spacing = 15,
            Children =
            {
                new TextBox() {
                    Watermark = "Наименование",
                    [!TextBox.TextProperty] = new Binding("Name")
                },
                new NumericUpDown()
                {
                    Watermark = "Стоимость",
                    ShowButtonSpinner = false,
                    [!NumericUpDown.ValueProperty] = new Binding("Price")
                },
            }
        };
        return stack;
    }
}