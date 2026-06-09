using UnityEngine;

public class LootSystem : MonoBehaviour
{
    [SerializeField] int coinsReward = 4;

    public void GiveLoot()
    {
        if (EconomyManager.Instance == null)
            return;

        EconomyManager.Instance.coins += coinsReward;
    }
}
