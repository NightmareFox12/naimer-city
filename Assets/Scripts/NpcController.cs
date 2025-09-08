using UnityEngine;

public class NpcController : MonoBehaviour
{


    void Awake()
    {

    }
    void Start()
    {

    }

    void Update()
    {

    }

    public float rayOriginHeight = 0.8f;
    public RaycastHit lastRayHit; // Info del último impacto
    public LayerMask rayLayers = ~0; // Por defecto, todas las capas
    public float rayDistance = 1.5f;
    public bool ObstacleAhead { get; private set; }

    void FixedUpdate()
    {
        //TODO: necesito una manera de crear un raycast mas gordo para que vea mejor
        Vector3 origin = transform.position + Vector3.up * rayOriginHeight;

        Vector3 direction = transform.position;

        ObstacleAhead = Physics.Raycast(
        origin,
        direction,
        out lastRayHit,
        rayDistance,
        rayLayers,
        QueryTriggerInteraction.Ignore
    );

        if (ObstacleAhead && lastRayHit.collider != null)
        {
            Debug.Log($"Raycast tocó: {lastRayHit.collider.gameObject.name}", lastRayHit.collider.gameObject);
        }

        Debug.DrawRay(origin, direction * rayDistance, Color.red);
    }
}
