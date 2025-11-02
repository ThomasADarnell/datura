using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeartDisplay : MonoBehaviour
{
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public GameObject heartPrefab;
    public int spacing = 5;

    private List<Image> hearts = new List<Image>();

    public void InitHearts(int maxHealth)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        hearts.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, transform);
            var rect = heart.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(i * (rect.sizeDelta.x + spacing), 0);
            hearts.Add(heart.GetComponent<Image>());
        }
    }

    public void UpdateHearts(int current, int max)
    {
        if (hearts.Count != max)
            InitHearts(max);

        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].sprite = (i < current) ? fullHeart : emptyHeart;
        }
    }
}
