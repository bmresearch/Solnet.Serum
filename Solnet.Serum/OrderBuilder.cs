using Solnet.Serum.Models;

namespace Solnet.Serum
{
    /// <summary>
    /// Implements a builder class and pattern around the <see cref="Order"/>.
    /// </summary>
    public class OrderBuilder
    {
        /// <summary>
        /// The order type.
        /// </summary>
        private OrderType _orderType;

        /// <summary>
        /// The self trade behavior for the order.
        /// </summary>
        private SelfTradeBehavior _selfTradeBehavior;

        /// <summary>
        /// The side for the order.
        /// </summary>
        private Side _side;

        /// <summary>
        /// The size of the order.
        /// </summary>
        private float _quantity;

        /// <summary>
        /// The price for the order.
        /// </summary>
        private float _price;

        /// <summary>
        /// The client's id for the order.
        /// </summary>
        private ulong _clientOrderId;
        
        /// <summary>
        /// Initialize the order builder.
        /// </summary>
        public OrderBuilder() { }

        /// <summary>
        /// Set the order's self trade behavior.
        /// <remarks>See <see cref="SelfTradeBehavior"/> for more information about an order's self trade behavior.</remarks>
        /// </summary>
        /// <param name="behavior">The desired self trade behavior.</param>
        /// <returns>The <see cref="OrderBuilder"/> instance.</returns>
        public OrderBuilder SetSelfTradeBehavior(SelfTradeBehavior behavior)
        {
            _selfTradeBehavior = behavior;
            return this;
        }
        
        /// <summary>
        /// Set the order side.
        /// <remarks>See <see cref="Side"/> for more information about order sides.</remarks>
        /// </summary>
        /// <param name="side">The desired side.</param>
        /// <returns>The <see cref="OrderBuilder"/> instance.</returns>
        public OrderBuilder SetSide(Side side)
        {
            _side = side;
            return this;
        }
        
        /// <summary>
        /// Set the order type.
        /// <remarks>See <see cref="OrderType"/> for more information about order types.</remarks>
        /// </summary>
        /// <param name="type">The desired order type.</param>
        /// <returns>The <see cref="OrderBuilder"/> instance.</returns>
        public OrderBuilder SetOrderType(OrderType type)
        {
            _orderType = type;
            return this;
        }

        /// <summary>
        /// Set the size for the order.
        /// </summary>
        /// <param name="quantity">The desired size.</param>
        /// <returns>The <see cref="OrderBuilder"/> instance.</returns>
        public OrderBuilder SetQuantity(float quantity)
        {
            _quantity = quantity;
            return this;
        }
        
        /// <summary>
        /// Set the client's order id.
        /// </summary>
        /// <param name="id">The desired id.</param>
        /// <returns>The <see cref="OrderBuilder"/> instance.</returns>
        public OrderBuilder SetClientOrderId(ulong id)
        {
            _clientOrderId = id;
            return this;
        }
        
        /// <summary>
        /// Set the price for the order.
        /// </summary>
        /// <param name="price">The desired price.</param>
        /// <returns>The <see cref="OrderBuilder"/> instance.</returns>
        public OrderBuilder SetPrice(float price)
        {
            _price = price;
            return this;
        }
        
        /// <summary>
        /// Build the order.
        /// </summary>
        /// <returns>The built <see cref="Order"/>.</returns>
        public Order Build()
        {
            return new ()
            {
                SelfTradeBehavior = _selfTradeBehavior,
                Side = _side,
                Type = _orderType,
                Price = _price,
                Quantity = _quantity,
                ClientId = _clientOrderId,
            };
        }
    }
}