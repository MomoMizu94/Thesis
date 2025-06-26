using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WalletLoad : MonoBehaviour
{
    public PlayerWalletData walletData;
    public TextMeshProUGUI walletText;

    void Start()
    {
        if (walletData == null && walletText == null)
        {
            Debug.Log("walletData is not assigned!");
            walletText.text = "No wallet found!";
        }
        else
        {
            Debug.Log("Wallet address: " + walletData.walletAddress);
            walletText.text = "Hello, player: \n" + walletData.walletAddress;
        }
    }
    public void Logout()
    {
        if (walletData != null)
        {
            // Clear the address
            walletData.walletAddress = ""; 
            walletData.privateKey = "";
            Debug.Log("Logged out. Wallet address cleared.");
        }

        // Load back to welcome screen
        SceneManager.LoadScene("WelcomeMenu");
    }
}
