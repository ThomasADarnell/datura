using UnityEngine;

public class LocustBehavior : MonoBehaviour
{
    public GameObject NorthRoot;
    public GameObject SouthRoot;
    public GameObject WestRoot;
    public GameObject EastRoot;

    public float maxSpeed = 2.5f;
    public float accelerationRate = 2f; // How fast the enemy speeds up
    public float decelerationRate = 5f; // How fast the enemy slows down
    public float stoppingDistance = 1.5f; // Distance from target where slow-down begins
    public float damageCooldown = 2f;


    protected Vector3 target;
    protected GameObject player;
    protected float currentSpeed = 0f; // New: Tracks the speed in the current frame
    protected Vector3 moveDirection; // Tracks the desired movement direction
    protected float nextDamageTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void OnTriggerEnter2D(Collider2D player)
    {
        if (player.CompareTag("Player") && Time.time >= nextDamageTime)
        { // When player and enemy collide
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();  // get player health
            float distUp = Vector2.Distance(NorthRoot.transform.position, player.transform.position);
            float distLeft = Vector2.Distance(WestRoot.transform.position, player.transform.position);
            float distRight = Vector2.Distance(EastRoot.transform.position, player.transform.position);
            float distDown = Vector2.Distance(SouthRoot.transform.position, player.transform.position);
            if (playerHealth != null)
            {
                if (distUp < distLeft && distUp < distRight && distUp < distDown)
                {
                    this.NorthRoot.SetActive(true);
                }
                if (distLeft < distUp && distLeft < distRight && distLeft < distDown)
                {
                    this.WestRoot.SetActive(true);
                }
                if (distRight < distLeft && distRight < distUp && distRight < distDown)
                {
                    this.EastRoot.SetActive(true);
                }
                if (distDown < distLeft && distDown < distRight && distDown < distUp)
                {
                    this.SouthRoot.SetActive(true);
                }

            }
            nextDamageTime = Time.time + damageCooldown;
        }
    }

}
