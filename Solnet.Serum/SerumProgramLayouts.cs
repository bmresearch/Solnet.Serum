namespace Solnet.Serum
{
    /// <summary>
    /// Layouts associated with encoding and decoding of <see cref="SerumProgram"/> instructions.
    /// </summary>
    internal class SerumProgramLayouts
    {
        /// <summary>
        /// The offset at which to write the method value.
        /// </summary>
        internal const int MethodOffset = 1;

        /// <summary>
        /// The offset at which to write the limit value for the <see cref="SerumProgramInstructions.Values.ConsumeEvents"/> method.
        /// </summary>
        internal const int ConsumeEventsLimitOffset = 5;
        
        /// <summary>
        /// The offset at which to write the limit value for the <see cref="SerumProgramInstructions.Values.Prune"/> method.
        /// </summary>
        internal const int PruneLimitOffset = 5;

        /// <summary>
        /// The offset at which to write the client id value for the <see cref="SerumProgramInstructions.Values.CancelOrderByClientIdV2"/> method.
        /// </summary>
        internal const int CancelOrderByClientIdV2ClientIdOffset = 5;
        
        /// <summary>
        /// Represents the layout of the <see cref="SerumProgramInstructions.Values.CancelOrderV2"/> method encoded data structure.
        /// </summary>
        internal static class CancelOrderV2
        {
            /// <summary>
            /// The offset at which to write the order side value for the <see cref="SerumProgramInstructions.Values.CancelOrderV2"/> method.
            /// </summary>
            internal const int SideOffset = 5;

            /// <summary>
            /// The offset at which to write the order id value for the <see cref="SerumProgramInstructions.Values.CancelOrderV2"/> method.
            /// </summary>
            internal const int OrderIdOffset = 9;
        }
        
        /// <summary>
        /// Represents the layout of the <see cref="SerumProgramInstructions.Values.NewOrderV3"/> method encoded data structure.
        /// </summary>
        internal static class NewOrderV3
        {
            /// <summary>
            /// The offset at which to write the order side value.
            /// </summary>
            internal const int SideOffset = 5;

            /// <summary>
            /// The offset at which to write the limit price value.
            /// </summary>
            internal const int PriceOffset = 9;

            /// <summary>
            /// The offset at which to write the max base quantity value.
            /// </summary>
            internal const int MaxBaseQuantityOffset = 17;

            /// <summary>
            /// The offset at which to write the max quote quantity value.
            /// </summary>
            internal const int MaxQuoteQuantity = 25;

            /// <summary>
            /// The offset at which to write the self trade behavior value.
            /// </summary>
            internal const int SelfTradeBehaviorOffset = 33;

            /// <summary>
            /// The offset at which to write the order type value.
            /// </summary>
            internal const int OrderTypeOffset = 37;

            /// <summary>
            /// The offset at which to write the client id value.
            /// </summary>
            internal const int ClientIdOffset = 41;

            /// <summary>
            /// The offset at which to write the limit value.
            /// </summary>
            internal const int LimitOffset = 49;
        }
    }
}