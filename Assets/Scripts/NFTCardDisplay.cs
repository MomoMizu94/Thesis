using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class NFTCardDisplay : MonoBehaviour
{
    public RawImage cardImage;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    public NFTCardData cardData;

    public void SetCardData(NFTCardData data)
    {
        // Debugging
        Debug.Log($"Setting card data for {cardData.name}");

        cardData = data;

        // Update the text fields for attack, health, mana, name, and description
        attackText.text = cardData.attack.ToString();
        healthText.text = cardData.health.ToString();
        manaText.text = cardData.mana.ToString();

        // Update the name and description text fields
        nameText.text = cardData.name;
        descriptionText.text = cardData.description;

        // Start the coroutine to load the image
        StartCoroutine(LoadImage(cardData.imageURI));
    }

    private IEnumerator LoadImage(string uri)
    {
        Debug.Log($"Loading image from: {uri}");
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            cardImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Debug.Log("Image loaded and applied successfully.");
        }
        else
        {
            Debug.LogError("Image load failed: " + request.error);
        }
    }
}
