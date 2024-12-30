using InputSystem;
using Photon.Pun;
using UnityEngine;


namespace Player
{ 
    
public class PlayerMovement : MonoBehaviourPun
{
    [Header("Movement Settings")]
    [SerializeField] private float acceleration = 3f;          // Ускорение (ед/сек^2)
    [SerializeField] private float maxAccelerationTime = 3.5f; // За сколько секунд достигаем max скорости вперёд
    [SerializeField] private float backwardSpeed = 2f;         // Максимальная скорость назад
    [SerializeField] private Vector3[] movementDirections = {
        Vector3.forward,   // Вперёд
        Vector3.right,     // Вправо
        Vector3.back,      // Назад
        Vector3.left       // Влево
    };

    private Rigidbody _rb;
    private int _currentDirectionIndex = 0; // Текущий индекс направления вектора
    private float _currentSpeed = 0f; // Текущая скорость движения
    private float MaxForwardSpeed => acceleration * maxAccelerationTime;

    private bool _isMovingUp;
    private bool _isMovingDown;
    private InputHandler _inputHandler;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _inputHandler = gameObject.AddComponent<InputHandler>();
    }
    
    private void OnEnable()
    {
        _inputHandler.OnRight += HandleRight;
        _inputHandler.OnLeft += HandleLeft;

        _inputHandler.OnUp += HandleUp;
        _inputHandler.OnDown += HandleDown;
        _inputHandler.OnVerticalReleased += HandleVerticalReleased;
    }

    private void OnDisable()
    {
        _inputHandler.OnRight -= HandleRight;
        _inputHandler.OnLeft -= HandleLeft;

        _inputHandler.OnUp -= HandleUp;
        _inputHandler.OnDown -= HandleDown;
        _inputHandler.OnVerticalReleased -= HandleVerticalReleased;
    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            return; // Управляем только своим объектом
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        // ======= 1) Обработка скорости =======
        if (_isMovingUp)
        {
            // Ускоряемся вперёд
            _currentSpeed = Mathf.Min(_currentSpeed + acceleration * Time.fixedDeltaTime, MaxForwardSpeed);
        }
        else if (_isMovingDown)
        {
            if (_currentSpeed > 0f)
            {
                // Замедляемся до полной остановки
                _currentSpeed = Mathf.Max(_currentSpeed - acceleration * Time.fixedDeltaTime, 0f);
            }
            else
            {
                // Ускоряемся назад
                _currentSpeed = Mathf.Max(_currentSpeed - acceleration * Time.fixedDeltaTime, -backwardSpeed);
            }
        }
        else
        {
            // Постепенно замедляемся
            if (_currentSpeed > 0f)
            {
                _currentSpeed = Mathf.Max(_currentSpeed - acceleration * 0.5f * Time.fixedDeltaTime, 0f);
            }
            else
            {
                _currentSpeed = Mathf.Min(_currentSpeed + acceleration * 0.5f * Time.fixedDeltaTime, 0f);
            }
        }
        
        Vector3 currentDirection = movementDirections[_currentDirectionIndex];
        Vector3 velocity = currentDirection * _currentSpeed;
        _rb.velocity = new Vector3(velocity.x, _rb.velocity.y, velocity.z);
    }
    
    private void RotateDirection(bool clockwise)
    {
        _currentSpeed = 0f;
        if (clockwise)
        {
            _currentDirectionIndex = (_currentDirectionIndex + 1) % movementDirections.Length;
        }
        else
        {
            _currentDirectionIndex = (_currentDirectionIndex - 1 + movementDirections.Length) % movementDirections.Length;
        }

        Debug.Log($"Current Direction: {movementDirections[_currentDirectionIndex]}");
    }
    
    public void HandleUp()
    {
        _isMovingUp = true;
        _isMovingDown = false;
    }

    public void HandleDown()
    {
        _isMovingDown = true;
        _isMovingUp = false;
    }

    public void HandleVerticalReleased()
    {
        _isMovingUp = false;
        _isMovingDown = false;
    }

    public void HandleRight()
    {
        RotateDirection(clockwise: true);
    }

    public void HandleLeft()
    {
        RotateDirection(clockwise: false);
    }
    
    [PunRPC]
    public void RespawnAtPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"Player respawned at: {newPosition}");
    }
}

}
