using System;
using System.Collections;
using System.Numerics;
using System.Text;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.Threading.Tasks;


public class NFTMinter : MonoBehaviour
{
    public NFTCardDataPool cardDataPool;

    [Header("Contract Information")]
    // Public testnet connection url
    public string rpcUrl;
    // Smart contract address
    public string contractAddress = "0x2F7aEb778Ab9b9956a9fAB8bA3a120E20290c2a4";
    // SC ABI
    public TextAsset contractABI;

    // Reference to ScriptableObject (wallet data)
    public PlayerWalletData playerWalletData;

    public void MintRandomizer()
    {
        _ = MintNFTAsync();
    }

    private async Task MintNFTAsync()
    {
        // Randomize card data
        NFTCardData randomCard = cardDataPool.nftCards[UnityEngine.Random.Range(0, cardDataPool.nftCards.Length)];

        // Build metadata JSON using SimpleJSON script
        var metadata = new JSONObject();
        metadata["name"] = randomCard.name;
        metadata["description"] = randomCard.description;
        metadata["image"] = randomCard.imageURI;

        var attributes = new JSONArray();

        var attackAttr = new JSONObject();
        attackAttr["trait_type"] = "Attack";
        attackAttr["value"] = randomCard.attack;
        attributes.Add(attackAttr);

        var healthAttr = new JSONObject();
        healthAttr["trait_type"] = "Health";
        healthAttr["value"] = randomCard.health;
        attributes.Add(healthAttr);

        var manaAttr = new JSONObject();
        manaAttr["trait_type"] = "Mana";
        manaAttr["value"] = randomCard.mana;
        attributes.Add(manaAttr);

        metadata["attributes"] = attributes;

        string metadataJson = metadata.ToString();
        string encodedJson = Convert.ToBase64String(Encoding.UTF8.GetBytes(metadataJson));
        string tokenURI = "data:application/json;base64," + encodedJson;

        // Contract interactions
        Account account = new Account(playerWalletData.privateKey);
        Web3 web3 = new Web3(account, rpcUrl);
        Contract contract = web3.Eth.GetContract(contractABI.text, contractAddress);
        var mintFunction = contract.GetFunction("mintItem");

        string toAddress = playerWalletData.walletAddress;

        var estimatedGas = await mintFunction.EstimateGasAsync(account.Address, null, null, toAddress, tokenURI);

        var value = new Nethereum.Hex.HexTypes.HexBigInteger(0);

        var mintTxn = await mintFunction.SendTransactionAsync(account.Address, estimatedGas, value, toAddress, tokenURI);

        Debug.Log("NFT Minted! Txn: " + mintTxn);

        // Reveal the newly minted card for player
        CardRevealUI.Instance.ShowCard(randomCard);
    }
}