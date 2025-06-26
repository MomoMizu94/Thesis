using UnityEngine;

[CreateAssetMenu(fileName = "PlayerWalletData", menuName = "Scriptable Objects/PlayerWalletData")]

public class PlayerWalletData : ScriptableObject
{
    public string walletAddress;
    public string privateKey;
}
