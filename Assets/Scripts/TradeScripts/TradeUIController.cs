using UnityEngine;
using UnityEngine.UI;
using System.Numerics;
using TMPro;

public class TradeUIController : MonoBehaviour
{
    public TMP_InputField recipientWalletInput;
    public TMP_InputField tokenIdInput;
    public TMP_InputField requestedTokenIdInput;
    public TradeContractService tradeService;

    public async void OnProposeTradeClicked()
    {
        string recipient = recipientWalletInput.text;
        if (!BigInteger.TryParse(tokenIdInput.text, out BigInteger proposerTokenId))
        {
            Debug.LogError("Invalid proposer token ID");
            return;
        }

        if (!BigInteger.TryParse(requestedTokenIdInput.text, out BigInteger requestedTokenId))
        {
            Debug.LogError("Invalid requested token ID");
            return;
        }

        await tradeService.ApproveAndCreateTrade(recipient, proposerTokenId, requestedTokenId);
    }
}
