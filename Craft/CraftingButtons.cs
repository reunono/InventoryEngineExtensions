using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.UI;

namespace Craft
{
    public class CraftingButtons : MonoBehaviour
    {
        [Tooltip("The inventory where the ingredients for crafting are found and where the resulting item will be added")]
        [SerializeField]
        private Inventory CraftingInventory;
        [SerializeField]
        private Craft Craft;
        private GameObject _craftingButton;

        private void Awake()
        {
            _craftingButton = transform.GetChild(0).gameObject;
        }

        private void Start()
        {
            foreach (var recipe in Craft.Recipes)
            {
                var craftingButton = Instantiate(_craftingButton, transform);
                craftingButton.transform.GetChild(0).GetComponent<Text>().text = recipe.Name;
                craftingButton.transform.GetChild(1).GetComponent<Text>().text = recipe.Item.ShortDescription;
                craftingButton.transform.GetChild(2).GetComponent<Image>().sprite = recipe.Item.Icon;
                craftingButton.transform.GetChild(3).GetComponent<Text>().text = recipe.IngredientsText;
                craftingButton.GetComponent<Button>().onClick.AddListener(()=>CraftingInventory.Craft(recipe));
            }
            Destroy(_craftingButton);
        }
    }
}
