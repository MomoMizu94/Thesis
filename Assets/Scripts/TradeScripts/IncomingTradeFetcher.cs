using UnityEngine;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

public class IncomingTradeFetcher : MonoBehaviour
{
    public GameObject tradeOfferCardPrefab;
    public Transform tradeListContainer;
    public TradeOfferManager tradeOfferManager;

    public string rpcUrl;
    public string tradeContractAddress;
    public TextAsset tradeContractABI;
    public PlayerWalletData walletData;

    public TMP_Text statusText;

    private Web3 web3;
    private Contract contract;

    [FunctionOutput]
    public class TradeOfferDTO : IFunctionOutputDTO
    {
        [Parameter("address", "proposer", 1)]
        public string Proposer { get; set; }

        [Parameter("address", "recipient", 2)]
        public string Recipient { get; set; }

        [Parameter("uint256", "proposerTokenId", 3)]
        public BigInteger ProposerTokenId { get; set; }

        [Parameter("uint256", "requestedTokenId", 4)]
        public BigInteger RequestedTokenId { get; set; }

        [Parameter("uint256", "timestamp", 5)]
        public BigInteger Timestamp { get; set; }

        [Parameter("bool", "isActive", 6)]
        public bool IsActive { get; set; }
    }


    public List<BigInteger> tokenIdsWithOffers = new List<BigInteger>();

    public async void FetchMyTradeOffers()
    {
        web3 = new Web3(rpcUrl);
        contract = web3.Eth.GetContract(tradeContractABI.text, tradeContractAddress);

        var getOfferFunc = contract.GetFunction("getTradeOffer");
        tokenIdsWithOffers.Clear();
        string myAddress = walletData.walletAddress.ToLower();
        int found = 0;

        for (int i = 0; i < 20; i++)
        {
            BigInteger tokenId = new BigInteger(i);
            try
            {
                var result = await getOfferFunc.CallDeserializingToObjectAsync<TradeOfferDTO>(tokenId);
                if (result.IsActive && result.Recipient.ToLower() == myAddress)
                {
                    tokenIdsWithOffers.Add(tokenId);
                    Debug.Log($"Trade offer found: Token ID {tokenId} from {result.Proposer}");
                    CreateOfferCard(result);
                    found++;
                }
            }
            catch { }

            await Task.Delay(500);
        }

        statusText.text = $"Found {found} trade offers.";
    }

    private void CreateOfferCard(TradeOfferDTO offer)
    {
        GameObject card = Instantiate(tradeOfferCardPrefab, tradeListContainer);

        var ui = card.GetComponent<TradeOfferCardUI>();

        if (ui != null)
        {
            ui.Setup(
                offer.Proposer,
                offer.ProposerTokenId,
                offer.RequestedTokenId,
                tradeOfferManager
            );
        }
    }
}
