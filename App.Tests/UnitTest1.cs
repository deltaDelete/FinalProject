using App.Models;
using App.Utils;
using Avalonia.Collections;

namespace App.Tests;

public class ExtensionsTest {
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
    
    
}