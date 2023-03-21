using Unity.Netcode;
using UnityEngine;

public class PlayerControl : NetworkBehaviour
{
    private SpriteRenderer _spriteRenderer;
    
    public float speed = 3f;
    private bool _controlActive;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public override void OnNetworkSpawn()
    {
        // set color blue if player is local player and red if player is remote player
        _spriteRenderer.color = IsOwner ? Color.blue : Color.red;
    }
    
    [ClientRpc]
    public void SetActiveControlClientRpc(bool active)
    {
        _spriteRenderer.enabled = active;
        _controlActive = active;
    }

    [ClientRpc]
    public void SpawnToPositionClientRpc(Vector3 position)
    {
        transform.position = position;
    }

    private void Update()
    {
        if (!_controlActive)
        {
            return;
        }
        
        if (!IsOwner)
        {
            return;
        }

        var input = Input.GetAxis("Vertical");
        
        var distance = input * speed * Time.deltaTime;
        transform.position += Vector3.up * distance;
    }
}
