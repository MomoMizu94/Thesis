using UnityEngine;
using System.Numerics;
using System.Threading.Tasks;
using System;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using TMPro;

public class TradeOfferManager : MonoBehaviour
{
    public GameObject tradeStatusPanel;
    public TMP_Text statusText; 

    public TMP_Text etherscanLinkText;
    private string etherscanBaseUrl = "https://sepolia.etherscan.io/tx/";
    private string latestTxHash;
    
    public PlayerWalletData walletData;
    public string rpcUrl;
    public string tradeContractAddress;
    public TextAsset tradeContractABI;
    public string nftContractAddress;
    public TextAsset nftContractABI;

    private TradeOfferCardUI selectedOffer;

    public async void OnAcceptTradeClicked()
    {
        var selected = GetSelectedOffer();

        if (selected == null)
        {
            Debug.LogWarning("No trade offer selected.");
            return;
        }

        var proposerTokenId = selected.proposerTokenId;
        var yourTokenId = selected.requestedTokenId;

        var account = new Nethereum.Web3.Accounts.Account(walletData.privateKey);
        var web3 = new Web3(account, rpcUrl);

        try
        {
            // Approve token
            var nftContract = web3.Eth.GetContract(nftContractABI.text, nftContractAddress);
            var approve = nftContract.GetFunction("approve");

            Debug.Log($"Approving your token {yourTokenId}...");
            var gas = new Nethereum.Hex.HexTypes.HexBigInteger(100000);
            var txHash = await approve.SendTransactionAsync(
                walletData.walletAddress,
                gas,
                null,
                null,
                tradeContractAddress,
                yourTokenId
            );

            Debug.Log($"Approval sent! TX Hash: {txHash}");

            // Accept the trade
            var tradeContract = web3.Eth.GetContract(tradeContractABI.text, tradeContractAddress);
            var accept = tradeContract.GetFunction("acceptTrade");

            Debug.Log($"Accepting trade: taking {proposerTokenId}, offering {yourTokenId}...");
            
            // Estimate gas with fallback
            HexBigInteger acceptGas;
            try
            {
                acceptGas = await accept.EstimateGasAsync(walletData.walletAddress, null, null, proposerTokenId, yourTokenId);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Gas estimation failed, using fallback gas limit. Reason: " + ex.Message);
                acceptGas = new HexBigInteger(300000);  // Fallback value
            }

            // Send transaction
            var tx = await accept.SendTransactionAsync(walletData.walletAddress, acceptGas, null, null, proposerTokenId, yourTokenId);


            Debug.Log($"Trade accepted! TX Hash: {tx}");
            latestTxHash = tx;
            statusText.text = "Trade accepted!\nTransaction submitted.";
            etherscanLinkText.text = $"<color=#0000EE><u>View on Etherscan</u></color>";
            etherscanLinkText.gameObject.SetActive(true);

        }
        catch (System.Exception ex)
        {
            Debug.LogError("Trade acceptance failed: " + ex.Message);
            statusText.text = "Trade failed: " + ex.Message;
        }

        // Auto-hide after few seconds
        await Task.Delay(2000);
        tradeStatusPanel.SetActive(false);
    }

    public async void OnDeclineTradeClicked()
    {
        var selected = GetSelectedOffer();

        if (selected == null)
        {
            Debug.LogWarning("No trade offer selected.");
            return;
        }

        var proposerTokenId = selected.proposerTokenId;
        var account = new Nethereum.Web3.Accounts.Account(walletData.privateKey);
        var web3 = new Web3(account, rpcUrl);

        try
        {
            var contract = web3.Eth.GetContract(tradeContractABI.text, tradeContractAddress);
            var declineFunc = contract.GetFunction("declineTrade");

            Debug.Log($"Declining trade offer for token ID {proposerTokenId}...");
            var gas = await declineFunc.EstimateGasAsync(walletData.walletAddress, null, null, proposerTokenId);
            var tx = await declineFunc.SendTransactionAsync(walletData.walletAddress, gas, null, null, proposerTokenId);

            Debug.Log($"Trade declined! TX Hash: {tx}");
            latestTxHash = tx;
            statusText.text = "Trade declined.";
            etherscanLinkText.text = $"<color=#0000EE><u>View on Etherscan</u></color>";
            etherscanLinkText.gameObject.SetActive(true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Decline failed: " + ex.Message);
            statusText.text = "Decline failed: " + ex.Message;
        }

        await Task.Delay(10000);
        tradeStatusPanel.SetActive(false);
    }
    
    public void SetSelectedOffer(TradeOfferCardUI offer)
    {
        selectedOffer = offer;
    }

    public TradeOfferCardUI GetSelectedOffer()
    {
        return selectedOffer;
    }

    public void OnEtherscanLinkClicked()
    {
        if (!string.IsNullOrEmpty(latestTxHash))
        {
            Application.OpenURL(etherscanBaseUrl + latestTxHash);
        }
    }
}
