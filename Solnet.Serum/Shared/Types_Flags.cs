
namespace Solnet.Serum.Shared {

//======================================================================
// Event Flags
//======================================================================
public class EventFlags : BitsU8
{
    public EventFlags(byte bits) : base(bits) { }
    public static implicit operator EventFlags(byte bits) => new(bits);

    public bool Fill  => Bit0;   // Is this a fill?
    public bool Out   => Bit1;   // Is this an output?
    public bool Bid   => Bit2;   // Is this a bid?
    public bool Maker => Bit3;   // Whether the event is a maker or not
}

//========================================================
// Account Flags
//---------------
// Depending on the bits set here, an account can be
// a 'Market', an 'OpenOrdersAccount', an 'EventQueue',
// a 'RequestQueue'  or an order book's Bids/Asks account
//========================================================
public class AccountFlags : BitsU64
{
    public AccountFlags(ulong bits) : base(bits) { }
    public static implicit operator AccountFlags(ulong bits) => new(bits);

    public bool Initialized       => Bit(0); // Has this account been initialized?
    public bool Type_Market       => Bit(1); // Is the type of this account 'Market'?
    public bool Type_OpenOrders   => Bit(2); // Is the type of this account 'OpenOrders'?
    public bool Type_RequestQueue => Bit(3); // Is the type of this account 'RequestQueue'?
    public bool Type_EventQueue   => Bit(4); // Is the type of this account 'EventQueue'?
    public bool Type_Bids         => Bit(5); // Is the type of this account 'Bids'?
    public bool Type_Asks         => Bit(6); // Is the type of this account 'Asks'?
}

} //Namespaces