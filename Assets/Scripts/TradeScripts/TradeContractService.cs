using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using UnityEngine;
using System;
using Nethereum.ABI.FunctionEncoding.Attributes;


public class TradeContractService : MonoBehaviour
{
    [Header("Contract Settings")]
    public string contractAddress;
    public string nftContractAbi;
    public PlayerWalletData walletData;
    public string rpcUrl;

    public string tradeContractABI;
    public string tradeContractAddress;

    private Web3 web3;
    private Contract contract;
    private string accountAddress;

    private Contract tradeContract;

    void Start()
    {
        var account = new Account(walletData.privateKey);
        accountAddress = account.Address;
        web3 = new Web3(account, rpcUrl);
        contract = web3.Eth.GetContract(nftContractAbi, contractAddress);
        tradeContract = web3.Eth.GetContract(tradeContractABI, tradeContractAddress);
    }

    // DTO class for deserializing getTradeOffer
    [FunctionOutput]
    public class TradeOfferDTO : IFunctionOutputDTO
    {
        [Parameter("address", "proposer", 1)]
        public string proposer { get; set; }

        [Parameter("address", "recipient", 2)]
        public string recipient { get; set; }

        [Parameter("uint256", "proposerTokenId", 3)]
        public BigInteger proposerTokenId { get; set; }

        [Parameter("uint256", "requestedTokenId", 4)]
        public BigInteger requestedTokenId { get; set; }

        [Parameter("uint256", "timestamp", 5)]
        public BigInteger timestamp { get; set; }

        [Parameter("bool", "isActive", 6)]
        public bool isActive { get; set; }
    }

    public async Task<string> ApproveToken(BigInteger tokenId)
    {
        try
        {
            var approveFunction = contract.GetFunction("approve");
            var gas = await approveFunction.EstimateGasAsync(accountAddress, null, null, contractAddress, tokenId);
            var txHash = await approveFunction.SendTransactionAsync(accountAddress, gas, null, null, contractAddress, tokenId);

            Debug.Log($"Approval TX Hash: {txHash}");
            return txHash;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Approval failed: {ex.Message}");
            return null;
        }
    }

    public async Task<string> CreateTradeOffer(string recipientAddress, BigInteger proposerTokenId, BigInteger requestedTokenId)
    {
        try
        {
            Debug.Log("Checking for existing trade offer...");
            var getOfferFunction = tradeContract.GetFunction("getTradeOffer");

            // Destructure tuple from getTradeOffer
            var offer = await getOfferFunction.CallDeserializingToObjectAsync<TradeOfferDTO>(proposerTokenId);

            if (offer.isActive)
            {
                Debug.LogWarning("Trade offer already exists for this token.");
                return null;
            }

            Debug.Log("No active trade. Proceeding with offer creation...");
            var createFunction = tradeContract.GetFunction("createTradeOffer");

            var txHash = await createFunction.SendTransactionAsync(
                accountAddress,
                new HexBigInteger(300000),
                null,
                null,
                recipientAddress,
                proposerTokenId,
                requestedTokenId
            );

            Debug.Log("Trade offer created! TX Hash: " + txHash);
            return txHash;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Trade creation failed: {ex.Message}");
            if (ex.InnerException != null)
                Debug.LogError("Inner exception: " + ex.InnerException.Message);
            return null;
        }
    }


    public async Task<string> ApproveAndCreateTrade(string recipientAddress, BigInteger proposerTokenId, BigInteger requestedTokenId)
    {
        var approvalHash = await ApproveToken(proposerTokenId);
        if (approvalHash == null)
        {
            Debug.LogError("Token approval failed. Cannot create trade offer.");
            return null;
        }

        await Task.Delay(10000); // Wait 10 seconds to let the approval confirm
        return await CreateTradeOffer(recipientAddress, proposerTokenId, requestedTokenId);
    }
}
