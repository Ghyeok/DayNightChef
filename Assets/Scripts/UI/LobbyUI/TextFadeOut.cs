using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextFadeOut : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    float fadeRate = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        StartCoroutine(FadeText());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator FadeText()
    {
        while (true)
        {
            yield return StartCoroutine(Fade(0f, 0.5f)); // 투명 -> 선명

            yield return StartCoroutine(Fade(0.5f, 0f)); // 선명 -> 투명
        }
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0f;

        while (timer < fadeRate)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeRate); // 

            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }
    }
}
