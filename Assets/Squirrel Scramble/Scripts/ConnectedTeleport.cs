using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedTeleport : MonoBehaviour
{
    [SerializeField] ConnectedTeleport reciever;
    bool isActive = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Squirrel squirrel = collision.gameObject.GetComponent<Squirrel>();
        if (squirrel != null && isActive)
        {
            reciever.Deactivate();
            squirrel.transform.localPosition = reciever.transform.localPosition;
            this.Deactivate();
        }
    }
    
    public void Deactivate()
    {
        isActive = false;
    }

    public void Activate()
    {
        isActive = true;
    }
}
