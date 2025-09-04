using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform player;

    //private vars
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    //TODO: buscar la forma de ponerle el transform a la camara para ver hacia delante
    //TODO: luego mover con el mouse usando fixedUpdate para un movimiento smoot
    //TODO: luego darle el movimiento al script player para que el se mueva a donde ve tu click
    void Start()
    {

    }

    void Update()
    {

    }

    void FixedUpdate()
    {

    }
}
