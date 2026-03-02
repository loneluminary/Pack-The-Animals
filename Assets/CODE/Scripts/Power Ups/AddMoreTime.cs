using UnityEngine;

[CreateAssetMenu(fileName = "New Add More Time")]
public class AddMoreTime : PowerUpSO
{
    public int MoreTime;
    
    public override void UsePowerUp()
    {
        if (GameManager.Instance.RemoveCoins(Cost))
        {
            GameManager.Instance.Timer += MoreTime; // Adds up MoreTime seconds to the timer.
        }
    }
}