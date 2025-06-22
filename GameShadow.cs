using UnityEngine;
using System.Collections.Generic;

public class DropShadowAdder : MonoBehaviour
{
    [Header("Shadow Settings")]
    [Tooltip("The offset of the shadow relative to the original sprite.")]
    public Vector2 shadowOffset = new Vector2(0.1f, -0.1f);

    [Tooltip("The color and transparency of the shadow.")]
    public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

    [Tooltip("Adjusts the sorting order of the shadow. Lower numbers render behind the original.")]
    public int shadowSortingOrderOffset = -1;

    [Header("Filtering (Optional)")]
    [Tooltip("If true, only add shadows to GameObjects on these specific layers.")]
    public bool useLayerFilter = false;
    [Tooltip("Only GameObjects on these layers will receive a shadow if 'Use Layer Filter' is true.")]
    public LayerMask layerFilter;

    [Header("Exclusion (Optional)")]
    [Tooltip("GameObjects with these tags will be excluded from shadow generation by this script.")]
    public List<string> excludeTags; // New: List of tags to exclude

    private List<GameObject> createdShadows = new List<GameObject>();

    void Start()
    {
        SpriteRenderer[] spriteRenderersInChildren = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer originalSpriteRenderer in spriteRenderersInChildren)
        {
            // Skip the SpriteRenderer on the parent object itself
            if (originalSpriteRenderer.gameObject == this.gameObject)
            {
                continue;
            }

            // Exclude based on tag
            bool excludedByTag = false;
            foreach (string tag in excludeTags)
            {
                if (originalSpriteRenderer.CompareTag(tag))
                {
                    excludedByTag = true;
                    break;
                }
            }
            if (excludedByTag)
            {
                Debug.Log($"Excluding {originalSpriteRenderer.name} from DropShadowAdder due to tag '{originalSpriteRenderer.tag}'.", originalSpriteRenderer.gameObject);
                continue;
            }

            // Apply layer filtering if enabled
            if (useLayerFilter && !(((1 << originalSpriteRenderer.gameObject.layer) & layerFilter) > 0))
            {
                continue;
            }

            AddShadow(originalSpriteRenderer.gameObject);
        }
    }

    void AddShadow(GameObject original)
    {
        SpriteRenderer originalSR = original.GetComponent<SpriteRenderer>();
        if (originalSR == null || originalSR.sprite == null)
        {
            Debug.LogWarning($"Skipping shadow for {original.name}: No SpriteRenderer or sprite found.", original);
            return;
        }

        GameObject shadowGO = new GameObject(original.name + "_Shadow");

        // KEY CHANGE: Make the shadow a child of the original sprite's GameObject
        shadowGO.transform.parent = original.transform;

        // REMOVED: Initial localScale and rotation are now managed by DynamicShadowFollower
        // shadowGO.transform.localScale = Vector3.one;
        // shadowGO.transform.rotation = Quaternion.identity;

        SpriteRenderer shadowSR = shadowGO.AddComponent<SpriteRenderer>();
        shadowSR.sprite = originalSR.sprite;
        shadowSR.sortingLayerID = originalSR.sortingLayerID;
        shadowSR.sortingOrder = originalSR.sortingOrder + shadowSortingOrderOffset;
        shadowSR.color = shadowColor;

        DynamicShadowFollower follower = shadowGO.AddComponent<DynamicShadowFollower>();
        follower.target = original.transform; // The target is still the original sprite's transform
        follower.offset = shadowOffset;
        follower.sortingOrderOffset = shadowSortingOrderOffset;
        follower.shadowSR = shadowSR;
        follower.targetSR = originalSR;

        createdShadows.Add(shadowGO);
    }

    public void DestroyAllShadows()
    {
        foreach (GameObject shadow in createdShadows)
        {
            if (shadow != null)
            {
                Destroy(shadow);
            }
        }
        createdShadows.Clear();
    }
}