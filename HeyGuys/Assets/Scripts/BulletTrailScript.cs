using UnityEngine;

public class BulletTrailScript : MonoBehaviour
{
    public Vector3 startPoint;
    public Vector3 endPoint;
    public LineRenderer lineRenderer;
    public float fadeSpeed;

    void Start()
    {
        lineRenderer.SetPositions(new Vector3[2] { startPoint, endPoint });
    }

    void Update()
    {
        lineRenderer.startColor = new Color(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a - fadeSpeed * Time.deltaTime);
        lineRenderer.endColor = new Color(lineRenderer.endColor.r, lineRenderer.endColor.g, lineRenderer.endColor.b, lineRenderer.endColor.a - fadeSpeed * Time.deltaTime);
        if (lineRenderer.startColor.a <= 0 && lineRenderer.endColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
