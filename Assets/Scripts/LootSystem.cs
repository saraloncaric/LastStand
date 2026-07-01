using UnityEngine;

public class LootSystem : MonoBehaviour
{
    [SerializeField] int coinsReward = 3;

    public void GiveLoot()
    {
        if (EconomyManager.Instance == null)
            return;

        EconomyManager.Instance.AddCoins(coinsReward);
    }
}
