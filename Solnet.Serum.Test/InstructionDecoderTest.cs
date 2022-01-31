using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solnet.Programs;
using Solnet.Rpc.Models;
using Solnet.Serum.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Solnet.Serum.Test
{
    [TestClass]
    public class InstructionDecoderTest
    {
        private static readonly string CancelOrdersAndSettleMsg = 
            "AQADDQpz5x/t0hNl7QruhPzk4rIGR/001ey9oRXwI9JjP4d4MZCYjBIPabdFGOFRg4aCuYIvpX0/Up2H0gsFDOvAKxptYTUM2" +
            "1S5Es4XParbLdSX6tunfXUP9th1OPASGbtY2PiI2PpWo39ogKOkNY5k/Nfr0Wt/9KEDGrcUmrxuhRiSbi3T/D9J3OFx/jgQJR" +
            "Q+s2+9GBNxoSwkDUIUrgnJSVYCOaxQQvf9r8PyaRmnlkSN6BQs0u7SDcCOz4CLebCdaSgViGOph6mtTc/i5/V7k/Malr7/YxBe" +
            "zcjPsCTq/LNI8RqttgK/juTyfqv8uSiqbkGWGETaSk2Kb5xsFcNlQQWSbGlLqfiZ41yfAAH/BysfePaWsZCjnts5xkiSRTT14" +
            "A0x4LQTafOL58gL65FcTUQQaY0+tE9OQfrFVtgS5kpxhQ8tbgKkevgk0Jq2ncQtcMsoy/okn7fuV7nSVsEnYu+pgdiCRuMX9X" +
            "DIP87eCqZqkDn14UiDO4FzWxHK14PVPQbd9uHXZaGT2cvhRs7reawctIXtX1s3kTqM9YV+/wCpRT/gXmftFbCh8k/tkXlqLbwa" +
            "AlSoCOY59nR43pTxwWwFCgYBAgMEAAUZAAsAAAAAAAAANC/f//////89DAAAAAAAAAoGAQIDBAAFGQALAAAAAAAAADMv3/////" +
            "//HgwAAAAAAAAKBgECAwQABRkACwAAAAAAAAAyL9////////4LAAAAAAAACgYBAgMEAAUZAAsAAAAAAAAAMS/f///////eCwAA" +
            "AAAAAAoJAQQABgcICQsMBQAFAAAA";

        private static readonly string CreateOpenOrdersNewOrdersAndSettleMsg =
            "AgAFEApz5x/t0hNl7QruhPzk4rIGR/001ey9oRXwI9JjP4d4bi3T/D9J3OFx/jgQJRQ+s2+9GBNxoSwkDUIUrgnJSVYxkJiME" +
            "g9pt0UY4VGDhoK5gi+lfT9SnYfSCwUM68ArGpr33IM/qQm6+EFjv3WCl9+nICT59ibJWro20VS5uUQ9AjmsUEL3/a/D8mkZp5" +
            "ZEjegULNLu0g3Ajs+Ai3mwnWltYTUM21S5Es4XParbLdSX6tunfXUP9th1OPASGbtY2PiI2PpWo39ogKOkNY5k/Nfr0Wt/9KE" +
            "DGrcUmrxuhRiSDTHgtBNp84vnyAvrkVxNRBBpjT60T05B+sVW2BLmSnEoFYhjqYeprU3P4uf1e5PzGpa+/2MQXs3Iz7Ak6vyz" +
            "SPEarbYCv47k8n6r/Lkoqm5BlhhE2kpNim+cbBXDZUEFkmxpS6n4meNcnwAB/wcrH3j2lrGQo57bOcZIkkU09eAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAan1RcZLFxRIYzJTD1K8X9Y2u4Im6H9ROPb2YoAAAAAhQ8tbgKkevgk0Jq2ncQtcM" +
            "soy/okn7fuV7nSVsEnYu8G3fbh12Whk9nL4UbO63msHLSF7V9bN5E6jPWFfv8AqamB2IJG4xf1cMg/zt4KpmqQOfXhSIM7gXN" +
            "bEcrXg9U9sKLMetn7/1UUmSgiOvObf6ncpgx9byhg2wueQRNGOvYICwIAATQAAAAAQGlkAQAAAACcDAAAAAAAAIUPLW4CpHr4" +
            "JNCatp3ELXDLKMv6JJ+37le50lbBJ2LvDQQBAAIMBQAPAAAADQwCAQMEBQYHAAgJDgwzAAoAAAAAAAAAXQwAAAAAAAAKAAAAA" +
            "AAAAEhLMAAAAAAAAQAAAAAAAAABAAAAAAAAAP//DQwCAQMEBQYHAAgJDgwzAAoAAAAAAAAAPQwAAAAAAAAWAAAAAAAAADgsaQ" +
            "AAAAAAAQAAAAAAAAABAAAAAAAAAP//DQwCAQMEBQYHAAgJDgwzAAoAAAAAAAAAHgwAAAAAAAAjAAAAAAAAACiqpQAAAAAAAQA" +
            "AAAAAAAABAAAAAAAAAP//DQwCAQMEBQYHAAgJDgwzAAoAAAAAAAAA/gsAAAAAAAAvAAAAAAAAAEgr3AAAAAAAAQAAAAAAAAAB" +
            "AAAAAAAAAP//DQwCAQMEBQYHAAgJDgwzAAoAAAAAAAAA3gsAAAAAAAA8AAAAAAAAACAjFgEAAAAAAQAAAAAAAAABAAAAAAAAA" +
            "P//DQkCAQAICQoHDw4FAAUAAAA=";

        private static readonly string CloseOpenOrdersMsg = 
            "AQACBApz5x/t0hNl7QruhPzk4rIGR/001ey9oRXwI9JjP4d4bi3T/D9J3OFx/jgQJRQ+s2+9GBNxoSwkDUIUrgnJSVYxkJiME" +
            "g9pt0UY4VGDhoK5gi+lfT9SnYfSCwUM68ArGoUPLW4CpHr4JNCatp3ELXDLKMv6JJ+37le50lbBJ2LvRT/gXmftFbCh8k/tkX" +
            "lqLbwaAlSoCOY59nR43pTxwWwBAwQBAAACBQAOAAAA";

        private static readonly string InitOpenOrdersAuthorityMsg =
            "AgEDBgpz5x/t0hNl7QruhPzk4rIGR/001ey9oRXwI9JjP4d43BTiskuLijN0Im9GJbaYltfYM7EWlK4B1p3CrtS9LytnTR0fu" +
            "hJxv9yK2x3vBF9FKqGptmtvvRKj5Z2Yt80B+jGQmIwSD2m3RRjhUYOGgrmCL6V9P1Kdh9ILBQzrwCsaBqfVFxksXFEhjMlMPU" +
            "rxf1ja7gibof1E49vZigAAAACFDy1uAqR6+CTQmradxC1wyyjL+iSft+5XudJWwSdi7zCsqcBb8R316Oq2E/s6bacxNqaZZYv" +
            "YBqQrYp2V4ZVJAQUFAgADBAEFAA8AAAA=";

        private static readonly string CancelByClientIdAndConsumeEventsMsg =
            "AgABCQpz5x/t0hNl7QruhPzk4rIGR/001ey9oRXwI9JjP4d4AN+Ypnsy9CYUTPWS+d6mQ62AG7sIO9zzX4rvqJX0TU0xkJiME" +
            "g9pt0UY4VGDhoK5gi+lfT9SnYfSCwUM68ArGm1hNQzbVLkSzhc9qtst1Jfq26d9dQ/22HU48BIZu1jY+IjY+lajf2iAo6Q1jm" +
            "T81+vRa3/0oQMatxSavG6FGJICOaxQQvf9r8PyaRmnlkSN6BQs0u7SDcCOz4CLebCdaZJsaUup+JnjXJ8AAf8HKx949paxkKO" +
            "e2znGSJJFNPXgDTHgtBNp84vnyAvrkVxNRBBpjT60T05B+sVW2BLmSnGFDy1uAqR6+CTQmradxC1wyyjL+iSft+5XudJWwSdi" +
            "7zXpjGB3tXakV2utaBIsepEZdxt2dSrG41gLDbgqbkt4AggGAgMEAQAFDQAMAAAAoIYBAAAAAAAIBQECBQYHBwADAAAA//8=";

        private static readonly string PruneMsg =
            "AgECCApz5x/t0hNl7QruhPzk4rIGR/001ey9oRXwI9JjP4d4uHiJfn+GplTj4talKGCSg9UtU/dYC8VBFhP5wi/VDboxkJiMEg" +
            "9pt0UY4VGDhoK5gi+lfT9SnYfSCwUM68ArGm1hNQzbVLkSzhc9qtst1Jfq26d9dQ/22HU48BIZu1jY+IjY+lajf2iAo6Q1jmT8" +
            "1+vRa3/0oQMatxSavG6FGJICOaxQQvf9r8PyaRmnlkSN6BQs0u7SDcCOz4CLebCdaQZjqIBBstRlvGKXFTn/swMxl19ZWB2KVy" +
            "TAWVJDJv12hQ8tbgKkevgk0Jq2ncQtcMsoy/okn7fuV7nSVsEnYu9gYhiIP+KUcsxGrD3ryAixnmdYImbKDZVmwFIINIPFJgEH" +
            "BwIDBAEGAAUHABAAAAD//w==";
        
        [ClassInitialize]
        public static void Setup(TestContext tc)
        {
            InstructionDecoder.Register(SerumProgram.MainNetProgramIdKeyV3, SerumProgram.Decode);
        }
        
        [TestMethod]
        public void DecodeCloseOpenOrdersTest()
        {
            Message msg = Message.Deserialize(Convert.FromBase64String(CloseOpenOrdersMsg));
            List<DecodedInstruction> ix =
                InstructionDecoder.DecodeInstructions(msg);

            Assert.AreEqual(1, ix.Count);
            Assert.AreEqual("Close Open Orders", ix[0].InstructionName);
            Assert.AreEqual("Serum Program", ix[0].ProgramName);
            Assert.AreEqual("9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin", ix[0].PublicKey);
            Assert.AreEqual(0, ix[0].InnerInstructions.Count);
            Assert.IsTrue(ix[0].Values.TryGetValue("Open Orders Account", out object openOrders));
            Assert.IsTrue(ix[0].Values.TryGetValue("Owner", out object owner));
            Assert.IsTrue(ix[0].Values.TryGetValue("Destination", out object destination));
            Assert.IsTrue(ix[0].Values.TryGetValue("Market", out object market));
            Assert.AreEqual("8R6NtLSyVpgG6f8Z9cLKFcuaJtt3mqHypKGsmRixwyJV", (PublicKey)openOrders);
            Assert.AreEqual("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh", (PublicKey)owner);
            Assert.AreEqual("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh", (PublicKey)destination);
            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", (PublicKey)market);
        }
        
        [TestMethod]
        public void DecodeInitializeOpenOrdersSubmitOrdersAndSettleFundsTest()
        {
            Message msg = Message.Deserialize(Convert.FromBase64String(CreateOpenOrdersNewOrdersAndSettleMsg));
            List<DecodedInstruction> ix = InstructionDecoder.DecodeInstructions(msg);

            Assert.AreEqual(8, ix.Count);
            Assert.AreEqual("Init Open Orders", ix[1].InstructionName);
            Assert.AreEqual("Serum Program", ix[1].ProgramName);
            Assert.AreEqual("9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin", ix[1].PublicKey);
            Assert.AreEqual(0, ix[1].InnerInstructions.Count);
            Assert.IsTrue(ix[1].Values.TryGetValue("Open Orders Account", out object openOrders));
            Assert.IsTrue(ix[1].Values.TryGetValue("Owner", out object owner));
            Assert.IsTrue(ix[1].Values.TryGetValue("Market", out object market));
            Assert.AreEqual("8R6NtLSyVpgG6f8Z9cLKFcuaJtt3mqHypKGsmRixwyJV", (PublicKey)openOrders);
            Assert.AreEqual("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh", (PublicKey)owner);
            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", (PublicKey)market);
            
            Assert.AreEqual("New Order V3", ix[2].InstructionName);
            Assert.AreEqual("Serum Program", ix[2].ProgramName);
            Assert.AreEqual("9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin", ix[2].PublicKey);
            Assert.AreEqual(0, ix[2].InnerInstructions.Count);
            Assert.IsTrue(ix[2].Values.TryGetValue("Open Orders Account", out openOrders));
            Assert.IsTrue(ix[2].Values.TryGetValue("Owner", out owner));
            Assert.IsTrue(ix[2].Values.TryGetValue("Market", out market));
            Assert.IsTrue(ix[2].Values.TryGetValue("Request Queue", out object requestQueue));
            Assert.IsTrue(ix[2].Values.TryGetValue("Event Queue", out object eventQueue));
            Assert.IsTrue(ix[2].Values.TryGetValue("Bids", out object bids));
            Assert.IsTrue(ix[2].Values.TryGetValue("Asks", out object asks));
            Assert.IsTrue(ix[2].Values.TryGetValue("Payer", out object payer));
            Assert.IsTrue(ix[2].Values.TryGetValue("Base Vault", out object bVault));
            Assert.IsTrue(ix[2].Values.TryGetValue("Quote Vault", out object qVault));
            Assert.IsTrue(ix[2].Values.TryGetValue("Token Program Id", out object tokenKeg));           
            Assert.IsTrue(ix[2].Values.TryGetValue("Side", out object side));
            Assert.IsTrue(ix[2].Values.TryGetValue("Limit Price", out object limit));
            Assert.IsTrue(ix[2].Values.TryGetValue("Max Base Coin Quantity", out object maxBase));
            Assert.IsTrue(ix[2].Values.TryGetValue("Max Quote Coin Quantity", out object maxQuote));
            Assert.IsTrue(ix[2].Values.TryGetValue("Self Trade Behavior", out object selfTrade));
            Assert.IsTrue(ix[2].Values.TryGetValue("Order Type", out object orderType));
            Assert.AreEqual("8R6NtLSyVpgG6f8Z9cLKFcuaJtt3mqHypKGsmRixwyJV", (PublicKey)openOrders);
            Assert.AreEqual("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh", (PublicKey)owner);
            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", (PublicKey)market);
            Assert.AreEqual("BRvzjEjphBLVMMq8tEvh4G5o9TTNJ4PSu23CAAdJDKsr", (PublicKey)requestQueue);
            Assert.AreEqual("9gpfTc4zsndJSdpnpXQbey16L5jW2GWcKeY3PLixqU4", (PublicKey)eventQueue);
            Assert.AreEqual("8MyQkxux1NnpNqpBbPeiQHYeDbZvdvs7CHmGpciSMWvs", (PublicKey)bids);
            Assert.AreEqual("HjB8zKe9xezDrgqXCSjCb5F7dMC9WMwtZoT7yKYEhZYV", (PublicKey)asks);
            Assert.AreEqual("tWQuevB8Rou1HS9a76fjYSQPrDixZMbVzXe2Q1kY5ma", (PublicKey)payer);
            Assert.AreEqual("3hUMPnn3WNUbhTBoyXH3wHkWyq85MEZx9LWLTdEEaTef", (PublicKey)bVault);
            Assert.AreEqual("HEArHmgm9mnsj2u98Ldr4iWSwWvPPUUg8L9fwxT1cTyv", (PublicKey)qVault);
            Assert.AreEqual("TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA", (PublicKey)tokenKeg);
            Assert.AreEqual(Side.Buy, (Side)side);
            Assert.AreEqual(3165UL, (ulong)limit);
            Assert.AreEqual(10UL, (ulong)maxBase);
            Assert.AreEqual(3165000UL, (ulong)maxQuote);
            Assert.AreEqual(SelfTradeBehavior.CancelProvide, (SelfTradeBehavior)selfTrade);
            Assert.AreEqual(OrderType.Limit, (OrderType)orderType);

            Assert.AreEqual("Settle Funds", ix[7].InstructionName);
            Assert.AreEqual("Serum Program", ix[7].ProgramName);
            Assert.AreEqual("9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin", ix[7].PublicKey);
            Assert.AreEqual(0, ix[7].InnerInstructions.Count);
            Assert.IsTrue(ix[7].Values.TryGetValue("Open Orders Account", out openOrders));
            Assert.IsTrue(ix[7].Values.TryGetValue("Owner", out owner));
            Assert.IsTrue(ix[7].Values.TryGetValue("Market", out market));
            Assert.IsTrue(ix[7].Values.TryGetValue("Base Vault", out bVault));
            Assert.IsTrue(ix[7].Values.TryGetValue("Quote Vault", out qVault));
            Assert.IsTrue(ix[7].Values.TryGetValue("Token Program Id", out tokenKeg));   
            Assert.IsTrue(ix[7].Values.TryGetValue("Base Account", out object bAcc));
            Assert.IsTrue(ix[7].Values.TryGetValue("Quote Account", out object qAcc));
            Assert.IsTrue(ix[7].Values.TryGetValue("Vault Signer", out object vaultSigner));
            Assert.AreEqual("8R6NtLSyVpgG6f8Z9cLKFcuaJtt3mqHypKGsmRixwyJV", (PublicKey)openOrders);
            Assert.AreEqual("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh", (PublicKey)owner);
            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", (PublicKey)market);
            Assert.AreEqual("3hUMPnn3WNUbhTBoyXH3wHkWyq85MEZx9LWLTdEEaTef", (PublicKey)bVault);
            Assert.AreEqual("HEArHmgm9mnsj2u98Ldr4iWSwWvPPUUg8L9fwxT1cTyv", (PublicKey)qVault);
            Assert.AreEqual("TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA", (PublicKey)tokenKeg);
            Assert.AreEqual("CQgjkmuDXXJ2WpF6bK9VWZko9T5hwVxAVgvmbV3gkfVe", (PublicKey)vaultSigner);
            Assert.AreEqual("AraQPzSsE31pdzeTe6Dkvu6g8PvreFW429DAYhsfKYRd", (PublicKey)bAcc);
            Assert.AreEqual("tWQuevB8Rou1HS9a76fjYSQPrDixZMbVzXe2Q1kY5ma", (PublicKey)qAcc);
        }

        [TestMethod]
        public void DecodeCancelOrdersTest()
        {
            Message msg = Message.Deserialize(Convert.FromBase64String(CancelOrdersAndSettleMsg));
            List<DecodedInstruction> ix = InstructionDecoder.DecodeInstructions(msg);

            Assert.AreEqual(5, ix.Count);
            Assert.AreEqual("Cancel Order V2", ix[1].InstructionName);
            Assert.AreEqual("Serum Program", ix[1].ProgramName);
            Assert.AreEqual("9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin", ix[1].PublicKey);
            Assert.AreEqual(0, ix[1].InnerInstructions.Count);
            Assert.IsTrue(ix[1].Values.TryGetValue("Open Orders Account", out object openOrders));
            Assert.IsTrue(ix[1].Values.TryGetValue("Owner", out object owner));
            Assert.IsTrue(ix[1].Values.TryGetValue("Market", out object market));
            Assert.IsTrue(ix[1].Values.TryGetValue("Bids", out object bids));
            Assert.IsTrue(ix[1].Values.TryGetValue("Asks", out object asks));
            Assert.IsTrue(ix[1].Values.TryGetValue("Event Queue", out object eventQueue));
            Assert.IsTrue(ix[1].Values.TryGetValue("Side", out object side));
            Assert.IsTrue(ix[1].Values.TryGetValue("Order Id", out object orderId));
            Assert.AreEqual("8R6NtLSyVpgG6f8Z9cLKFcuaJtt3mqHypKGsmRixwyJV", (PublicKey)openOrders);
            Assert.AreEqual("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh", (PublicKey)owner);
            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", (PublicKey)market);
            Assert.AreEqual("9gpfTc4zsndJSdpnpXQbey16L5jW2GWcKeY3PLixqU4", (PublicKey)eventQueue);
            Assert.AreEqual("8MyQkxux1NnpNqpBbPeiQHYeDbZvdvs7CHmGpciSMWvs", (PublicKey)bids);
            Assert.AreEqual("HjB8zKe9xezDrgqXCSjCb5F7dMC9WMwtZoT7yKYEhZYV", (PublicKey)asks);
            Assert.AreEqual(Side.Buy, (Side)side);
            Assert.AreEqual(new BigInteger(57240246860720736513843M), (BigInteger)orderId);
        }
        
        [TestMethod]
        public void DecodeInitOpenOrdersAuthorityTest()
        {
            Message msg = Message.Deserialize(Convert.FromBase64String(InitOpenOrdersAuthorityMsg));
            List<DecodedInstruction> ix = InstructionDecoder.DecodeInstructions(msg);

            Assert.AreEqual(1, ix.Count);
            Assert.AreEqual("Init Open Orders", ix[0].InstructionName);
            Assert.AreEqual("Serum Program", ix[0].ProgramName);
            Assert.AreEqual("9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin", ix[0].PublicKey);
            Assert.IsTrue(ix[0].Values.TryGetValue("Open Orders Account", out object openOrders));
            Assert.IsTrue(ix[0].Values.TryGetValue("Owner", out object owner));
            Assert.IsTrue(ix[0].Values.TryGetValue("Market", out object market));
            Assert.IsTrue(ix[0].Values.TryGetValue("Market Authority", out object marketAuthority));
            Assert.AreEqual("7xFCBA6F9xLg56hCMRBeDboJQUFNE5KHfjXGuFukavau", (PublicKey)openOrders);
            Assert.AreEqual("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh", (PublicKey)owner);
            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", (PublicKey)market);
            Assert.AreEqual("Fp7AZKKazcqU4N83opDzEZMTnQ6CtRxT118CGNZpziSe", (PublicKey)marketAuthority);
        }

        [TestMethod]
        public void DecodeCancelAndConsumeEventsTest()
        {
            Message msg = Message.Deserialize(Convert.FromBase64String(CancelByClientIdAndConsumeEventsMsg));
            List<DecodedInstruction> ix = InstructionDecoder.DecodeInstructions(msg);

            Assert.AreEqual(2, ix.Count);
            Assert.AreEqual("Cancel Order By Client Id V2", ix[0].InstructionName);
            Assert.AreEqual("Serum Program", ix[0].ProgramName);
            Assert.AreEqual("9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin", ix[0].PublicKey);
            Assert.IsTrue(ix[0].Values.TryGetValue("Open Orders Account", out object openOrders));
            Assert.IsTrue(ix[0].Values.TryGetValue("Owner", out object owner));
            Assert.IsTrue(ix[0].Values.TryGetValue("Market", out object market));
            Assert.IsTrue(ix[0].Values.TryGetValue("Bids", out object bids));
            Assert.IsTrue(ix[0].Values.TryGetValue("Asks", out object asks));
            Assert.IsTrue(ix[0].Values.TryGetValue("Event Queue", out object eventQueue));
            Assert.IsTrue(ix[0].Values.TryGetValue("Client Id", out object clientId));
            Assert.AreEqual("14QkUy2jkZU2coqY8mTjL3uGiicTTng1VXrYKK1xk7yW", (PublicKey)openOrders);
            Assert.AreEqual("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh", (PublicKey)owner);
            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", (PublicKey)market);
            Assert.AreEqual("8MyQkxux1NnpNqpBbPeiQHYeDbZvdvs7CHmGpciSMWvs", (PublicKey)bids);
            Assert.AreEqual("HjB8zKe9xezDrgqXCSjCb5F7dMC9WMwtZoT7yKYEhZYV", (PublicKey)asks);
            Assert.AreEqual("9gpfTc4zsndJSdpnpXQbey16L5jW2GWcKeY3PLixqU4", (PublicKey)eventQueue);
            Assert.AreEqual(100000UL, (ulong)clientId);
            
            Assert.AreEqual("Consume Events", ix[1].InstructionName);
            Assert.AreEqual("Serum Program", ix[1].ProgramName);
            Assert.AreEqual("9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin", ix[1].PublicKey);
            Assert.IsTrue(ix[1].Values.TryGetValue("Market", out  market));
            Assert.IsTrue(ix[1].Values.TryGetValue("Event Queue", out  eventQueue));
            Assert.IsTrue(ix[1].Values.TryGetValue("Base Account", out object bAcc));
            Assert.IsTrue(ix[1].Values.TryGetValue("Quote Account", out object qAcc));
            Assert.IsTrue(ix[1].Values.TryGetValue("Open Orders Account 1", out object openOrdersConsumed));
            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", (PublicKey)market);
            Assert.AreEqual("9gpfTc4zsndJSdpnpXQbey16L5jW2GWcKeY3PLixqU4", (PublicKey)eventQueue);
            Assert.AreEqual("AraQPzSsE31pdzeTe6Dkvu6g8PvreFW429DAYhsfKYRd", (PublicKey)bAcc);
            Assert.AreEqual("tWQuevB8Rou1HS9a76fjYSQPrDixZMbVzXe2Q1kY5ma", (PublicKey)qAcc);
            Assert.AreEqual("14QkUy2jkZU2coqY8mTjL3uGiicTTng1VXrYKK1xk7yW", (PublicKey)openOrdersConsumed);
        }

        [TestMethod]
        public void DecodePruneTest()
        {
            Message msg = Message.Deserialize(Convert.FromBase64String(PruneMsg));
            List<DecodedInstruction> ix = InstructionDecoder.DecodeInstructions(msg);
            
            Assert.AreEqual(1, ix.Count);
            Assert.AreEqual("Prune", ix[0].InstructionName);
            Assert.AreEqual("Serum Program", ix[0].ProgramName);
            Assert.AreEqual("9xQeWvG816bUx9EPjHmaT23yvVM2ZWbrrpZb9PusVFin", ix[0].PublicKey);
            Assert.IsTrue(ix[0].Values.TryGetValue("Market", out object market));
            Assert.IsTrue(ix[0].Values.TryGetValue("Bids", out object bids));
            Assert.IsTrue(ix[0].Values.TryGetValue("Asks", out object asks));
            Assert.IsTrue(ix[0].Values.TryGetValue("Event Queue", out object eventQueue));
            Assert.IsTrue(ix[0].Values.TryGetValue("Owner", out object owner));
            Assert.IsTrue(ix[0].Values.TryGetValue("Open Orders Account", out object openOrders));
            Assert.IsTrue(ix[0].Values.TryGetValue("Prune Authority", out object pruneAuthority));
            Assert.AreEqual("4LUro5jaPaTurXK737QAxgJywdhABnFAMQkXX4ZyqqaZ", (PublicKey)market);
            Assert.AreEqual("8MyQkxux1NnpNqpBbPeiQHYeDbZvdvs7CHmGpciSMWvs", (PublicKey)bids);
            Assert.AreEqual("HjB8zKe9xezDrgqXCSjCb5F7dMC9WMwtZoT7yKYEhZYV", (PublicKey)asks);
            Assert.AreEqual("9gpfTc4zsndJSdpnpXQbey16L5jW2GWcKeY3PLixqU4", (PublicKey)eventQueue);
            Assert.AreEqual("hoakwpFB8UoLnPpLC56gsjpY7XbVwaCuRQRMQzN5TVh", (PublicKey)owner);
            Assert.AreEqual("RwatxRLAiLNqjzBxB8hWXArgtJ7G84zq7xxoYnrUzZB", (PublicKey)openOrders);
            Assert.AreEqual("DR6cxg8D7dXYhtCfuMqBmQFSTRAgcYXUd9SZK7dFgsD3", (PublicKey)pruneAuthority);
        }
    }
}