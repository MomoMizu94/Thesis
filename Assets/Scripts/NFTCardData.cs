using UnityEngine;

[System.Serializable]
public class NFTCardData
{
    public string name;
    public string description;
    public int attack;
    public int health;
    public int mana;
    public string imageURI;
    public string ownerAddress;

    public System.Numerics.BigInteger tokenId;
}
