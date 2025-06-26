using UnityEngine;
using TMPro;
using System.Numerics;

public class TradeOfferCardUI : MonoBehaviour
{
    public TMP_Text proposerText;
    public TMP_Text offeredTokenText;
    public TMP_Text requestedTokenText;

    [HideInInspector] public BigInteger proposerTokenId;
    [HideInInspector] public BigInteger requestedTokenId;
    [HideInInspector] public string proposer;

    private TradeOfferManager tradeManager;

    public void Setup(string proposer, BigInteger proposerTokenId, BigInteger requestedTokenId, TradeOfferManager manager)
    {
        this.proposer = proposer;
        this.proposerTokenId = proposerTokenId;
        this.requestedTokenId = requestedTokenId;
        this.tradeManager = manager;

        proposerText.text = $"From: {proposer}";
        offeredTokenText.text = $"They offer: Token #{proposerTokenId}";
        requestedTokenText.text = $"They want: Token #{requestedTokenId}";
    }

    public void SelectThisOffer()
    {
        tradeManager.SetSelectedOffer(this);
    }
}
