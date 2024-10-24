using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public RectTransform loadingPanel;
    private Tweener tweener;
    
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
        StartCoroutine(ShowLoadingAndLoadScene("MainGameScene"));
    }
    
    public void LoadSecondLevel()
    {
        StartCoroutine(ShowLoadingAndLoadScene("Level2GameScene"));
    }
    
    public void ShowLoadingScreen()
    {
        tweener.AddTween(loadingPanel,loadingPanel.anchoredPosition, new Vector3(0,0,0),0.5f);
    }
    
    private IEnumerator ShowLoadingAndLoadScene(string sceneName)
    {
        // Show the Loading Screen (lerp into position)
        ShowLoadingScreen();

        // Wait for 1 second
        yield return new WaitForSeconds(1f);

        // Begin loading the scene asynchronously
        SceneManager.sceneLoaded += OnSceneLoaded;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        DontDestroyOnLoad(this.gameObject); // Keep UIManager persistent between scenes

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
        UnityEditor.EditorApplication.isPlaying = false;
        //if run in built application
        Application.Quit();
    }
    
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "WalkingScene")
        {
            GameObject quitButtonObject = GameObject.FindGameObjectWithTag("QuitButton");
            if (quitButtonObject != null)
            {
                Button quitButton = quitButtonObject.GetComponent<Button>();
                
                quitButton.onClick.AddListener(QuitGame);
            }
        }
    }
}
