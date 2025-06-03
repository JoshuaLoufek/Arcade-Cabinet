using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=11ofnLOE8pw

// Terminology: 
    // A Route is a full trajectory that an object will take from start to finish.
    // Routes are made up of multiple connecting Paths

public class Path : MonoBehaviour
{
    [SerializeField] private Transform[] controlPoints;
    private Vector2 gizmosPosition;
    public bool disableAllGizmos;
    public bool disableDottedLine;
    public bool disableSolidLines;
    public bool disableChildSprites;

    // This is purely for visuals within the editor. Draws the lines between the control points and the dots for the path.
    private void OnDrawGizmos()
    {
        // An exit clause so that the gizmo lines can be disabled from the editor
        if (disableAllGizmos)
        {
            DisableChildSprites(false);
            return;
        }

        if (disableChildSprites) DisableChildSprites(false);
        else DisableChildSprites(true);

        for (float t = 0; t <= 1f; t += 0.05f)
        {
            if (disableDottedLine) break;

            gizmosPosition = Mathf.Pow(1 - t, 3) * controlPoints[0].position +
                3 * Mathf.Pow(1 - t, 2) * t * controlPoints[1].position +
                3 * (1 - t) * Mathf.Pow(t, 2) * controlPoints[2].position +
                Mathf.Pow(t, 3) * controlPoints[3].position;

            Gizmos.DrawSphere(gizmosPosition, 0.25f);
        }

        if (disableSolidLines) return;

        Gizmos.DrawLine(new Vector2(controlPoints[0].position.x, controlPoints[0].position.y),
            new Vector2(controlPoints[1].position.x, controlPoints[1].position.y));

        Gizmos.DrawLine(new Vector2(controlPoints[2].position.x, controlPoints[2].position.y),
            new Vector2(controlPoints[3].position.x, controlPoints[3].position.y));
    }

    private void DisableChildSprites(bool activationState)
    {
        this.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = activationState;
        this.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = activationState;
        this.transform.GetChild(2).GetComponent<SpriteRenderer>().enabled = activationState;
        this.transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = activationState;
    }
}
