using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
  [Header("Movement")]
  public float speed = 5f;

  //private vars
  private InputAction moveAction;
  private CharacterController controller;

  void Awake()
  {
  }

  void Start()
  {
    controller = GetComponent<CharacterController>();

    moveAction = InputSystem.actions.FindAction("Move");
  }

  void Update()
  {
    Vector2 moveValue = moveAction.ReadValue<Vector2>();

    // Convertimos el Vector2 en Vector3 (X y Z para moverse en el plano horizontal)
    Vector3 move = new(moveValue.x, 0, moveValue.y);

    // Aplicamos el movimiento con velocidad
    controller.Move(speed * Time.deltaTime * move);
  }
  void OnEnable()
  {
    moveAction?.Enable();
  }

  void OnDisable()
  {
    // Deshabilitar la acción para evitar memory leaks
    moveAction.Disable();
  }
}
