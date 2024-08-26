using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class eyeBlink : MonoBehaviour
{   public GameObject text;
    private Image[] images;
    private bool isBlinking = false;
    private RectTransform rectTransform;
    private Coroutine blinkCoroutine = null;
    private Gradient gradient;
    // Start is called before the first frame update
    void Start()
    {
        images = GetComponentsInChildren<Image>();
        rectTransform = GetComponent<RectTransform>();
        gradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[3];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];

        colorKey[0].color = Color.white;
        colorKey[0].time = 0;
        colorKey[0].color = Color.yellow;
        colorKey[0].time = 0.5f;
        colorKey[0].color = Color.red;
        colorKey[0].time = 1.0f;
        for(int i = 0; i < 3; i++)
        {
            alphaKey[i].alpha = 1.0f;
            alphaKey[i].time = colorKey[i].time;
        }
        gradient.SetKeys(colorKey, alphaKey);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
   public void UpdateEye(float value,bool left)
    {
        Debug.Log("value"+value+")");
        images[1].fillAmount = value;
        Color newColor0 = images[0].color;
        Color newColor2 = images[2].color;
        Color newTextColor = gradient.Evaluate(value);

        newColor0.a = value;
        newColor2.a = value;
        newTextColor.a = value;

        images[0].color = newColor0;
        images[2].color = newColor2;
        text.GetComponent<TMP_Text>().color = newTextColor;

        if (left) { text.GetComponent<TMP_Text>().text = "Looking at the left"; } 
        else
        {
            text.GetComponent<TMP_Text>().text = "Looking at the right";
        }
            
        if (value == 1f && !isBlinking)
        {
            // Start blinking only if it's not already blinking
            isBlinking = true;
            blinkCoroutine = StartCoroutine(Blink());
        }
        else if (value != 1f && isBlinking)
        {
            // Stop blinking if the slider value is not 1 and it's currently blinking
            StopBlinking();
        }
    }
    void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        rectTransform.localScale = Vector3.one; // Reset scale
        isBlinking = false;
    }
    IEnumerator Blink()
    {
        float closeDuration = 0.2f; // Adjust this value to control how fast the eye closes
        float openDuration = 0.3f; // Adjust this value to control how fast the eye opens

        float blinkScale = 0.1f;
        Vector3 originalScale = rectTransform.localScale;
        Vector3 targetScale = new Vector3(originalScale.x, blinkScale, originalScale.z);

        while (isBlinking)
        {
            // Close the eye
            for (float elapsedTime = 0; elapsedTime < closeDuration && isBlinking; elapsedTime += Time.deltaTime)
            {
                rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / closeDuration);
                yield return null;
            }


            // Open the eye
            for (float elapsedTime = 0; elapsedTime < openDuration && isBlinking; elapsedTime += Time.deltaTime)
            {
                rectTransform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / openDuration);
                yield return null;
            }

            yield return new WaitForSeconds(1f); // 1-second gap between opening and the next blink
        }

        rectTransform.localScale = originalScale;
        blinkCoroutine = null; // Reset coroutine reference after completion
    }
}
