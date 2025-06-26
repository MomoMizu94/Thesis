using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Nethereum.Web3;
using Nethereum.Contracts;
using SimpleJSON;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Numerics;

public class NFTCollectionFetcher : MonoBehaviour
{
    public string rpcUrl;
    public string contractAddress = "0x2F7aEb778Ab9b9956a9fAB8bA3a120E20290c2a4";
    public TextAsset contractABI;
    public PlayerWalletData playerWalletData;

    public GameObject cardPrefab;
    public Transform contentTransform;   
    
    public async void OnCollectionButtonClicked()
    {
        Debug.Log("Collection button clicked. Starting to fetch NFTs...");
        await FetchAndDisplayNFTs();
    }

    private async Task FetchAndDisplayNFTs()
    {
        ClearCards();

        var web3 = new Web3(rpcUrl);
        var contract = web3.Eth.GetContract(contractABI.text, contractAddress);

        var ownerOfFunction = contract.GetFunction("ownerOf");
        var tokenURIFunction = contract.GetFunction("tokenURI");

        string walletAddress = playerWalletData.walletAddress.ToLower();

        int maxTokenId = 20; // Set this to a known max
        int found = 0;

        for (int i = 0; i < maxTokenId; i++)
        {
            BigInteger tokenId = new BigInteger(i);

            try
            {
                string owner = await ownerOfFunction.CallAsync<string>(tokenId);

                if (owner.ToLower() == walletAddress)
                {
                    string tokenURI = await tokenURIFunction.CallAsync<string>(tokenId);
                    string metadataJson = DecodeTokenURI(tokenURI);
                    NFTCardData cardData = ParseMetadata(metadataJson);

                    cardData.ownerAddress = owner;

                    Debug.Log($"Card data fetched: {cardData.name}, {cardData.imageURI}, {cardData.attack}, {cardData.health}, {cardData.mana}");
                    DisplayCard(cardData);

                    found++; // Increment only when card is actually shown
                }
            }
            catch (Nethereum.Contracts.SmartContractCustomErrorRevertException e)
            {
                Debug.LogWarning($"Token ID {tokenId} caused revert: {e.Message}");
                continue;
            }

            // Delay between requests to avoid rate limiting
            await Task.Delay(1000);

        }

        Debug.Log($"Found and displayed {found} NFTs owned by {walletAddress}.");
    }

    private string DecodeTokenURI(string tokenURI)
    {
        if (tokenURI.StartsWith("data:application/json;base64,"))
        {
            string base64Data = tokenURI.Substring("data:application/json;base64,".Length);
            byte[] jsonBytes = System.Convert.FromBase64String(base64Data);
            return Encoding.UTF8.GetString(jsonBytes);
        }

        Debug.LogWarning("Non-base64 metadata format not yet handled.");
        return null;
    }

    private NFTCardData ParseMetadata(string json)
    {
        var jsonObj = JSON.Parse(json);
        var cardData = new NFTCardData();

        cardData.name = jsonObj["name"];
        cardData.description = jsonObj["description"];
        cardData.imageURI = jsonObj["image"];
        cardData.attack = jsonObj["attributes"][0]["value"].AsInt;
        cardData.health = jsonObj["attributes"][1]["value"].AsInt;
        cardData.mana = jsonObj["attributes"][2]["value"].AsInt;

        return cardData;
    }

    private void DisplayCard(NFTCardData cardData)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("cardPrefab is not assigned!");
            return;
        }

        if (contentTransform == null)
        {
            Debug.LogError("Content area is not assigned!");
            return;
        }

        Debug.Log($"Displaying card: {cardData.name}, {cardData.imageURI}, {cardData.attack}, {cardData.health}, {cardData.mana}");

        GameObject card = Instantiate(cardPrefab, contentTransform);

        NFTCardDisplay cardUI = card.GetComponent<NFTCardDisplay>();
        if (cardUI == null)
        {
            Debug.LogError("NFTCardDisplay component missing on cardPrefab!");
            return;
        }

        cardUI.SetCardData(cardData);
    }
    private void ClearCards()
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
    }
}
