using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance { get; private set; }
    public TextMeshProUGUI scoreText;
    private int currentScore = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public int GetScore()
    {
        return currentScore;
    }

    public void AddScore(int points)
    {
        currentScore += points;
        scoreText.text = "Score: " + currentScore;
        Transform target = scoreText.transform.parent;
        StartCoroutine(PopAnimation(scoreText.transform.parent, 1.2f));
    }

    public IEnumerator PopAnimation(Transform target, float scaleAmount)
    {
        if (target == null) yield break;
        float duration = 0.2f; 
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float curve = Mathf.Sin((elapsed / duration) * Mathf.PI);
            float s = 1f + (curve * (scaleAmount - 1f));
            
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        
        target.localScale = Vector3.one;
    }
}