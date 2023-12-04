using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using App.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace App.Views.Templates; 

public class TableBaseView : TemplatedControl {
    public static readonly StyledProperty<ObservableCollection<DataGridColumn>> ColumnsProperty =
        AvaloniaProperty.Register<TableBaseView, ObservableCollection<DataGridColumn>>(
            nameof(Columns), new ObservableCollection<DataGridColumn>(), true, BindingMode.Default);
    public static readonly StyledProperty<Type?> ModelTypeProperty =
        AvaloniaProperty.Register<TableBaseView, Type?>(
            nameof(ModelType), null, true, BindingMode.Default);

    public static readonly StyledProperty<TableViewModelBase?> ViewModelProperty =
        AvaloniaProperty.Register<TableBaseView, TableViewModelBase?>(
            nameof(ViewModel), null, true, BindingMode.Default);

    public ObservableCollection<DataGridColumn> Columns {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public Type? ModelType {
        get => GetValue(ModelTypeProperty);
        set => SetValue(ModelTypeProperty, value);
    }

    public TableViewModelBase? ViewModel {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
}