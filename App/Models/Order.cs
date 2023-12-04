using System;
using System.ComponentModel.DataAnnotations;
using Avalonia.Collections;
using Newtonsoft.Json;
using ReactiveUI;

namespace App.Models;

[JsonObject]
public class Order : ReactiveObject {
    private int _id;
    private DateTimeOffset _date = DateTimeOffset.Now;
    private TimeSpan _time = DateTimeOffset.Now.TimeOfDay;
    private Customer? _customer = null;
    private OrderStatus? _status = null;
    private AvaloniaList<OrderedProduct>? _products;

    [Key]
    [JsonProperty]
    public int Id {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    [Required]
    [JsonProperty]
    public DateTimeOffset Date {
        get => _date;
        set => this.RaiseAndSetIfChanged(ref _date, value);
    }

    [Required]
    [JsonProperty]
    public TimeSpan Time {
        get => _time;
        set => this.RaiseAndSetIfChanged(ref _time, value);
    }

    [Required]
    [JsonProperty]
    public Customer? Customer {
        get => _customer;
        set => this.RaiseAndSetIfChanged(ref _customer, value);
    }

    [Required]
    [JsonProperty]
    public OrderStatus? Status {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    [JsonProperty]
    public AvaloniaList<OrderedProduct>? Products {
        get => _products;
        set => this.RaiseAndSetIfChanged(ref _products, value);
    }
}
