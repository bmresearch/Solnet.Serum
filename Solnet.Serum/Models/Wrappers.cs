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
        /// The result of the signature confirmation.
        /// </summary>
        public ResponseValue<ErrorResult> ConfirmationResult;

        /// <summary>
        /// Event fired when the signature is confirmed by the cluster.
        /// </summary>
        public event EventHandler<SignatureConfirmationStatus> ConfirmationChanged;

        /// <summary>
        /// Changes the state of the given signature confirmation according to the cluster's notification.
        /// </summary>
        /// <param name="subscriptionState">The newest subscription state.</param>
        /// <param name="confirmationResult"></param>
        internal void ChangeState(SubscriptionState subscriptionState, ResponseValue<ErrorResult> confirmationResult)
        {
            Subscription = subscriptionState;
            ConfirmationResult = confirmationResult;
            ConfirmationChanged?.Invoke(this, new SignatureConfirmationStatus(confirmationResult.Value));
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
        /// Initializes the signature confirmation status with the given <see cref="ErrorResult"/>.
        /// </summary>
        /// <param name="errorResult">The error result that may have occurred processing the transaction.</param>
        internal SignatureConfirmationStatus(ErrorResult errorResult = default)
        {
            if (errorResult?.Error?.InstructionError == null)
                return;

            if (errorResult.Error?.InstructionError?.CustomError.HasValue == false)
                return;

            SerumProgramError serumError =
                (SerumProgramError)Enum.Parse(typeof(SerumProgramError),
                    errorResult.Error?.InstructionError?.CustomError.ToString());

            Error = serumError;
            TransactionError = errorResult.Error;
            InstructionError = errorResult.Error?.InstructionError;
        }
    }
}