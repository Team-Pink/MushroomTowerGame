using UnityEngine;

public class TransformBounce : MonoBehaviour
{
    private enum TransformVector
    {
        Hover,
        Pulse
    }

    private enum HoverAxis
    {
        x,
        y,
        z
    }

    [SerializeField] TransformVector bounceType;
    [SerializeField] HoverAxis hoverAxis;

    [SerializeField] float intensity;
    [SerializeField] float speed;
    private float startingHover;
    private Vector3 startingPulse;

    private void Awake()
    {
        switch (bounceType)
        {
            case TransformVector.Hover:
                switch (hoverAxis)
                {
                    case HoverAxis.x:
                        startingHover = transform.position.x;
                        break;
                    case HoverAxis.y:
                        startingHover = transform.position.y;
                        break;
                    case HoverAxis.z:
                        startingHover = transform.position.z;
                        break;
                }
                break;
            case TransformVector.Pulse:
                startingPulse = transform.localScale;
                break;
        }
    }

    private void Update()
    {
        switch (bounceType)
        {
            case TransformVector.Hover:
                switch (hoverAxis)
                {
                    case HoverAxis.x:
                        transform.position = new(startingHover + Mathf.Sin(Time.deltaTime * speed) * intensity, transform.position.y, transform.position.z);
                        break;
                    case HoverAxis.y:
                        transform.position = new(transform.position.x, startingHover + Mathf.Sin(Time.time * speed) * intensity, transform.position.z);
                        break;
                    case HoverAxis.z:
                        transform.position = new(transform.position.x, transform.position.y, startingHover + Mathf.Sin(Time.time * speed) * intensity);
                        break;
                }
                break;
            case TransformVector.Pulse:
                transform.localScale = new(
                    startingPulse.x*1.1f + Mathf.Sin(Time.time * speed) * intensity,
                    startingPulse.y*1.1f + Mathf.Sin(Time.time * speed) * intensity,
                    startingPulse.z*1.1f + Mathf.Sin(Time.time * speed) * intensity);
                break;
        }
    }
}