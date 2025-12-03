using System.Collections;
using UnityEditor;
using UnityEngine;

public class Sticky : MonoBehaviour
{
    public float stickTime = 3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("collision test");
            PlayerMovement moveScript = FindFirstObjectByType<PlayerMovement>();
            moveScript.canMove = false;
            moveScript.canDash = false;
            StartCoroutine(TimedStick(stickTime, moveScript));
        }
    }

    public IEnumerator TimedStick(float time, PlayerMovement moveScript)
    {
        yield return new WaitForSeconds(time);
        moveScript.canMove = true;
        moveScript.canDash = true;

    }
}
