using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Extensions;

public class UIManager : Singleton<UIManager>
{
    [Title("Animations")] 
    [SerializeField] private Transform coinIcon;
    
    [Title("Screens")]
    [SerializeField] GameObject lossScreen;
    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject pauseScreen;

    [Title("UI Texts")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI coinText;
    [SerializeField] TextMeshProUGUI timerText;

    [Title("Toast Popup")]
    [SerializeField] private RectTransform toastPopupContainer;
    [SerializeField] private CanvasGroup toastPopupTemplate;

    [SerializeField] private AudioSource musicSource;
    
    private void Start()
    {
        if (musicSource)
        {
            if (FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length > 1) Destroy(musicSource.gameObject);
            else DontDestroyOnLoad(musicSource);
        }
        
        if (levelText)
        {
            int levelno = PlayerPrefs.GetInt(GameManager.LEVEL_PREFS_KEY);
            levelText.text = $"Level: {levelno}";
        }

        if (toastPopupTemplate)
        {
            toastPopupTemplate.alpha = 0f;
            toastPopupTemplate.gameObject.SetActive(false);
        }

        UpdateTimerText();
    }

    public void TogglePauseScreen(bool toggle)
    {
        Time.timeScale = toggle ? 0f : 1f;
        pauseScreen.SetActive(toggle);
    }
    
    public void LossScreen()
    {
        if (lossScreen.activeSelf) return;

        Time.timeScale = 0f;
        lossScreen.SetActive(true);
    }

    public void WinScreen()
    {
        if (winScreen.activeSelf) return;
        
        Time.timeScale = 0f;
        GameManager.Instance.UpdateNextLevel();
        winScreen.SetActive(true);
    }

    public void Play() => SceneManager.LoadScene(1);
    public void Quit() => Application.Quit();
    
    public void NoAdsAvailable() // UnUsed. 
    {
        ShowToastMessage("Not Enough Coins Available.");
        ShowToastMessage("No ads available right now.");
    }

    public void ShowToastMessage(string text)
    {
        if (!toastPopupContainer) return;
        
        var popup = Instantiate(toastPopupTemplate, toastPopupContainer);
        popup.GetComponentInChildren<TMP_Text>().text = text;
        
        popup.gameObject.SetActive(true);
        popup.DOFade(1f, 0.3f).OnComplete(() =>
        {
            popup.DOFade(0f, 0.3f).SetDelay(3f).OnComplete(() => Destroy(popup.gameObject));
        });
    }
    
    public void UpdateCoinsText(bool adding)
    {
        if (!coinText) return;
        
        coinText.text = GameManager.Instance.CurrentCoins.ToString();

        coinText.DOComplete();
        coinText.DOScale(1.1f, 0.2f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        if (adding) coinText.DOColor(Color.green, 0.2f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        else coinText.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
    }

    public void CoinsAddingAnimation(Vector3 startPos)
    {
        startPos = Camera.main!.WorldToScreenPoint(startPos).WithZ(0f);
        
        for (int i = 0; i < 7; i++)
        {
            var coin = Instantiate(coinIcon, startPos, Quaternion.identity, coinIcon.root);
            coin.gameObject.SetActive(true);
            coin.localScale = Vector3.zero;

            var random = new Vector3(Random.Range(-80f, 80f), Random.Range(-100f, 100f));

            var seq = DOTween.Sequence().OnComplete(() => Destroy(coin.gameObject)).SetUpdate(true);
            seq.Append(coin.DOScale(1f, 0.5f).SetEase(Ease.OutQuad));
            seq.Join(coin.DOMove(startPos + random, 0.5f).SetEase(Ease.InSine));
            seq.Append(coin.DOMove(coinText.transform.position.WithZ(0f), 0.5f).SetEase(Ease.InQuad));
        }
    }

    public void UpdateTimerText()
    {
        if (!timerText) return;

        if (GameManager.Instance.Timer <= 0)
        { 
            timerText.text = "Time: " + "00:00";
            return;
        }
        
        int minutes = Mathf.FloorToInt(GameManager.Instance.Timer / 60f);
        int seconds = Mathf.FloorToInt(GameManager.Instance.Timer % 60f);
        timerText.text = "Time: " + $"{minutes:00}:{seconds:00}";
    }
}