using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [Header("Main Menu UI Header")]
    [SerializeField] Canvas Canvas_MainMenu;
    private CanvasGroup cvg_mainmenu => Canvas_MainMenu.GetComponent<CanvasGroup>();

    [Header("Main Menu UI Header")]
    [SerializeField] Canvas Canvas_Gameplay;
    private CanvasGroup cvg_gmply => Canvas_Gameplay.GetComponent<CanvasGroup>();
    [SerializeField] private RectTransform healthUI;
    [SerializeField] private RectTransform abilityUI;
    [SerializeField] private RectTransform objectiveUI;
    [SerializeField] private TextMeshProUGUI objectiveTask;
    [SerializeField] private TextMeshProUGUI newsTask;

    [Header("Result UI")]
    [SerializeField] private Canvas Canvas_Result;
    [SerializeField] private CanvasGroup winState;
    [SerializeField] private CanvasGroup loseState;
    
    [Header("Blackscreen")]
    [SerializeField] private Canvas Canvas_BS;
    private CanvasGroup cvg_bs => Canvas_BS.GetComponent<CanvasGroup>();

    private void Awake()
    {
        healthUI.DOAnchorPosY(-150f, 0);
        abilityUI.DOAnchorPosX(-150f, 0);
        objectiveUI.DOAnchorPosY(240, 0);
    }
    private void Start()
    {
        Canvas_Result.gameObject.SetActive(false);
        winState.gameObject.SetActive(false);
        loseState.gameObject.SetActive(false);
        Invoke(nameof(HideMainMenu), 2f);
    }
    public void ChangeInterractableMainMenu(bool value) => cvg_mainmenu.interactable = value;
    public IEnumerator HideMainMenu()
    {
        cvg_mainmenu.DOFade(0, 1.6f);
        yield return new WaitForSeconds(0.3f);
    }
    public IEnumerator DisplayGameplay()
    {
        healthUI.DOAnchorPosY(15, 0.8f).SetEase(Ease.OutBack);
        abilityUI.DOAnchorPosX(15, 0.8f).SetEase(Ease.OutBack).SetDelay(0.2f);
        objectiveUI.DOAnchorPosY(-24, 0.8f).SetEase(Ease.OutBack).SetDelay(0.33f);
        yield return new WaitForSeconds(1.5f);
    }
    public void HideGameplay()
    {
        cvg_gmply.alpha = 0;
        cvg_gmply.interactable = false;
    }
    public void UpdateTask(string task)
    {
        objectiveTask.text = task;
    }
    public void EffectUpdatedTask()
    {
        objectiveTask.transform.DOScale(2f, 0.25f).OnComplete(() =>
        {
            objectiveTask.transform.DOScale(1f, 0.25f).SetDelay(0.15f);
        });
        objectiveTask.DOColor(Color.green, 0.25f).OnComplete(() =>
        {
            objectiveTask.DOColor(Color.white, 0.25f).SetDelay(0.15f);
        });
    }
    public IEnumerator DisplayState(bool win)
    {
        Canvas_Result.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        if (win)
        {
            winState.alpha = 1;
            winState.interactable = false;
            winState.gameObject.SetActive(true);
            winState.transform.localScale = Vector3.one * 0.05f;
            winState.transform.DOScale(1f, 0.22f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                winState.interactable = true;
            });
        }
        else
        {
            loseState.alpha = 1;
            loseState.interactable = false;
            loseState.gameObject.SetActive(true);
            loseState.transform.localScale = Vector3.one * 0.05f;
            loseState.transform.DOScale(1f, 0.22f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                loseState.interactable = true;
            });
        }
    }
    public IEnumerator DisplayNews()
    {
        newsTask.gameObject.SetActive(true);
        newsTask.alpha = 1;
        yield return new WaitForSeconds(1f);
        yield return newsTask.DOFade(0, 0.75f);
    }
    public IEnumerator DisplayBlackScreen()
    {
        Canvas_BS.gameObject.SetActive(true);
        cvg_bs.alpha = 0;
        yield return cvg_bs.DOFade(1, 1.2f);
    }
}
