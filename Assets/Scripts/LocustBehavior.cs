using System.Collections;
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

    public float moveDistance = 3.0f; // Max distance from start position
    public float totalDuration = 1.0f; // Total time for the entire cycle (out and back)
    public KeyCode triggerKey = KeyCode.Space; // The button to press

    private Vector3 startPosition;
    private bool isAnimating = false;

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
                    startPosition = NorthRoot.transform.position;
                    StartCoroutine(this.Attacking(NorthRoot));
                }
                if (distLeft < distUp && distLeft < distRight && distLeft < distDown)
                {
                    this.WestRoot.SetActive(true);
                    startPosition = WestRoot.transform.position;
                    //StartCoroutine(this.Attacking(WestRoot));
                }
                if (distRight < distLeft && distRight < distUp && distRight < distDown)
                {
                    this.EastRoot.SetActive(true);
                    startPosition = EastRoot.transform.position;
                    //StartCoroutine(this.Attacking(EastRoot));
                }
                if (distDown < distLeft && distDown < distRight && distDown < distUp)
                {
                    this.SouthRoot.SetActive(true);
                    startPosition = SouthRoot.transform.position;
                    //StartCoroutine(this.Attacking(SouthRoot));
                }

            }
            nextDamageTime = Time.time + damageCooldown;
        }
    }
    public IEnumerator Attacking(GameObject direction)
    {
        isAnimating = true;

        // Calculate the end position relative to the object's local forward direction
        Vector3 endPosition = startPosition + direction.transform.forward * moveDistance;

        float timeElapsed = 0f;

        // --- Phase 1: Move Outward ---
        while (timeElapsed < totalDuration / 2f)
        {
            // Lerp smoothly from start to end position
            direction.transform.localPosition = Vector3.Lerp(startPosition, endPosition, timeElapsed / (totalDuration / 2f));
            timeElapsed += Time.deltaTime;
            yield return null; // Wait until the next frame
        }
        direction.transform.localPosition = endPosition; // Ensure it hits the exact end point

        timeElapsed = 0f;

        // --- Phase 2: Move Back to Start ---
        while (timeElapsed < totalDuration / 2f)
        {
            // Lerp smoothly from end position back to start
            direction.transform.localPosition = Vector3.Lerp(endPosition, startPosition, timeElapsed / (totalDuration / 2f));
            timeElapsed += Time.deltaTime;
            yield return null; // Wait until the next frame
        }
        direction.transform.localPosition = startPosition; // Ensure it returns home exactly

        isAnimating = false; // Animation finished, can trigger again
        direction.SetActive(false);
    }

}
