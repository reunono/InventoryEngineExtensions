using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.UI;
public class PickupDisplayItem : MonoBehaviour 
{		
    [SerializeField] private Image Icon;
    [SerializeField] private Text Name;
    [SerializeField] private Text Quantity;
    public void Display(InventoryItem item, int quantity)
    {
        Icon.sprite = item.Icon;
        Name.text = item.ItemName;
        Quantity.text = quantity.ToString();
    }
    public void AddQuantity(int quantity) => Quantity.text = (int.Parse(Quantity.text) + quantity).ToString();
}
