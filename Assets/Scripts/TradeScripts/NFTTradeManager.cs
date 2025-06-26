using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Nethereum.Web3;
using SimpleJSON;

public class NFTTradeManager : MonoBehaviour
{
    [Header("Blockchain Settings")]
    public string rpcUrl;
    public string contractAddress;
    public TextAsset contractABI;

    [Header("References")]
    public PlayerWalletData playerWalletData;
    public NFTApprovalService approvalService;
    public TradeContractService tradeService;

    [Header("UI")]
    public GameObject cardPrefab;
    public Transform contentTransform;

    [Header("UI Inputs")]
    public TMP_InputField recipientInputField;
    public TMP_InputField desiredTokenIdInputField;

    private NFTCardData selectedCard;

    public async void ShowTradableCards()
    {
        ClearCards();
        await LoadOwnedNFTs();
    }

    private async Task LoadOwnedNFTs()
    {
        var web3 = new Web3(rpcUrl);
        var contract = web3.Eth.GetContract(contractABI.text, contractAddress);
        var ownerOf = contract.GetFunction("ownerOf");
        var tokenURI = contract.GetFunction("tokenURI");

        string wallet = playerWalletData.walletAddress.ToLower();
        int found = 0;

        for (int i = 0; i < 20; i++)
        {
            BigInteger tokenId = new BigInteger(i);

            try
            {
                string owner = await ownerOf.CallAsync<string>(tokenId);
                if (owner.ToLower() != wallet) continue;

                string uri = await tokenURI.CallAsync<string>(tokenId);
                string json = DecodeBase64Json(uri);
                NFTCardData data = ParseMetadata(json);
                data.tokenId = tokenId;
                data.ownerAddress = wallet;

                CreateCardUI(data);
                found++;
            }
            catch { /* Skip missing tokenIds: Not implemented */ }

            await Task.Delay(800); // Avoid rate limits
        }

        Debug.Log($"Loaded {found} tradable NFTs.");
    }

    private string DecodeBase64Json(string uri)
    {
        const string prefix = "data:application/json;base64,";
        if (!uri.StartsWith(prefix)) return null;

        string base64 = uri.Substring(prefix.Length);
        byte[] bytes = System.Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    private NFTCardData ParseMetadata(string json)
    {
        var obj = JSON.Parse(json);
        return new NFTCardData
        {
            name = obj["name"],
            description = obj["description"],
            imageURI = obj["image"],
            attack = obj["attributes"][0]["value"].AsInt,
            health = obj["attributes"][1]["value"].AsInt,
            mana = obj["attributes"][2]["value"].AsInt
        };
    }

    private void CreateCardUI(NFTCardData data)
    {
        GameObject card = Instantiate(cardPrefab, contentTransform);
        var display = card.GetComponent<NFTCardDisplay>();

        if (display != null)
        {
            display.SetCardData(data);
        }

        var clickHandler = card.GetComponent<TradeCardClickHandler>();
        if (clickHandler != null)
        {
            clickHandler.Initialize(this, data);
        }
    }

    public void SetSelectedCard(NFTCardData card)
    {
        selectedCard = card;
        Debug.Log($"Selected card for trade: {card.name} (TokenID: {card.tokenId})");
    }

    public async void ProposeTrade()
    {
        if (selectedCard == null)
        {
            Debug.LogWarning("No card selected.");
            return;
        }

        string recipient = recipientInputField.text;
        string desiredTokenIdText = desiredTokenIdInputField.text;

        if (!BigInteger.TryParse(desiredTokenIdText, out BigInteger desiredTokenId))
        {
            Debug.LogError("Invalid token ID.");
            return;
        }

        try
        {
            Debug.Log("Approving NFT...");
            Debug.Log("Sending following data: " + playerWalletData.walletAddress + playerWalletData.privateKey + selectedCard.tokenId);
            await approvalService.ApproveToken(playerWalletData.walletAddress, playerWalletData.privateKey, selectedCard.tokenId);

            Debug.Log("Creating trade...");
            await tradeService.CreateTradeOffer(recipient, selectedCard.tokenId, desiredTokenId);

            Debug.Log("Trade proposed.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Trade failed: " + ex.Message);
        }
    }

    private void ClearCards()
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
    }
}
