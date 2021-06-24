namespace Solnet.Serum
{
    /// <summary>
    /// Represents the instruction types for the <see cref="SerumProgram"/>.
    /// </summary>
    public enum SerumProgramInstructions : byte
    {
        /// <summary>
        /// 
        /// </summary>
        InitializeMarket = 0,
        
        /// <summary>
        /// 
        /// </summary>
        NewOrder = 1,
        
        /// <summary>
        /// 
        /// </summary>
        MatchOrder = 2,
        
        /// <summary>
        /// 
        /// </summary>
        ConsumeEvents = 3,
        
        /// <summary>
        /// 
        /// </summary>
        CancelOrder = 4,
        
        /// <summary>
        /// 
        /// </summary>
        SettleFunds = 5,
        
        /// <summary>
        /// 
        /// </summary>
        CancelOrderByClientId = 6,
        
        /// <summary>
        /// 
        /// </summary>
        NewOrderV3 = 10,
        
        /// <summary>
        /// 
        /// </summary>
        CancelOrderV2 = 11,
        
        /// <summary>
        /// 
        /// </summary>
        CancelOrderByClientIdV2 = 12,
    }
}