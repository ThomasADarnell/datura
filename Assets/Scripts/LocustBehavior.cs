using System.Collections;
using UnityEngine;

public class LocustBehavior : MonoBehaviour
{
    public GameObject NorthRoot;
    public GameObject SouthRoot;
    public GameObject WestRoot;
    public GameObject EastRoot;

    public float damageCooldown = 2f;

    protected float nextDamageTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float moveDistance = .5f; // Max distance from start position
    public float totalDuration = 1.0f; // Total time for the entire cycle (out and back)

    private Vector3 startPosition;
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
                    Vector3 end = new Vector3(0f, moveDistance, 0f);
                    StartCoroutine(this.Attacking(NorthRoot, end));
                }
                if (distLeft < distUp && distLeft < distRight && distLeft < distDown)
                {
                    this.WestRoot.SetActive(true);
                    Vector3 end = new Vector3(-moveDistance, 0f, 0f);
                    StartCoroutine(this.Attacking(WestRoot, end));
                }
                if (distRight < distLeft && distRight < distUp && distRight < distDown)
                {
                    this.EastRoot.SetActive(true);
                    Vector3 end = new Vector3(moveDistance, 0f, 0f);
                    StartCoroutine(this.Attacking(EastRoot, end));
                }
                if (distDown < distLeft && distDown < distRight && distDown < distUp)
                {
                    this.SouthRoot.SetActive(true);
                    Vector3 end = new Vector3(0f, -moveDistance, 0f);
                    StartCoroutine(this.Attacking(SouthRoot, end));
                }

            }
            nextDamageTime = Time.time + damageCooldown;
        }
    }
    public IEnumerator Attacking(GameObject direction, Vector3 endPosition)
    {
        startPosition = direction.transform.localPosition;
        endPosition += startPosition;

        // Calculate the end position relative to the object's local forward direction

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

        direction.SetActive(false);
    }

}
