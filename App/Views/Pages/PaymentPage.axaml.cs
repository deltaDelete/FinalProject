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
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using Microsoft.EntityFrameworkCore;

namespace App.Views.Pages; 

public partial class PaymentPage : ReactiveUserControl<TableViewModelBase<Payment>> {
    public PaymentPage() {
        InitializeComponent();

        ViewModel = new(
            () => new AppDatabase().Payments
                                   .Include(x => x.Order)
                                   .ToList(),
            PaymentSelectors,
            DefaultPaymentSelector,
            FilterSelectors,
            DefaultFilterSelector,
            EditItem,
            NewItem,
            RemoveItem
        );
    }

    private async void RemoveItem(Payment? i) {
        if (i is null) {
            return;
        }

        var result = await MessageBoxUtils.ShowYesNoDialog(
                         "Подтверждение",
                         $"Вы действительно хотите удалить запись"
                     );
        if (result is not ContentDialogResult.Primary) return;
        await using var db = new AppDatabase();
        db.Payments.Remove(i);
        await db.SaveChangesAsync();
        ViewModel?.RemoveLocal(i);
    }

    private async Task NewItem() {
        var itemToEdit = new Payment();
        var stack = GenerateDialogPanel(itemToEdit);

        var dialog = new ContentDialog() {
            Title = "Добавление записи",
            PrimaryButtonText = "Создать",
            CloseButtonText = "Закрыть",
            DataContext = itemToEdit,
            Content = stack,
            DefaultButton = ContentDialogButton.Primary,
            [!ContentDialog.PrimaryButtonCommandParameterProperty] = new Binding(".")
        };

        dialog.AddControlValidation<Payment>(stack.Children, async item => {
            if (item is null) return;
            item = item.Clone();
            await using var db = new AppDatabase();
            db.Attach(item);
            db.Payments.Add(item);
            await db.SaveChangesAsync();
            ViewModel!.AddLocal(item);
        });
        await dialog.ShowAsync();
    }

    private async void EditItem(Payment? i) {
        if (i is null) return;
        var stack = GenerateDialogPanel(i);
        var dialog = new ContentDialog() {
            Title = "Изменение записи",
            PrimaryButtonText = "Изменить",
            CloseButtonText = "Закрыть",
            DataContext = i.Clone(),
            Content = stack,
            DefaultButton = ContentDialogButton.Primary,
            [!ContentDialog.PrimaryButtonCommandParameterProperty] = new Binding(".")
        };

        dialog.AddControlValidation<Payment>(stack.Children, async newItem => {
            if (newItem is null) return;
            await using var db = new AppDatabase();
            db.Attach(newItem);
            db.Payments.Update(newItem);
            await db.SaveChangesAsync();
            ViewModel!.ReplaceItem(i, newItem);
        });

        await dialog.ShowAsync();
    }

    private static readonly Dictionary<int, Func<Payment, object>> PaymentSelectors = new() {
        { 1, it => it.Id },
        { 2, it => it.Date },
        { 3, it => it.Time },
        { 4, it => it.Order },
        { 5, it => it.Total },
    };

    private static readonly Dictionary<int, Func<string, Func<Payment, bool>>> FilterSelectors = new() {
        { 1, query => it => it.Id.ToString().Contains(query) },
        { 2, query => it => it.Date.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 3, query => it => it.Time.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 4, query => it => it.Order.Id.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 5, query => it => it.Total.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.InvariantCultureIgnoreCase) },
    };

    private static object DefaultPaymentSelector(Payment it) => it.Id;

    private static Func<Payment, bool> DefaultFilterSelector(string query)
        => it => it.Id.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Date.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Time.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Order.Id.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Total.ToString(CultureInfo.InvariantCulture)
                      .Contains(query, StringComparison.InvariantCultureIgnoreCase);

    private StackPanel GenerateDialogPanel(Payment item) {
        using var db = new AppDatabase();
        var customers = db.Orders.ToList();

        var stack = new StackPanel {
            Spacing = 15,
            Children = {
                new DatePicker() {
                    [!DatePicker.SelectedDateProperty] = new Binding("Date"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                },
                new TimePicker() {
                    [!TimePicker.SelectedTimeProperty] = new Binding("Time"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                },
                new ComboBox() {
                    PlaceholderText = "Заказ",
                    ItemsSource = customers,
                    [!SelectingItemsControl.SelectedItemProperty] = new Binding("Order"),
                    [!SelectingItemsControl.SelectedValueProperty] = new Binding("Order.Id"),
                    DisplayMemberBinding = new Binding("Id"),
                    SelectedValueBinding = new Binding("Id"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                },
                new NumericUpDown() {
                    Watermark = "Итого",
                    ShowButtonSpinner = false,
                    Minimum = 0,
                    FormatString = "#.00",
                    [!NumericUpDown.ValueProperty] = new Binding("Total"),
                },
            }
        };

        return stack;
    }
}