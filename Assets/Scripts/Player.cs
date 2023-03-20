using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private Rigidbody2D playerRigidbody;
    private SpriteRenderer spriteRenderer;
    
    public float speed = 3f;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (IsOwner)
        {
            spriteRenderer.color = Color.blue;
        }
        else
        {
            spriteRenderer.color = Color.red;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        var input = Input.GetAxis("Vertical");
        
        var distance = input * speed * Time.deltaTime;
        var targetPosition = transform.position + Vector3.up * distance;
        
        playerRigidbody.MovePosition(targetPosition);
    }
}
