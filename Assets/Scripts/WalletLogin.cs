using UnityEngine;
using UnityEngine.SceneManagement;

public class WalletLogin : MonoBehaviour
{
    [SerializeField] private PlayerWalletData walletData;

    public void Login(string walletAddress)
    {
        walletData.walletAddress = walletAddress;

        // Loading main menu
        SceneManager.LoadScene("MainMenu");
    }
}
