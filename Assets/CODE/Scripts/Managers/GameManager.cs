using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public float Timer;
    
    [Title("Coins")]
    [SerializeField] int startingCoins = 100;
    public int CurrentCoins;

    public const string LEVEL_PREFS_KEY = "LevelNumber";
    public const string COINS_PREFS_KEY = "Coins";
    
    private void Awake()
    {
        if (!PlayerPrefs.HasKey(LEVEL_PREFS_KEY))
        {
            AddCoins(startingCoins);
            PlayerPrefs.SetInt(LEVEL_PREFS_KEY, 1);
        }
        else
        {
            Timer += PlayerPrefs.GetInt(LEVEL_PREFS_KEY) * 10;
            AddCoins(PlayerPrefs.GetInt(COINS_PREFS_KEY));
        }
    }
    
    private void Update()
    {
        if (Timer <= 0)
        {
            UIManager.Instance.LossScreen();
            return;
        }
        
        Timer -= Time.deltaTime;
        UIManager.Instance.UpdateTimerText();
    }
    
    public void AddCoins(int amount)
    {
        CurrentCoins += amount;
        PlayerPrefs.SetInt(COINS_PREFS_KEY, CurrentCoins);
        
        UIManager.Instance.UpdateCoinsText(true);
    }
    
    public bool RemoveCoins(int amount)
    {
        if (CurrentCoins < amount)
        {
            UIManager.Instance.ShowToastMessage("Not Enough Coins Available.");
            return false;
        }
        
        CurrentCoins -= amount;
        PlayerPrefs.SetInt(COINS_PREFS_KEY, CurrentCoins);
        
        UIManager.Instance.UpdateCoinsText(false);

        return true;
    }
    
    public void UpdateNextLevel()
    {
        PlayerPrefs.SetInt(LEVEL_PREFS_KEY, PlayerPrefs.GetInt(LEVEL_PREFS_KEY, 1) + 1);
    }
    
    public void Restart()
    {
        Time.timeScale = 1f;
        
        UIManager.Instance.TogglePauseScreen(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 0f;
        
        UIManager.Instance.TogglePauseScreen(false);
        SceneManager.LoadScene(0);
    }

    public bool IsCompleted()
    {
        return !FindAnyObjectByType<Animal>() && !FindAnyObjectByType<Box>();
    }
}