using UnityEngine;
using UnityEngine.EventSystems;

public class JuicyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData = null)
    {
        // Kalau hover membesar sedikit
        StopAllCoroutines();
        StartCoroutine(ScaleRoutine(originalScale * 1.1f, 0.1f));
    }

    public void OnPointerExit(PointerEventData eventData = null)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleRoutine(originalScale, 0.1f));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Memipih sedikit saat ditekan
        StopAllCoroutines();
        StartCoroutine(ScaleRoutine(originalScale * 0.9f, 0.05f));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleRoutine(originalScale * 1.1f, 0.05f));
    }

    System.Collections.IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}