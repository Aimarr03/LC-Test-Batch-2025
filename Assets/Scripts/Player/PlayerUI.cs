using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [Header("Dash")]
    [SerializeField] private Image DashIcon;
    [SerializeField] private Image DashProgressCooldown;
    [Header("Health")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBar;

    private float dashCooldown;
    private bool dashIconInvoked = false;
    void Start()
    {
        
    }

    public void DashInvoke(float dashInvoke)
    {
        this.dashCooldown = dashInvoke;
        StartCoroutine(DashCooldownHandling());
    }
    public void DashUnavailable()
    {
        if (dashIconInvoked) return;
        dashIconInvoked = true;
        DashIcon.GetComponent<RectTransform>().DOShakeAnchorPos(0.35f, new Vector2(13, 0),30, 90, false, false).OnComplete(() =>
        {
            dashIconInvoked=false;
        });
    }
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        healthText.text = $"{currentHealth}/{maxHealth}";
        healthBar.fillAmount = ((float)currentHealth) / maxHealth;
    }
    IEnumerator DashCooldownHandling()
    {
        float currentDuration = 0;
        while (currentDuration < dashCooldown)
        {
            DashProgressCooldown.fillAmount = currentDuration/dashCooldown;
            currentDuration += Time.deltaTime;
            yield return null;
        }
    }
    
}
