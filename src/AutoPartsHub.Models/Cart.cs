namespace AutoPartsHub.Models;

/// <summary>
/// Представляет пользовательскую корзину и управляет её позициями.
/// </summary>
/// <remarks>
/// Корзина связана с пользователем один-к-одному и содержит много
/// <see cref="CartItem"/> по связи один-ко-многим.
/// </remarks>
public sealed class Cart
{
    private readonly List<CartItem> _items = [];

    /// <summary>
    /// Создаёт экземпляр корзины для восстановления Entity Framework Core.
    /// </summary>
    private Cart()
    {
    }

    /// <summary>
    /// Создаёт пустую корзину для указанного пользователя.
    /// </summary>
    public Cart(Guid userId, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Пользователь обязателен.");

        Id = Guid.NewGuid();
        UserId = userId;
        UpdatedAt = now;
    }

    /// <summary>Получает уникальный идентификатор корзины.</summary>
    public Guid Id { get; private set; }

    /// <summary>Получает уникальный внешний ключ пользователя в связи один-к-одному.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Получает дату и время последнего изменения корзины.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Получает пользователя по связи один-к-одному.</summary>
    public User? User { get; private set; }

    /// <summary>Получает сторону «многие» связи один-ко-многим с позициями корзины.</summary>
    public IReadOnlyCollection<CartItem> Items => _items;

    /// <summary>
    /// Добавляет товар в корзину или увеличивает количество существующей позиции.
    /// </summary>
    public void Add(Product product, int quantity, DateTimeOffset now)
    {
        if (quantity <= 0)
            throw new DomainException("Количество должно быть больше нуля.");
        if (!product.IsActive || product.Stock < quantity)
            throw new DomainException("Товар недоступен в указанном количестве.");

        // Одинаковые товары хранятся одной строкой, поэтому количество объединяется.
        var existing = _items.SingleOrDefault(item => item.ProductId == product.Id);
        var newQuantity = (existing?.Quantity ?? 0) + quantity;
        if (newQuantity > product.Stock)
            throw new DomainException("Количество в корзине превышает остаток.");

        if (existing is null)
            _items.Add(new CartItem(Id, product.Id, quantity));
        else
            existing.ChangeQuantity(newQuantity);

        UpdatedAt = now;
    }

    /// <summary>
    /// Изменяет количество товара с учётом доступного остатка.
    /// </summary>
    public void ChangeQuantity(Guid productId, int quantity, int availableStock, DateTimeOffset now)
    {
        var item = _items.SingleOrDefault(value => value.ProductId == productId)
            ?? throw new DomainException("Товар отсутствует в корзине.");

        if (quantity <= 0 || quantity > availableStock)
            throw new DomainException("Количество должно быть положительным и не превышать остаток.");

        item.ChangeQuantity(quantity);
        UpdatedAt = now;
    }

    /// <summary>
    /// Удаляет товар из корзины, если он был добавлен.
    /// </summary>
    public void Remove(Guid productId, DateTimeOffset now)
    {
        var item = _items.SingleOrDefault(value => value.ProductId == productId);
        if (item is not null)
            _items.Remove(item);
        UpdatedAt = now;
    }

    /// <summary>
    /// Удаляет все позиции корзины.
    /// </summary>
    public void Clear(DateTimeOffset now)
    {
        _items.Clear();
        UpdatedAt = now;
    }
}
