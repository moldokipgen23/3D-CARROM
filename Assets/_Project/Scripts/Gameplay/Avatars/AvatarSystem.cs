using UnityEngine;
using UnityEngine.UI;

public class AvatarSystem : MonoBehaviour
{
    [Header("Avatar Settings")]
    public Sprite defaultAvatar;
    public Sprite[] premiumAvatars;
    public Sprite[] eventAvatars;
    
    [Header("UI References")]
    public Image avatarImage;
    
    [Header("Currency References")]
    public CurrencyService currencyService;
    
    private void Start()
    {
        InitializeReferences();
    }
    
    private void InitializeReferences()
    {
        if (currencyService == null)
        {
            currencyService = ServiceLocator.Get<CurrencyService>();
        }
    }
    
    public void LoadAvatar(string avatarId)
    {
        Sprite avatar = GetAvatarById(avatarId);
        
        if (avatarImage != null)
        {
            avatarImage.sprite = avatar;
        }
        
        Debug.Log($"Avatar loaded: {avatarId}");
    }
    
    public Sprite GetAvatarById(string avatarId)
    {
        // Try to parse avatar ID to determine type
        if (avatarId.StartsWith("default_"))
        {
            return defaultAvatar;
        }
        else if (avatarId.StartsWith("premium_"))
        {
            int index = int.Parse(avatarId.Split('_')[1]);
            if (index >= 0 && index < premiumAvatars.Length)
            {
                return premiumAvatars[index];
            }
        }
        else if (avatarId.StartsWith("event_"))
        {
            int index = int.Parse(avatarId.Split('_')[1]);
            if (index >= 0 && index < eventAvatars.Length)
            {
                return eventAvatars[index];
            }
        }
        
        return defaultAvatar;
    }
    
    public bool CanUnlockAvatar(string avatarId)
    {
        if (avatarId.StartsWith("premium_"))
        {
            int index = int.Parse(avatarId.Split('_')[1]);
            // Check if player has enough currency
            return currencyService.GetCurrency("tokens") >= 100 * (index + 1);
        }
        else if (avatarId.StartsWith("event_"))
        {
            // Event avatars might be unlocked through events
            return true; // Simplified for now
        }
        
        return true; // Default avatars are always available
    }
    
    public void UnlockAvatar(string avatarId)
    {
        Debug.Log($"Avatar unlocked: {avatarId}");
        
        if (avatarId.StartsWith("premium_"))
        {
            int index = int.Parse(avatarId.Split('_')[1]);
            // Deduct currency
            currencyService.SpendCurrency("tokens", 100 * (index + 1));
        }
    }
}