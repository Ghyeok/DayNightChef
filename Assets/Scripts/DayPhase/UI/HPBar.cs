using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    private Image hpBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpBar = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        hpBar.fillAmount = DayPhasePlayerManager.Instance.playerCurHP / DayPhasePlayerManager.Instance.playerMaxHP;
    }
}
