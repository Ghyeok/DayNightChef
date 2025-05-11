using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Weight : MonoBehaviour
{
    private TextMeshProUGUI weightText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weightText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        weightText.text = $"{DayPhasePlayerManager.Instance.curBagWeight}" + " / " + $"{DayPhasePlayerManager.Instance.maxBagWeight}";
    }
}
