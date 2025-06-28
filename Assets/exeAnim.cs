using UnityEngine;

public class exeAnim : MonoBehaviour
{
    public float interactionRadius = 0.5f;
    private PlayerMovement player;

    private void Awake()
    {
        player = FindObjectOfType<PlayerMovement>();
        if (player == null)
            Debug.LogWarning("PlayerMovement не знайдено!");
    }

    public void TryHitTree()
    {
        Vector2 center = transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, interactionRadius);
        foreach (var hit in hits)
        {
            Plant plant = hit.GetComponent<Plant>();
            if (plant != null)
            {
                plant.Interact();

                if (player != null)
                    player.AddAnger(1);

                break; // тільки перша взаємодія
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
