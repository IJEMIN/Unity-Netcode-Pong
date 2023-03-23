using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerControl : NetworkBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private NetworkTransform _networkTransform;
    
    public float speed = 5f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _networkTransform = GetComponent<NetworkTransform>();
    }
    
    public override void OnNetworkSpawn()
    {
        // set color blue if player is local player and red if player is remote player
        _spriteRenderer.color = IsOwner ? Color.blue : Color.red;
    }
    
    [ClientRpc]
    public void SetRenderActiveClientRpc(bool active)
    {
        _spriteRenderer.enabled = active;
    }
    
    [ClientRpc]
    public void SpawnToPositionClientRpc(Vector3 position)
    {
        transform.position = position;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive)
        {
            return;
        }
        
        if (!IsOwner)
        {
            return;
        }
        
        var input = Input.GetAxis("Vertical");
        
        var distance = input * speed * Time.deltaTime;
        var position = transform.position;
        position.y += distance;
        position.y = Mathf.Clamp(position.y, -4.5f, 4.5f);
        transform.position = position;
    }
}
