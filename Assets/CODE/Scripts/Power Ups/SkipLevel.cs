using UnityEngine;

[CreateAssetMenu(fileName = "New Skip Level")]
public class SkipLevel : PowerUpSO
{
    public override void UsePowerUp()
    {
        if (GameManager.Instance.RemoveCoins(Cost))
        {
            GameManager.Instance.UpdateNextLevel();
            GameManager.Instance.Restart();
        }
    }
}