using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject loadingPanel;
    public TextMeshProUGUI highScoreText;
    private RectTransform loadingRect;
    private Tweener tweener;
    
    public bool firstLoad = true;
    
    private static UIManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        tweener = GetComponent<Tweener>();

        loadingRect = loadingPanel.GetComponent<RectTransform>();
        loadingPanel.SetActive(true);
        loadingRect.sizeDelta = new Vector2(Screen.width, Screen.height);
                
        HideLoadingScreen();
        
        LoadHighScoreAndTime();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void LoadHighScoreAndTime()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        float highScoreTime = PlayerPrefs.GetFloat("HighScoreTime", 0f);
        
        TimeSpan timeSpan = TimeSpan.FromSeconds(highScoreTime);
        string timeFormatted = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        highScoreText.text = "High Score: " + highScore + "\n" + "Time: " + timeFormatted;
        //print(highScoreText.text);
    }
    
    private void HideLoadingScreen()
    {
        Vector2 hiddenPosition = new Vector2(0, -Screen.height);
        tweener.AddTween(loadingRect, loadingRect.anchoredPosition, hiddenPosition, 0.5f);
    }

    public void LoadFirstLevel()
    {
        StartCoroutine(ShowLoadingAndLoadScene("MainGameScene", true));
    }
    
    public void LoadSecondLevel()
    {
        StartCoroutine(ShowLoadingAndLoadScene("Level2GameScene", true));
    }
    
    public void ShowLoadingScreen()
    {
        tweener.AddTween(loadingRect,loadingRect.anchoredPosition, new Vector3(0,0,0),0.5f);
    }
    
    private IEnumerator ShowLoadingAndLoadScene(string sceneName, bool showLoadingScreen)
    {
        //print("Load:" + sceneName);
        // Show the Loading Screen (lerp into position)
        if (showLoadingScreen)
        {
            ShowLoadingScreen();
        }

        // Wait for 1 second
        yield return new WaitForSeconds(1f);

        // Begin loading the scene asynchronously
        SceneManager.sceneLoaded += OnSceneLoaded;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        if (firstLoad)
        {
            DontDestroyOnLoad(this.gameObject); // Keep UIManager persistent between scenes
            firstLoad = false;
        }

        //Wait until the scene has finished loading
        while (!asyncLoad.isDone)
        {
            yield return null; // Wait for the next frame
        }

        // Step 5: After the scene has loaded, wait for 1 more second
        yield return new WaitForSeconds(1f);

        // Step 6: Hide the Loading Screen (lerp out of position)
        HideLoadingScreen();
    }
    
    public void QuitGame()
    {
        StartCoroutine(ShowLoadingAndLoadScene("StartScene", true));
    }
    
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGameScene" || scene.name == "Level2GameScene")
        {
            GameObject quitButtonObject = GameObject.FindGameObjectWithTag("QuitButton");
            if (quitButtonObject != null)
            {
                Button quitButton = quitButtonObject.GetComponent<Button>();
                
                quitButton.onClick.RemoveListener(QuitGame);
                quitButton.onClick.AddListener(QuitGame);
                
            }
        }

        if (scene.name == "StartScene")
        {
            GameObject highScoreTextObject = GameObject.Find("HighScoreText");
            highScoreText = highScoreTextObject.GetComponent<TextMeshProUGUI>();
            
            LoadHighScoreAndTime();
            
            GameObject buttonObject = GameObject.FindGameObjectWithTag("Level1Button");
            if (buttonObject != null)
            {
                Button quitButton = buttonObject.GetComponent<Button>();
                
                quitButton.onClick.RemoveListener(LoadFirstLevel);
                quitButton.onClick.AddListener(LoadFirstLevel);
            }
            
            buttonObject = GameObject.FindGameObjectWithTag("Level2Button");
            if (buttonObject != null)
            {
                Button quitButton = buttonObject.GetComponent<Button>();
                
                quitButton.onClick.RemoveListener(LoadSecondLevel);
                quitButton.onClick.AddListener(LoadSecondLevel);
            }
        }
        
    }
}
