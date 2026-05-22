using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ShopItem
{
    public string itemName;
    public string description;
    public int cost;
    public ItemEffect effect;
    public float effectValue;
    public enum ItemEffect { HealPlayer, SpeedBoost, JumpBoost, AmmoRefill }
}

public class ShopSystem : MonoBehaviour
{
    public List<ShopItem> items = new List<ShopItem>();
    public GameObject shopUI;
    private bool playerInRange, isOpen;
    private PlayerMovement pm;
    private HealthSystem ph;

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) { pm = player.GetComponent<PlayerMovement>(); ph = player.GetComponent<HealthSystem>(); }
        if (shopUI) shopUI.SetActive(false);
    }

    void Update() { /* Interact is handled by Player Input via OnInteract */ }

    public void ToggleShop()
    {
        isOpen = !isOpen;
        shopUI.SetActive(isOpen);
        Time.timeScale = isOpen ? 0f : 1f;
    }

    public void PurchaseItem(int index)
    {
        if (index < 0 || index >= items.Count) return;
        ShopItem item = items[index];
        if (!GameManager.Instance.SpendCoins(item.cost)) { Debug.Log("Not enough coins!"); return; }

        switch (item.effect)
        {
            case ShopItem.ItemEffect.HealPlayer: ph?.Heal(item.effectValue); break;
            case ShopItem.ItemEffect.SpeedBoost: if (pm) pm.moveSpeed += item.effectValue; break;
            case ShopItem.ItemEffect.JumpBoost: if (pm) pm.jumpForce += item.effectValue; break;
        }
    }

    void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) playerInRange = true; }
    void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) { playerInRange = false; if (isOpen) ToggleShop(); } }

    public void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (ctx.performed && playerInRange) ToggleShop();
    }
}