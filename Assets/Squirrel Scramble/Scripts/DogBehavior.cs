using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Dog))]
public abstract class DogBehavior : MonoBehaviour
{
    public Dog dog { get; private set; }
    public float duration;

    private void Awake()
    {
        this.dog = GetComponent<Dog>();
    }

    public void Enable()
    {
        Enable(this.duration);
    }

    public virtual void Enable(float time) // THE REASON THE DOG STOPS SHORTLY AFTER STARTING IS BECAUSE OF THE DURATION
    {
        this.enabled = true;

        CancelInvoke(); // ensures that the old timer is destroyed if a new timer is created
        Invoke(nameof(Disable), duration);
    }

    public virtual void Disable()
    {
        this.enabled = false;
        CancelInvoke();
    }
}
