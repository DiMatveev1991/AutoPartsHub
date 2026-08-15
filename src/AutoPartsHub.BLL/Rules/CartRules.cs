using AutoPartsHub.Models;

namespace AutoPartsHub.BLL.Rules;

/// <summary>
/// Выполняет изменения состава пользовательской корзины.
/// </summary>
internal static class CartRules
{
    /// <summary>Создаёт пустую корзину пользователя.</summary>
    internal static Cart Create(Guid userId, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Пользователь обязателен.");

        return new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UpdatedAt = now
        };
    }

    /// <summary>Добавляет новую позицию или объединяет её с существующей.</summary>
    internal static void Add(Cart cart, Product product, int quantity, DateTimeOffset now)
    {
        if (quantity <= 0)
            throw new DomainException("Количество должно быть больше нуля.");
        if (!product.IsActive || product.Stock < quantity)
            throw new DomainException("Товар недоступен в указанном количестве.");

        var existing = cart.Items.SingleOrDefault(item => item.ProductId == product.Id);
        var newQuantity = (existing?.Quantity ?? 0) + quantity;
        if (newQuantity > product.Stock)
            throw new DomainException("Количество в корзине превышает остаток.");

        if (existing is null)
        {
            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Product = product,
                Quantity = quantity
            });
        }
        else
        {
            existing.Quantity = newQuantity;
        }

        cart.UpdatedAt = now;
    }

    /// <summary>Заменяет количество существующей позиции с учётом остатка.</summary>
    internal static void ChangeQuantity(
        Cart cart,
        Guid productId,
        int quantity,
        int availableStock,
        DateTimeOffset now)
    {
        var item = cart.Items.SingleOrDefault(value => value.ProductId == productId)
            ?? throw new DomainException("Товар отсутствует в корзине.");

        if (quantity <= 0 || quantity > availableStock)
        {
            throw new DomainException(
                "Количество должно быть положительным и не превышать остаток.");
        }

        item.Quantity = quantity;
        cart.UpdatedAt = now;
    }

    /// <summary>Удаляет позицию, если она присутствует в корзине.</summary>
    internal static void Remove(Cart cart, Guid productId, DateTimeOffset now)
    {
        var item = cart.Items.SingleOrDefault(value => value.ProductId == productId);
        if (item is not null)
            cart.Items.Remove(item);
        cart.UpdatedAt = now;
    }

    /// <summary>Очищает корзину после успешного формирования заказа.</summary>
    internal static void Clear(Cart cart, DateTimeOffset now)
    {
        cart.Items.Clear();
        cart.UpdatedAt = now;
    }
}
