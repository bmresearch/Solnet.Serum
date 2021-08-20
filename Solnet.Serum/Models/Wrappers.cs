using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Core.Sockets;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using System;

namespace Solnet.Serum.Models
{
    /// <summary>
    /// Wraps a subscription with a generic type to hold either order book or trade events.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// The address associated with this data.
        /// </summary>
        public PublicKey Address;

        /// <summary>
        /// The underlying subscription state.
        /// </summary>
        public SubscriptionState SubscriptionState;
    }

    /// <summary>
    /// Wraps the base subscription to have the underlying data of the subscription, which is sometimes needed to perform
    /// some logic before returning data to the subscription caller.
    /// </summary>
    /// <typeparam name="T">The type of the subscription.</typeparam>
    internal class SubscriptionWrapper<T> : Subscription
    {
        /// <summary>
        /// The underlying data.
        /// </summary>
        public T Data;

        public bool TestData;
    }

    /// <summary>
    /// Wraps the base request result and subscription state, provides an event handler to be notified when the given signature
    /// is <see cref="Commitment.Confirmed"/>, with additional information in the context of the transaction and the execution of it's instructions.
    /// </summary>
    public class SignatureConfirmation
    {
        /// <summary>
        /// The transaction signature.
        /// </summary>
        public string Signature;

        /// <summary>
        /// The state of the subscription to the 
        /// </summary>
        public SubscriptionState Subscription;

        /// <summary>
        /// The request result associated with submitting the transaction to the cluster.
        /// </summary>
        public RequestResult<string> Result;
        
        /// <summary>
        /// The confirmation result associated with submitting and subscribing to the transaction's confirmation.
        /// </summary>
        public ResponseValue<ErrorResult> ConfirmationResult;
        
        /// <summary>
        /// The transaction error, if occurred.
        /// </summary>
        public TransactionError TransactionError;

        /// <summary>
        /// The instruction error, if occurred.
        /// </summary>
        public InstructionError InstructionError;

        /// <summary>
        /// The error which may have occurred during request submission and instruction execution.
        /// </summary>
        public SerumProgramError Error;

        /// <summary>
        /// The result of the transaction simulation.
        /// </summary>
        public SimulationLogs SimulationLogs;
        
        /// <summary>
        /// Event fired when the signature is confirmed by the cluster.
        /// </summary>
        public event EventHandler<SignatureConfirmationStatus> ConfirmationChanged;

        /// <summary>
        /// Changes the state of the given signature confirmation according to the cluster's notification.
        /// </summary>
        /// <param name="subscriptionState">The newest subscription state.</param>
        /// <param name="confirmationResult">The transaction's confirmation result.</param>
        public void ChangeState(SubscriptionState subscriptionState, ResponseValue<ErrorResult> confirmationResult)
        {
            Subscription = subscriptionState;
            ParseErrorAndInvoke(confirmationResult.Value.Error);
        }

        /// <summary>
        /// Changes the state of the given signature confirmation according to the cluster's notification.
        /// </summary>
        /// <param name="simLogs">The transaction's simulation logs.</param>
        public void ChangeState(SimulationLogs simLogs)
        {
            SimulationLogs = simLogs;
            ParseErrorAndInvoke(simLogs.Error);
        }

        /// <summary>
        /// Parse existing errors into the the <see cref="SerumProgramError"/> structure and invoke the event.
        /// This is done for cases where the error occurs before the user has a chance to add a listener to the event.
        /// </summary>
        private void ParseErrorAndInvoke(TransactionError errorResult = default)
        {
            if (errorResult == null)
                return;

            if (errorResult.InstructionError?.CustomError.HasValue == false)
                return;

            SerumProgramError serumError =
                (SerumProgramError)Enum.Parse(typeof(SerumProgramError),
                    errorResult.InstructionError?.CustomError.ToString() ?? string.Empty);
            
            Error = serumError;
            TransactionError = errorResult;
            InstructionError = errorResult?.InstructionError;
            ConfirmationChanged?.Invoke(this, new SignatureConfirmationStatus(errorResult, serumError));
        }
    }

    /// <summary>
    /// Represents the signature confirmation status, parsing any errors, if occurred, into the <see cref="SerumProgramError"/> structure.
    /// </summary>
    public class SignatureConfirmationStatus : EventArgs
    {
        /// <summary>
        /// The transaction error, if occurred.
        /// </summary>
        public readonly TransactionError TransactionError;

        /// <summary>
        /// The instruction error, if occurred.
        /// </summary>
        public readonly InstructionError InstructionError;

        /// <summary>
        /// The error which may have occurred during request submission and instruction execution.
        /// </summary>
        public SerumProgramError? Error;

        /// <summary>
        /// Initializes the signature confirmation status with the given <see cref="ErrorResult"/>.
        /// </summary>
        /// <param name="errorResult">The error result that may have occurred processing the transaction.</param>
        /// <param name="serumError">The serum error that may have occurred.</param>
        internal SignatureConfirmationStatus(TransactionError errorResult = default, SerumProgramError serumError = default)
        {
            Error = serumError;
            TransactionError = errorResult;
            InstructionError = errorResult?.InstructionError;
        }
    }
}