using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Vector2 _moveDir;
    private Rigidbody2D _rb;
    [SerializeField] private float speed;

    private void Awake()
    {
        _rb =  GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _moveDir * speed;
    }

    private void OnMove(InputValue value)
    {
        _moveDir = value.Get<Vector2>();
    }
}
