using UnityEngine;

public class TradeCardClickHandler : MonoBehaviour
{
    private NFTCardData cardData;
    private NFTTradeManager tradeManager;

    public void Initialize(NFTTradeManager manager, NFTCardData data)
    {
        tradeManager = manager;
        cardData = data;
    }

    public void OnClick()
    {
        tradeManager.SetSelectedCard(cardData);
        Debug.Log("Card clicked: " + cardData.tokenId);
    }
}

