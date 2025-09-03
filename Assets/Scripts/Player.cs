using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
  [Header("Movement")]
  public float speed = 5f;

  //private vars
  private InputAction moveAction;
  private InputAction sprintAction;
  private CharacterController controller;

  void Awake()
  {
  }

  void Start()
  {
    controller = GetComponent<CharacterController>();

    moveAction = InputSystem.actions.FindAction("Move");
    sprintAction = InputSystem.actions.FindAction("Sprint");
  }

  void Update()
  {
    Vector2 moveValue = moveAction.ReadValue<Vector2>();

    // Vector en el plano XZ
    Vector3 move = new(moveValue.x, 0, moveValue.y);

    // Normalizar para evitar velocidad extra en diagonal
    if (move.magnitude > 1f)
      move.Normalize();

    // Convertir a dirección local del jugador
    move = transform.TransformDirection(move);

    // Calcular velocidad final
    float currentSpeed = speed * (sprintAction.IsPressed() ? 2 : 1);

    // Aplicar movimiento
    controller.Move(move * currentSpeed * Time.deltaTime);
  }


  void OnDisable()
  {
    // Deshabilitar la acción para evitar memory leaks
    moveAction.Disable();
    sprintAction.Disable();
  }
}
