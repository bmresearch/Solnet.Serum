using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Serum.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Solnet.Serum.Test
{
    public class MarketManagerTestBase
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        private static readonly Queue<string> ResponseQueue;
        private static readonly Queue<string> AddressQueue;

        /// <summary>
        /// Setup the JSON RPC test with the request and response data.
        /// </summary>
        /// <param name="network">The network address for the <c>GetAccountInfo</c> request.</param>
        /// <returns>The mocked rpc client.</returns>
        private static Mock<IRpcClient> MockRpcClient(string network)
        {
            Mock<IRpcClient> rpcMock = new(MockBehavior.Strict);
            rpcMock
                .Setup(s => s.NodeAddress)
                .Returns(new Uri(network))
                .Verifiable();
            return rpcMock;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="address"></param>
        /// <param name="commitment"></param>
        private static void MockGetAccountInfoAsync(Mock<IRpcClient> rpcMock, string address,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock
                .Setup(s => s.GetAccountInfoAsync(
                    It.Is<string>(s1 => s1 == address),
                    It.Is<Commitment>(c => c == commitment)))
                .ReturnsAsync(new RequestResult<ResponseValue<AccountInfo>>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<ResponseValue<AccountInfo>>(ResponseQueue.Dequeue(),
                        JsonSerializerOptions)) { WasRequestSuccessfullyHandled = true })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="marketAddress"></param>
        /// <param name="ownerAddress"></param>
        /// <param name="commitment"></param>
        private static void MockGetProgramAccountsAsync(Mock<IRpcClient> rpcMock, string marketAddress,
            string ownerAddress,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock
                .Setup(s => s.GetProgramAccountsAsync(
                    It.Is<string>(s1 => s1 == AddressQueue.Dequeue()),
                    It.Is<Commitment>(c => c == commitment),
                    It.Is<int>(i => i == 3228),
                    It.Is<List<MemCmp>>(filters => filters == new List<MemCmp>()
                    {
                        new() { Offset = 13, Bytes = marketAddress }, new() { Offset = 45, Bytes = ownerAddress }
                    })))
                .ReturnsAsync(new RequestResult<List<AccountKeyPair>>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<List<AccountKeyPair>>(ResponseQueue.Dequeue(), JsonSerializerOptions))
                {
                    WasRequestSuccessfullyHandled = true
                })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="ownerAddress"></param>
        /// <param name="tokenMint"></param>
        /// <param name="commitment"></param>
        private static void MockGetTokenAccountsByOwnerAsync(Mock<IRpcClient> rpcMock, string ownerAddress,
            string tokenMint,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock
                .Setup(s => s.GetTokenAccountsByOwnerAsync(
                    It.Is<string>(s1 => s1 == ownerAddress),
                    It.Is<string>(s1 => s1 == tokenMint),
                    It.IsAny<string>(),
                    It.Is<Commitment>(c => c == commitment)))
                .ReturnsAsync(new RequestResult<ResponseValue<List<TokenAccount>>>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<ResponseValue<List<TokenAccount>>>(ResponseQueue.Dequeue(),
                        JsonSerializerOptions)) { WasRequestSuccessfullyHandled = true })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="ownerAddress"></param>
        /// <param name="tokenMint"></param>
        /// <param name="commitment"></param>
        private static void MockGetRecentBlockhashAsync(Mock<IRpcClient> rpcMock, string ownerAddress, string tokenMint,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock
                .Setup(s => s.GetRecentBlockHashAsync(
                    It.Is<Commitment>(c => c == commitment)))
                .ReturnsAsync(new RequestResult<ResponseValue<BlockHash>>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<ResponseValue<BlockHash>>(ResponseQueue.Dequeue(),
                        JsonSerializerOptions)) { WasRequestSuccessfullyHandled = true })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="accountSize"></param>
        /// <param name="commitment"></param>
        private static void MockGetMinimumBalanceForRentExemptionAsync(Mock<IRpcClient> rpcMock, long accountSize,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock
                .Setup(s => s.GetMinimumBalanceForRentExemptionAsync(
                    It.Is<long>(c => c == accountSize),
                    It.Is<Commitment>(c => c == commitment)))
                .ReturnsAsync(new RequestResult<ulong>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<ulong>(ResponseQueue.Dequeue(),
                        JsonSerializerOptions)) { WasRequestSuccessfullyHandled = true })
                .Verifiable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcMock"></param>
        /// <param name="transaction"></param>
        /// <param name="commitment"></param>
        private static void MockSendTransactionAsync(Mock<IRpcClient> rpcMock, byte[] transaction,
            Commitment commitment = Commitment.Finalized)
        {
            rpcMock
                .Setup(s => s.SendTransactionAsync(
                    It.Is<byte[]>(b => Convert.ToBase64String(transaction) == Convert.ToBase64String(b)),
                    It.Is<bool>(b => !b),
                    It.Is<Commitment>(c => c == commitment)))
                .ReturnsAsync(new RequestResult<string>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    JsonSerializer.Deserialize<string>(ResponseQueue.Dequeue(),
                        JsonSerializerOptions)) { WasRequestSuccessfullyHandled = true })
                .Verifiable();
        }

        /// <summary>
        /// Mocks a serum client used to perform requests pertaining to the given market address.
        /// </summary>
        /// <param name="rpcClient">The rpc client.</param>
        /// <param name="marketAddress">The market address.</param>
        /// <param name="marketAccountData">The market account data.</param>
        /// <returns>The mocked serum client.</returns>
        private static Mock<ISerumClient> MockSerumClient(IRpcClient rpcClient, string marketAddress,
            string marketAccountData)
        {
            Mock<ISerumClient> serumClientMock = new(MockBehavior.Strict);

            serumClientMock
                .Setup(x => x.RpcClient)
                .Returns(rpcClient);

            serumClientMock
                .Setup(x => x.GetMarketAsync(
                    It.Is<string>(s => s == marketAddress),
                    It.IsAny<Commitment>()))
                .ReturnsAsync(() => Market.Deserialize(Convert.FromBase64String(marketAccountData)));

            return serumClientMock;
        }
    }
}