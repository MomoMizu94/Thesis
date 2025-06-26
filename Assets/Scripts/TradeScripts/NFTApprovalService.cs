using UnityEngine;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;
using System;

public class NFTApprovalService : MonoBehaviour
{
    [Header("Blockchain Setup")]
    public string rpcUrl;
    public string contractAddress;          // NFT contract address
    public string tradeContractAddress;

    private string ownerAddress;
    private string privateKey;


    public void Initialize(string ownerAddress, string privateKey)
    {
        this.ownerAddress = ownerAddress;
        this.privateKey = privateKey;
    }

    public async Task<bool> ApproveToken(string ownerAddress, string privateKey, BigInteger tokenId)
    {
        var account = new Nethereum.Web3.Accounts.Account(privateKey);
        var web3 = new Web3(account, rpcUrl);

        string erc721Abi = @"[
            { ""constant"": false, ""inputs"": [ { ""name"": ""to"", ""type"": ""address"" }, { ""name"": ""tokenId"", ""type"": ""uint256"" } ], ""name"": ""approve"", ""outputs"": [], ""type"": ""function"" }
        ]";

        var contract = web3.Eth.GetContract(erc721Abi, contractAddress);
        var approveFunction = contract.GetFunction("approve");

        try
        {
            Debug.Log($"Approving tokenId {tokenId} from {ownerAddress} to {tradeContractAddress}");

            var gasLimit = new Nethereum.Hex.HexTypes.HexBigInteger(500000);
            var txHash = await approveFunction.SendTransactionAsync(ownerAddress, gasLimit, null, null, tradeContractAddress, tokenId);

            Debug.Log("Token approval transaction sent. Tx Hash: " + txHash);

            // Wait for confirmation
            var receiptService = new Nethereum.RPC.TransactionReceipts.TransactionReceiptPollingService(web3.TransactionManager);
            var receipt = await receiptService.PollForReceiptAsync(txHash);

            if (receipt.Status.Value == 1)
            {
                Debug.Log("Approval confirmed on-chain.");
                return true;
            }
            else
            {
                Debug.LogError("Approval failed on-chain.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Approval failed: " + ex.Message);
            return false;
        }
    }


}