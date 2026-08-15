namespace AutoPartsHub.Models;

/// <summary>Определяет роль пользователя в системе.</summary>
public enum UserRole
{
    /// <summary>Покупатель, использующий каталог и оформление заказов.</summary>
    Customer = 1,

    /// <summary>Администратор каталога и заказов.</summary>
    Admin = 2
}

/// <summary>Определяет состояние продаваемой запчасти.</summary>
public enum ProductCondition
{
    /// <summary>Новый товар.</summary>
    New = 1,

    /// <summary>Товар, бывший в употреблении.</summary>
    Used = 2,

    /// <summary>Восстановленный товар.</summary>
    Refurbished = 3
}

/// <summary>Определяет этап обработки заказа.</summary>
public enum OrderStatus
{
    /// <summary>Заказ ожидает онлайн-оплаты.</summary>
    PendingPayment = 1,

    /// <summary>Заказ оплачен.</summary>
    Paid = 2,

    /// <summary>Заказ обрабатывается.</summary>
    Processing = 3,

    /// <summary>Заказ передан в доставку.</summary>
    Shipped = 4,

    /// <summary>Заказ доставлен покупателю.</summary>
    Delivered = 5,

    /// <summary>Заказ отменён.</summary>
    Cancelled = 6
}

/// <summary>Определяет способ получения заказа.</summary>
public enum DeliveryMethod
{
    /// <summary>Самовывоз.</summary>
    Pickup = 1,

    /// <summary>Курьерская доставка.</summary>
    Courier = 2,

    /// <summary>Доставка транспортной компанией.</summary>
    TransportCompany = 3
}

/// <summary>Определяет способ оплаты заказа.</summary>
public enum PaymentMethod
{
    /// <summary>Онлайн-оплата банковской картой.</summary>
    CardOnline = 1,

    /// <summary>Оплата при получении.</summary>
    CashOnDelivery = 2
}

/// <summary>Определяет условие товарной подписки.</summary>
public enum SubscriptionType
{
    /// <summary>Уведомление о появлении товара в наличии.</summary>
    BackInStock = 1,

    /// <summary>Уведомление о снижении цены до заданного значения.</summary>
    PriceDrop = 2
}

/// <summary>Определяет результат отправки уведомления.</summary>
public enum NotificationStatus
{
    /// <summary>Уведомление ожидает отправки.</summary>
    Pending = 1,

    /// <summary>Уведомление успешно отправлено.</summary>
    Sent = 2,

    /// <summary>Отправка уведомления завершилась ошибкой.</summary>
    Failed = 3
}
