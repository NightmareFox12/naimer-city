using Unity.Android.Gradle.Manifest;
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
    moveAction?.Enable();
    sprintAction?.Enable();
  }

  void Start()
  {
    controller = GetComponent<CharacterController>();

    moveAction = InputSystem.actions.FindAction("Move");
    sprintAction = InputSystem.actions.FindAction("Sprint");
  }

  // Gravedad interna
  private const float gravity = -9.81f;
  private float verticalVelocity;

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

    // --- GRAVITY --- 
    if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -1f; // mantiene pegado al suelo
    else verticalVelocity += gravity * Time.deltaTime;

    move.y = verticalVelocity;

    // Aplicar movimiento
    controller.Move(currentSpeed * Time.deltaTime * move);
  }


  void OnDisable()
  {
    // Deshabilitar la acción para evitar memory leaks
    moveAction.Disable();
    sprintAction.Disable();
  }
}
