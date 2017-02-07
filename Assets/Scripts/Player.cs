using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    #region(Variables)
    // Public
    public int health = 100;
    public int maxFuel = 100;
    public int armor = 300;
    public float fuel = 50;
    public float jetPackLift = 30f;
    public float movementSpeed = 20f;
    public float jumpHeight = 150f;
    public float rayDistance = 0.1f;
    public LayerMask layerMask;
    public bool isJumping = false;

    // Private
    private Rigidbody2D rigid;
    private MeshRenderer renderer;
    private Camera cam;
    private bool canJump = true;
    private Vector2 hitNormal;
    #endregion

    #region(Unity Functions)
    // Use this for initialization
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        rigid = GetComponent<Rigidbody2D>();
        renderer = GetComponent<MeshRenderer>();
        HandleNetwork();
    }

    void HandleNetwork()
    {
        if (!isLocalPlayer)
        {
            cam.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            Jump();
            Movement();
            JetPack();
        }
    }

    void FixedUpdate()
    {
        //Perform raycast under player to ground
        Bounds bounds = renderer.bounds;
        Vector3 size = bounds.size;
        Vector3 scale = transform.localScale;
        float height = (size.y * 0.5f) * scale.y;
        //Create a ray from the bottom of the player
        Vector3 origin = transform.position + Vector3.down * height;
        Vector3 direction = Vector3.down;
        Ray ray = new Ray(origin, direction);
        //Perform Raycast
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayDistance, ~layerMask.value);
        Debug.DrawLine(origin, origin + direction * rayDistance, Color.magenta);
        //Check if we hit something with the Ray
        if (hit.collider != null)
        {
            isJumping = false;
        }
    }
    #endregion

    #region(Movement Functions)
    void Jump()
    {
        // If we are not Jumping
        if (!isJumping)
        {
            // Obtain input for Jump
            float inputVertical = Input.GetAxis("Jump");
            if (inputVertical > 0 && canJump)
            {
                canJump = false;
                // Append movement with jump formula
                rigid.AddForce(Vector3.up * inputVertical
                    * jumpHeight, ForceMode2D.Impulse);
                // Set bool as true
                // isJumping = true;
            }

            //Rule to allow you to jump again
            if (inputVertical == 0)
            {
                canJump = true;
            }
        }
    }
    void Movement()
    {
        // Obtaining input for left and right
        float inputHorizontal = Input.GetAxis("Horizontal");

        rigid.velocity = new Vector2(0, rigid.velocity.y);
        // Appending movement formula
        if (inputHorizontal > 0 && hitNormal.x != -1) //Move Right
        {
            rigid.velocity = new Vector2(movementSpeed, rigid.velocity.y);
        }

        if (inputHorizontal < 0 && hitNormal.x != 1) //Move Left
        {
            rigid.velocity = new Vector2(-movementSpeed, rigid.velocity.y);
        }
    }

    void JetPack()
    {
        if (fuel < maxFuel)
        {
            fuel += 5 * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl) && fuel > 0)
        {
            rigid.AddForce(Vector3.up * jetPackLift *
                Time.deltaTime * 1.5f, ForceMode2D.Impulse);
            fuel -= 20 * Time.deltaTime;
        }
    }
    #endregion

    #region (Collision Functions)

    void OnCollisionStay2D(Collision2D col)
    {
        //Obtains the contact normal of the collision
        hitNormal = col.contacts[0].normal;
        Vector3 point = col.contacts[0].point;
        Debug.DrawLine(point, point + (Vector3)hitNormal * 5, Color.magenta);
    }

    void OnCollisionExit2D(Collision2D col)
    {
        hitNormal = Vector2.zero;
        isJumping = true;
    }

    #endregion
}


// ALT + SHIFT + (UP or DOWN) = Change All Lines