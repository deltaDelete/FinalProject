# База данных

```mermaid
erDiagram
    Customers ||--|{ Orders: "размещает"
    Orders ||--|{ Payments: "включает"
    OrderStatuses ||--|{ Orders: "имеет статус"
    
    Customers {
        CustomerId INT PK
        FirstName VARCHAR(50)
        LastName VARCHAR(50)
        Address VARCHAR(255)
        PhoneNumber bigint
        Email VARCHAR(255)
    }

    Orders {
        OrderId INT PK
        CustomerId INT FK
        OrderDate DATE
        OrderTime TIME
        OrderStatus INT FK
    }
    
    OrderStatuses {
        StatusId INT PK
        StatusName VARCHAR(50)
    }

    Payments {
        PaymentId INT PK
        OrderId INT FK
        Total DECIMAL
        PaymentDate DATE
        PaymentTime TIME
    }

    Products {
        ProductId INT PK
        Name VARCHAR(255)
        Description TEXT
        Price DECIMAL
        Quantity INT
    }
    
    OrderedProducts {
        OrderId INT "PK FK"
        ProductId INT "PK FK"
        OrderedQuantity INT
    }
    
    Products ||--|{ OrderedProducts: ""
    Orders ||--|{ OrderedProducts: ""

```

Соответсвие названий таблиц русским словам

| Название таблицы | Название на русском |
|------------------|---------------------|
| Customers        | Заказчики           |
| Orders           | Заказы              |
| OrderStatuses    | Статусы заказа      |
| Payments         | Платежи             |
| OrderedProducts  | Заказанные товары   |
| Products         | Товары              |

