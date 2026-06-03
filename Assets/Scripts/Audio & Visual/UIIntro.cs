using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIIntro : MonoBehaviour
{
    public CanvasGroup scoreGroup; // Ada 2 Canvas Group yang berisi BG dan teks
    public CanvasGroup hpGroup;    

    void Start()
    {
        // Buat semua grup transparan
        scoreGroup.alpha = 0;
        hpGroup.alpha = 0;

        // Tentukan posisi muncul kotak UI
        scoreGroup.transform.localPosition += new Vector3(0, 50, 0);
        hpGroup.transform.localPosition += new Vector3(0, 50, 0);

        StartCoroutine(ShowUI());
    }

    IEnumerator ShowUI()
    {
        // 1. Kunci posisi akhir kotak UI
        Vector3 scoreTargetPos = scoreGroup.transform.localPosition;
        Vector3 hpTargetPos = hpGroup.transform.localPosition;

        // 2. Tentukan posisi sembunyi kotak UI 
        Vector3 scoreStartPos = scoreTargetPos + new Vector3(0, 100, 0);
        Vector3 hpStartPos = hpTargetPos + new Vector3(0, 100, 0);

        // 3. Set ke posisi mulai dan transparan sebelum muncul
        scoreGroup.transform.localPosition = scoreStartPos;
        hpGroup.transform.localPosition = hpStartPos;
        scoreGroup.alpha = 0;
        hpGroup.alpha = 0;

        yield return new WaitForSeconds(0.2f); // Delay singkat

        float duration = 0.8f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / duration;
            // Gunakan SmoothStep agar gerakan melambat saat mau sampai
            float t = Mathf.SmoothStep(0, 1, ratio);

            // Update posisi dan transparansi
            scoreGroup.transform.localPosition = Vector3.Lerp(scoreStartPos, scoreTargetPos, t);
            hpGroup.transform.localPosition = Vector3.Lerp(hpStartPos, hpTargetPos, t);
            
            scoreGroup.alpha = t;
            hpGroup.alpha = t;

            yield return null;
        }
        // Pastikan tepat di posisi target
        scoreGroup.transform.localPosition = scoreTargetPos;
        hpGroup.transform.localPosition = hpTargetPos;
    }
}