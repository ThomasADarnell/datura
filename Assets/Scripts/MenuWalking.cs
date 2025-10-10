using UnityEngine;

public class MenuWalking : MonoBehaviour
{
    public float speed = 1f;
    public Animator animator;
    private RectTransform rectTransform;
    private Rect canvasRect;
    private int direction = 0; // 0 = up, 1 = right, 2 = down, 3 = left

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        animator = GetComponent<Animator>();
        
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            canvasRect = new Rect(0, 0, Screen.width, Screen.height);
        }
    }
    void Update()
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        bool isOffScreen = true;
        for (int i = 0; i < 4; i++)
        {
            if (canvasRect.Contains(corners[i]))
            {
                isOffScreen = false;
                break;
            }
        }
        if (isOffScreen)
        {
            switch (direction)
            {
                case 0:
                    animator.SetFloat("X", 0);
                    animator.SetFloat("Y", 1);
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -canvasRect.height / 2);
                    break;
                case 1:
                    animator.SetFloat("X", 1);
                    animator.SetFloat("Y", 0);
                    rectTransform.anchoredPosition = new Vector2(-canvasRect.width / 2, rectTransform.anchoredPosition.y);
                    break;
                case 2:
                    animator.SetFloat("X", 0);
                    animator.SetFloat("Y", -1);
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, canvasRect.height / 2);
                    break;
                case 3:
                    animator.SetFloat("X", -1);
                    animator.SetFloat("Y", 0);
                    rectTransform.anchoredPosition = new Vector2(canvasRect.width / 2, rectTransform.anchoredPosition.y);
                    break;
            } 
        } else
        {
            switch(direction)
            {
                case 0:
                    rectTransform.anchoredPosition += new Vector2(0, speed * Time.deltaTime);
                    break;
                case 1:
                    rectTransform.anchoredPosition += new Vector2(speed * Time.deltaTime, 0);
                    break;
                case 2:
                    rectTransform.anchoredPosition += new Vector2(0, -speed * Time.deltaTime);
                    break;
                case 3:
                    rectTransform.anchoredPosition += new Vector2(-speed * Time.deltaTime, 0);
                    break;
            }
        }
    }
}
