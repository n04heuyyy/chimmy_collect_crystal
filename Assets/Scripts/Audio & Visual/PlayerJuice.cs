using UnityEngine;

public class PlayerJuice : MonoBehaviour
{
    private Transform playerTransform;
    private Vector3 originalScale;
    private Vector3 lastPosition;

    [Header("Banking/Tilting Settings")]
    public float maxTiltAngle = 12f;
    public float tiltSpeed = 8f;

    [Header("Squash&Stretch")]
    public float stretchFactor = 0.05f;
    public float returnSpeed = 10f;

    void Start()
    {
        playerTransform = transform;
        originalScale = playerTransform.localScale;
        lastPosition = playerTransform.position;
    }

    void Update()
    {
        if (PlayerController.instance != null && !PlayerController.instance.canMove)
        {
            playerTransform.localScale = Vector3.Lerp(playerTransform.localScale, originalScale, returnSpeed * Time.deltaTime);
            playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.identity, tiltSpeed * Time.deltaTime);
            return;
        }

        Vector3 currentPosition = playerTransform.position;
        Vector3 velocity = (currentPosition - lastPosition) / Time.deltaTime;
        lastPosition = currentPosition;

        float moveVelocityX = velocity.x;

        // Ini banking/dynamic tilting (Sprite dimiringkan berdasarkan arah horizontal)
        // Kalau ke kanan miring ke kiri dsb.
        float targetTilt = -Mathf.Clamp(moveVelocityX * 0.5f, -maxTiltAngle, maxTiltAngle);
        
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetTilt);
        playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, tiltSpeed * Time.deltaTime);

        // Ini squash & stretch (Sprite meregang ketika bergerak cepat)
        float speedMagnitude = velocity.magnitude;
        Vector3 targetScale = originalScale;

        if (speedMagnitude > 0.8f)
        {
            // Perubahan skala dengan mathf/clamp minimal agar stabil
            float squashAmount = Mathf.Clamp(Mathf.Abs(moveVelocityX) * stretchFactor, 0f, 0.1f);
            targetScale.x = originalScale.x + squashAmount;
            targetScale.y = originalScale.y - squashAmount;
        }
        else
        {
            targetScale = originalScale;
        }

        playerTransform.localScale = Vector3.Lerp(playerTransform.localScale, targetScale, returnSpeed * Time.deltaTime);
    }
}