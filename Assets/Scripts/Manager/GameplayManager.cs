using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    CameraManager cameraManager;
    CanvasManager canvasManager;
    EffectManager effectManager;
    [SerializeField] private PlayerController playerController;
    public static GameplayManager instance { get; private set; }
    public bool paused { get; private set; }
    private List<EnemyController> enemies;
    private int defeatedEnemies = 0;
    private void Awake()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        defeatedEnemies = 0;

        enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None).ToList();

        cameraManager = GetComponent<CameraManager>();
        canvasManager = GetComponent<CanvasManager>();
        effectManager = GetComponent<EffectManager>();
        
        paused = true;
        canvasManager.UpdateTask($"Defeat Goblin {defeatedEnemies}/{enemies.Count}");
        PlayerController.PlayerDead += PlayerController_PlayerDead;
        EnemyController.Defeated += EnemyController_Defeated;
    }
    private void Start()
    {
        playerController.ChangeEnabilityInput(false);
        StartCoroutine(AudioManager.Instance.AssignNewMusic("bgm-mainmenu"));
    }
    private void OnDestroy()
    {
        PlayerController.PlayerDead -= PlayerController_PlayerDead;
        EnemyController.Defeated -= EnemyController_Defeated;
    }

    private void PlayerController_PlayerDead()
    {
        paused = true;
        playerController.ChangeEnabilityInput(false);
        canvasManager.HideGameplay();
        StartCoroutine(OnLose());
    }
    
    private void EnemyController_Defeated()
    {
        defeatedEnemies++;
        canvasManager.UpdateTask($"Defeat Goblin {defeatedEnemies}/{enemies.Count}");
        if (defeatedEnemies == enemies.Count)
        {
            paused = true;
            playerController.ChangeEnabilityInput(false);
            canvasManager.HideGameplay();
            effectManager.FreezeHit(0.5f);
            AudioManager.Instance.StopMusic();
            StartCoroutine(OnWin());
        }
        else
        {
            StartCoroutine(canvasManager.DisplayNews());
            canvasManager.EffectUpdatedTask();
        }
    }

    public void StartGame()
    {
        canvasManager.ChangeInterractableMainMenu(false);
        StartCoroutine(StartSequencePlayingGame());
    }
    public void QuitGame()
    {
        Debug.Log("Quiting");
        StartCoroutine(Quit());
    }
    private IEnumerator StartSequencePlayingGame()
    {
        AudioManager.Instance.PlayAudio("sfx-click");
        StartCoroutine(AudioManager.Instance.StopMusicWithFade(0.6f));
        yield return canvasManager.HideMainMenu();
        yield return new WaitForSeconds(0.3f);
        yield return cameraManager.DisplayObjective();
        yield return new WaitForSeconds(0.3f);
        yield return canvasManager.DisplayGameplay();
        StartCoroutine(AudioManager.Instance.AssignNewMusic("bgm-gameplay"));
        paused = false;
        playerController.ChangeEnabilityInput(true);
    }
    IEnumerator OnLose()
    {
        StartCoroutine(effectManager.DisplayLoseEffect());
        yield return cameraManager.ZoomInPlayer(5f);
        yield return new WaitForSeconds(3.5f);
        yield return canvasManager.DisplayState(false);
    }
    IEnumerator OnWin()
    {
        yield return cameraManager.ZoomInPlayer();
        yield return canvasManager.DisplayState(true);
    }
    public void RestartGame()
    {
        StartCoroutine(Restart());
    }
    IEnumerator Quit()
    {
        Debug.Log("Display Quit Sequence");
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayAudio("sfx-cancel");
        yield return canvasManager.DisplayBlackScreen();
        yield return new WaitForSeconds(1.5f);
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
    IEnumerator Restart()
    {
        AudioManager.Instance.PlayAudio("sfx-click");
        yield return canvasManager.DisplayBlackScreen();
        SceneManager.LoadScene(0);
    }

    
}
