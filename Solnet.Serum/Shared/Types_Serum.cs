using System.Diagnostics;

namespace Solnet.Serum.Shared {

//==========================================================
// Types Specific to Serum (Enumerations)
//==========================================================

public enum OrderType
{
    Limit             = 0,  // A limit order.
    ImmediateOrCancel = 1,  // An order which is immediately filled or cancelled.
    PostOnly          = 2,  // The order is a post only order.
}


// Represents the side of an order.
public enum SideLayout
{
    Buy  = 0,  // The order is a buy order.
    Sell = 1,  // The order is a sell order.
}

//==========================================================
// Types Specific to Serum (Other)
//==========================================================
public class OrderId
{
    public const int Length = 16;
    public byte[] Bytes;
    public OrderId(byte[] bytes)
    {
        Debug.Assert(bytes.Length == Length);
        Bytes = bytes;
    }
}

} // Namespaces