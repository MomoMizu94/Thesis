using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class CardRevealUI : MonoBehaviour
{
    public static CardRevealUI Instance;

    [Header("UI References")]
    public GameObject cardRevealPanel;
    public RawImage cardImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;

    private void Awake()
    {
        Instance = this;
        cardRevealPanel.SetActive(false);
    }

    public void ShowCard(NFTCardData cardData)
    {
        nameText.text = cardData.name;
        descriptionText.text = cardData.description;
        attackText.text = cardData.attack.ToString();
        healthText.text = cardData.health.ToString();
        manaText.text = cardData.mana.ToString();

        StartCoroutine(LoadImageFromURI(cardData.imageURI));
        cardRevealPanel.SetActive(true);
    }

    private IEnumerator LoadImageFromURI(string uri)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            cardImage.texture = texture;
        }
    }
    public void CloseCardPanel()
    {
        cardRevealPanel.SetActive(false);
    }
}
