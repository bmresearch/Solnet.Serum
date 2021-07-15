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
        ConsumeEvents = 3,
        
        /// <summary>
        /// 
        /// </summary>
        SettleFunds = 5,
        
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