using UnityEngine;
using InventorySystem;
namespace InventorySampleScene
{
    public class ItemScript : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        string itemName;
        private bool trigger;
        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("Collision");
            if (other.CompareTag("Player"))
            {
                if (!InventoryController.instance.InventoryFull("Hotbar", itemName))
                {
                    InventoryController.instance.AddItem("Hotbar", itemName);
                    if(AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayPickup();
                    }
                    Destroy(gameObject);

                }
                else
                {
                    Debug.Log("Inventory Cannot Fit Item");
                }

            }
        }
    }
}
