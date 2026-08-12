namespace AutoPartsHub.Core;

public enum UserRole
{
    Customer = 1,
    Admin = 2
}

public enum ProductCondition
{
    New = 1,
    Used = 2,
    Refurbished = 3
}

public enum OrderStatus
{
    PendingPayment = 1,
    Paid = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6
}

public enum DeliveryMethod
{
    Pickup = 1,
    Courier = 2,
    TransportCompany = 3
}

public enum PaymentMethod
{
    CardOnline = 1,
    CashOnDelivery = 2
}

public enum SubscriptionType
{
    BackInStock = 1,
    PriceDrop = 2
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3
}
