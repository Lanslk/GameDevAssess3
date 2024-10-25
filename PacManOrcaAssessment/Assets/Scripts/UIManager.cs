using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public RectTransform loadingPanel;
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

        loadingPanel.sizeDelta = new Vector2(Screen.width, Screen.height);
        
        HideLoadingScreen();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void HideLoadingScreen()
    {
        Vector2 hiddenPosition = new Vector2(0, -Screen.height);
        tweener.AddTween(loadingPanel, loadingPanel.anchoredPosition, hiddenPosition, 0.5f);
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
        tweener.AddTween(loadingPanel,loadingPanel.anchoredPosition, new Vector3(0,0,0),0.5f);
    }
    
    private IEnumerator ShowLoadingAndLoadScene(string sceneName, bool showLoadingScreen)
    {
        print("Load:" + sceneName);
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
        //UnityEditor.EditorApplication.isPlaying = false;
        //Application.Quit();

        
        
        StartCoroutine(ShowLoadingAndLoadScene("StartScene", true));
        //SceneManager.sceneLoaded += OnSceneLoaded;
        //AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("StartScene", LoadSceneMode.Single);
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
