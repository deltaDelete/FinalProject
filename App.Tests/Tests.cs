using System.Reactive.Linq;
using App.Models;
using App.Utils;
using App.ViewModels;
using AutoFixture;
using Avalonia.Collections;
using ReactiveUI;

namespace App.Tests;

public class Tests {
    private readonly IFixture _fixture;

    public Tests() {
        _fixture = new Fixture();
    }

    [Fact]
    public void Generic_Clone_Test() {
        var orderedProduct = (int id) =>
            new OrderedProduct() {
                Id = id
            };
        var obj = new Product() {
            Id = 1,
            Name = "Name",
            Price = 1000.10M,
            Orders = new(Enumerable.Range(0, 10).Select(x => orderedProduct(x)))
        };

        var result = Utils.Extensions.Clone(obj);

        Assert.Multiple(
            () => Assert.Equal(obj.Id, result.Id),
            () => Assert.Equal(obj.Name, result.Name),
            () => Assert.Equal(obj.Name, result.Name),
            () => Assert.NotEqual(obj.Orders, result.Orders)
        );
    }

    [Fact]
    public void TableViewModelBase_Database_Initialization_Test() {
        var (list, actual) = CreateViewModel();

        Assert.NotNull(actual.Items);
        Assert.NotNull(actual.Pages);
        Assert.NotEmpty(actual.Items);
        Assert.NotEmpty(actual.Pages);
    }

    [Fact]
    public void TableViewModelBase_TotalPages_Test() {
        var (list, actual) = CreateViewModel();

        Assert.Equal(actual.Pages.Count, actual.TotalPages);
    }

    [Fact]
    public async void TableViewModelBase_OnSearchChanged_Test() {
        var (list, actual) = CreateViewModel();

        actual.Search(list.First().Name, 0, false);

        Assert.Contains(list.First(), actual.Items);
    }

    [Fact]
    public void TableViewModelBase_TakeFirst_Test() {
        var (list, actual) = CreateViewModel();

        actual.TakeFirst();

        Assert.Equal(1, actual.CurrentPage);
        Assert.Equivalent(actual.Pages.First(), actual.Filtered);
    }

    [Fact]
    public void TableViewModelBase_TakeLast_Test() {
        var (list, actual) = CreateViewModel();

        actual.TakeLast();

        Assert.Equal(actual.Pages.Count, actual.CurrentPage);
        Assert.Equivalent(actual.Pages.Last(), actual.Filtered);
    }

    [Fact]
    public void TableViewModelBase_TakeNext_Test() {
        var (list, actual) = CreateViewModel();

        actual.TakeNext();

        Assert.Equal(2, actual.CurrentPage);
        Assert.Equivalent(actual.Pages[actual.CurrentPage - 1], actual.Filtered);
    }

    [Fact]
    public void TableViewModelBase_TakePrev_Test() {
        var (list, actual) = CreateViewModel();
        actual.TakeNext();

        actual.TakePrev();

        Assert.Equal(1, actual.CurrentPage);
        Assert.Equivalent(actual.Pages.First(), actual.Filtered);
    }

    [Fact]
    public void TableViewModelBase_ReplaceItem_Test() {
        var (list, actual) = CreateViewModel();
        var itemToReplace = list.First();
        var newItem = new TestType {
            Id = 1,
            Name = "TestName"
        };

        actual.ReplaceItem(itemToReplace, newItem);
        
        Assert.Equal(newItem, actual.Items.First());
    }

    [Fact]
    public void TableViewModelBase_AddLocal_Test() {
        var (list, actual) = CreateViewModel();
        var newItem = new TestType {
            Id = 1,
            Name = "TestName"
        };

        actual.AddLocal(newItem);
        
        Assert.Equal(newItem, actual.Filtered.Last());
    }

    private (List<TestType> list, TableViewModelBase<TestType> actual) CreateViewModel() {
        var list = _fixture.CreateMany<TestType>(50).ToList();
        var actual = TestTableViewModelFactory(list);
        actual.Activator.Activate();
        actual.GetDataFromDbSynchronous();
        return (list, actual);
    }

    private static readonly Func<List<TestType>, TableViewModelBase<TestType>> TestTableViewModelFactory =
        (List<TestType> dataset) => new TableViewModelBase<TestType>(
            () => dataset,
            new Dictionary<int, Func<TestType, object>>() {
                { 1, it => it.Id },
                { 2, it => it.Name },
            },
            it => it.Id,
            new Dictionary<int, Func<string, Func<TestType, bool>>>() {
                { 1, query => it => it.Id.ToString().Contains(query) },
                { 2, query => it => it.Name.ToString().Contains(query) },
            },
            (string query) => (it) =>
                it.Id.ToString().Contains(query, StringComparison.InvariantCultureIgnoreCase)
                || it.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase),
            type => { },
            () => Task.CompletedTask,
            type => { }
        );

    public class TestType {
        public int Id { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
    }
}