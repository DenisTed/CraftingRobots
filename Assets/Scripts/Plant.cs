using UnityEngine;

public class Plant : MonoBehaviour
{
    public string plantName;
    public int hp = 1;
    public Animator animator;

    public void Interact()
    {
        if (hp > 0)
        {
            hp--;
            if (animator != null)
                animator.SetTrigger("Hit");

            if (hp <= 0)
                Destroy(gameObject); // або анімацію зникнення
        }
    }
}
