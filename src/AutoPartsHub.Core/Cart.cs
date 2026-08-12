namespace AutoPartsHub.Core;

public sealed class Cart
{
    private readonly List<CartItem> _items = [];

    private Cart()
    {
    }

    public Cart(Guid userId, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Пользователь обязателен.");

        Id = Guid.NewGuid();
        UserId = userId;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public User? User { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items;

    public void Add(Product product, int quantity, DateTimeOffset now)
    {
        if (quantity <= 0)
            throw new DomainException("Количество должно быть больше нуля.");
        if (!product.IsActive || product.Stock < quantity)
            throw new DomainException("Товар недоступен в указанном количестве.");

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

    public void ChangeQuantity(Guid productId, int quantity, int availableStock, DateTimeOffset now)
    {
        var item = _items.SingleOrDefault(value => value.ProductId == productId)
            ?? throw new DomainException("Товар отсутствует в корзине.");

        if (quantity <= 0 || quantity > availableStock)
            throw new DomainException("Количество должно быть положительным и не превышать остаток.");

        item.ChangeQuantity(quantity);
        UpdatedAt = now;
    }

    public void Remove(Guid productId, DateTimeOffset now)
    {
        var item = _items.SingleOrDefault(value => value.ProductId == productId);
        if (item is not null)
            _items.Remove(item);
        UpdatedAt = now;
    }

    public void Clear(DateTimeOffset now)
    {
        _items.Clear();
        UpdatedAt = now;
    }
}
