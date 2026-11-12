using UnityEngine;

public class RisingWater : MonoBehaviour
{
    [Header("Movement Settings")]
    public float riseSpeed = 2.0f; 
    public bool isRising = false;  

    [Header("Damage Settings")]
    public int damageAmount = 3;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (isRising)
        {
            transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }

    public void ResetWater()
    {
        isRising = false;
        transform.position = startPosition;
    }
}