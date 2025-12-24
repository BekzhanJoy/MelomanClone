using System.Text.Json;

namespace MelomanClone.Models
{
    public class Cart
    {
        private readonly ISession _session;
        private const string CartKey = "cart";

        public Cart(IHttpContextAccessor accessor)
        {
            _session = accessor.HttpContext.Session;
        }

        public List<CartItem> Items
        {
            get
            {
                var data = _session.GetString(CartKey);
                return data == null
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(data);
            }
        }

        private void Save(List<CartItem> items)
        {
            _session.SetString(CartKey, JsonSerializer.Serialize(items));
        }

        public void Add(CartItem item)
        {
            var items = Items;
            var existing = items.FirstOrDefault(x => x.ProductId == item.ProductId);

            if (existing != null)
                existing.Quantity++;
            else
                items.Add(item);

            Save(items);
        }

        public void Remove(int productId)
        {
            var items = Items;
            var item = items.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                items.Remove(item);
                Save(items);
            }
        }

        public decimal Total()
        {
            return Items.Sum(x => x.Price * x.Quantity);
        }

        public void Clear()
        {
            _session.Remove(CartKey);
        }
    }
}
