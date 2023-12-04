using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using App.Models;
using App.Utils;
using App.ViewModels;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using Splat;

namespace App.Views.Pages;

public partial class OrderPage : ReactiveUserControl<OrderViewModel>, IEnableLogger {
    public OrderPage() {
        InitializeComponent();

        ViewModel = new(
            () => new AppDatabase().Orders
                                   .Include(x => x.Status)
                                   .Include(x => x.Customer)
                                   .Include(x => x.Products)
                                   .ThenInclude(x => x.Product)
                                   .ToList(),
            OrderSelectors,
            DefaultOrderSelector,
            FilterSelectors,
            DefaultFilterSelector,
            EditItem,
            NewItem,
            RemoveItem
        );
    }

    private async void RemoveItem(Order? i) {
        if (i is null) {
            return;
        }

        var result = await MessageBoxUtils.ShowYesNoDialog(
                         "Подтверждение",
                         $"Вы действительно хотите удалить запись"
                     );
        if (result is not ContentDialogResult.Primary) return;
        await using var db = new AppDatabase();
        db.Orders.Remove(i);
        await db.SaveChangesAsync();
        ViewModel?.RemoveLocal(i);
    }

    private async Task NewItem() {
        var itemToEdit = new Order();
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

        dialog.AddControlValidation<Order>(stack.Children, async item => {
            if (item is null) return;
            item = item.Clone();
            await using var db = new AppDatabase();
            db.Attach(item);
            db.Orders.Add(item);
            await db.SaveChangesAsync();
            ViewModel!.AddLocal(item);
        });
        await dialog.ShowAsync();
    }

    private async void EditItem(Order? i) {
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

        dialog.AddControlValidation<Order>(stack.Children, async newItem => {
            if (newItem is null) return;
            await using var db = new AppDatabase();
            db.Attach(newItem);
            db.Orders.Update(newItem);
            await db.SaveChangesAsync();
            ViewModel!.ReplaceItem(i, newItem);
        });

        await dialog.ShowAsync();
    }

    private static readonly Dictionary<int, Func<Order, object>> OrderSelectors = new() {
        { 1, it => it.Id },
        { 2, it => it.Date },
        { 3, it => it.Time },
        { 4, it => it.Customer },
        { 5, it => it.Status },
    };

    private static readonly Dictionary<int, Func<string, Func<Order, bool>>> FilterSelectors = new() {
        { 1, query => it => it.Id.ToString().Contains(query) },
        { 2, query => it => it.Date.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 3, query => it => it.Time.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 4, query => it => it.Customer.FullName.Contains(query, StringComparison.InvariantCultureIgnoreCase) },
        { 5, query => it => it.Status.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase) },
    };

    private static object DefaultOrderSelector(Order it) => it.Id;

    private static Func<Order, bool> DefaultFilterSelector(string query)
        => it => it.Id.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Date.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Time.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Customer.FullName.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                 || it.Status.Name.ToString(CultureInfo.InvariantCulture)
                      .Contains(query, StringComparison.InvariantCultureIgnoreCase);

    private StackPanel GenerateDialogPanel(Order item) {
        using var db = new AppDatabase();
        var statuses = db.OrderStatus.ToList();
        var customers = db.Customers.ToList();

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
                    PlaceholderText = "Статус",
                    ItemsSource = statuses,
                    [!SelectingItemsControl.SelectedItemProperty] = new Binding("Status"),
                    [!SelectingItemsControl.SelectedValueProperty] = new Binding("Status.Id"),
                    DisplayMemberBinding = new Binding("Name"),
                    SelectedValueBinding = new Binding("Id"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                },
                new ComboBox() {
                    PlaceholderText = "Заказчик",
                    ItemsSource = customers,
                    [!SelectingItemsControl.SelectedItemProperty] = new Binding("Customer"),
                    [!SelectingItemsControl.SelectedValueProperty] = new Binding("Customer.Id"),
                    DisplayMemberBinding = new Binding("FullName"),
                    SelectedValueBinding = new Binding("Id"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                },
            }
        };

        return stack;
    }
}

public class OrderViewModel : TableViewModelBase<Order> {
    public ReactiveCommand<Order?, Unit> AddProductToOrderCommand { get; }

    public OrderViewModel(Func<List<Order>> databaseGetter, Dictionary<int, Func<Order, object>> orderSelectors,
                          Func<Order, object> defaultOrderSelector,
                          Dictionary<int, Func<string, Func<Order, bool>>> filterSelectors,
                          Func<string, Func<Order, bool>> defaultFilterSelector, Action<Order?> editItem,
                          Func<Task> newItem, Action<Order?> removeItem) : base(
        databaseGetter, orderSelectors, defaultOrderSelector, filterSelectors, defaultFilterSelector, editItem, newItem,
        removeItem) {
        var canAddProduct = this.WhenAnyValue(x => x.SelectedRow)
                                .Select(x => x is not null);
        AddProductToOrderCommand = ReactiveCommand.CreateFromTask<Order?>(AddProduct, canAddProduct);
    }

    private async Task AddProduct(Order? order) {
        await using var db = new AppDatabase();
        var products = db.Products.ToList();
        await db.DisposeAsync();
        var itemToEdit = new OrderedProduct() {
            Order = order,
            OrderId = order.Id,
            Quantity = 1,
        };
        var stack = new StackPanel {
            Spacing = 15,
            Children = {
                new ComboBox() {
                    PlaceholderText = "Товар",
                    ItemsSource = products,
                    DisplayMemberBinding = new Binding("Name"),
                    SelectedValueBinding = new Binding("Id"),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    [!SelectingItemsControl.SelectedItemProperty] = new Binding("Product"),
                    [!SelectingItemsControl.SelectedValueProperty] = new Binding("ProductId"),
                },
                new NumericUpDown() {
                    Watermark = "Итого",
                    ShowButtonSpinner = false,
                    Minimum = 0,
                    [!NumericUpDown.ValueProperty] = new Binding("Quantity"),
                },
            }
        };

        var dialog = new ContentDialog() {
            Title = "Добавление записи",
            PrimaryButtonText = "Создать",
            CloseButtonText = "Закрыть",
            DataContext = itemToEdit,
            Content = stack,
            DefaultButton = ContentDialogButton.Primary,
            [!ContentDialog.PrimaryButtonCommandParameterProperty] = new Binding(".")
        };

        dialog.AddControlValidation<OrderedProduct>(stack.Children, async item => {
            if (item is null) return;
            item = item.Clone();
            await using var db = new AppDatabase();
            if (db.OrdersProducts.Local.All(x => x.Id != item.Id)) {
                db.Attach(item);
            }
            db.OrdersProducts.Add(item);
            await db.SaveChangesAsync();
            AddOrderedProductLocal(item);
        });
        await dialog.ShowAsync();
    }

    void AddOrderedProductLocal(OrderedProduct orderedProduct) {
        orderedProduct.Order.Products.Add(orderedProduct);
    }
}