using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0f;
    [SerializeField] LayerMask groundLayer;
    ShooterController shooterController;
    Vector3 velocity;
    CharacterController characterController;
    [SerializeField] GameObject cameraObject;
    bool isGrounded;

    public override void OnNetworkSpawn()
    {
        cameraObject.SetActive(false);
        if (!IsOwner) return;
        cameraObject.SetActive(true);
       
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        shooterController =GetComponent<ShooterController>();
       // animator = GetComponentInChildren<Animator>(); // Get the Animator component
    }

    void Update()
    {   
       if (!IsOwner) return;
       if(shooterController.IsDead.Value == true)return;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        characterController.Move(move * speed * Time.deltaTime);
        
        float speedB = z; 
        float speedA = x; 

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            
        }
        
        velocity.y += Physics.gravity.y * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
    }
}
