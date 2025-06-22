using UnityEngine;

public class DynamicShadowFollower : MonoBehaviour
{
    [Tooltip("The Transform of the original GameObject this shadow is following (and is now its parent).")]
    public Transform target; // This will now be the parent of this shadow GameObject

    [Tooltip("The offset applied to the shadow relative to its target (in the target's local space).")]
    public Vector2 offset;

    [Tooltip("Adjusts the sorting order of the shadow. Lower numbers render behind the original.")]
    public int sortingOrderOffset;

    // References to the SpriteRenderers, passed from DropShadowAdder
    [HideInInspector] public SpriteRenderer shadowSR;
    [HideInInspector] public SpriteRenderer targetSR;

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject); // Target was destroyed, destroy shadow
            return;
        }

        // --- Step 1: Update Shadow Sprite and Sorting Order ---
        if (targetSR != null && shadowSR != null)
        {
            shadowSR.sprite = targetSR.sprite;
            shadowSR.sortingLayerID = targetSR.sortingLayerID;
            shadowSR.sortingOrder = targetSR.sortingOrder + sortingOrderOffset;
        }
        else
        {
            // Fallback if references weren't set by DropShadowAdder
            shadowSR = GetComponent<SpriteRenderer>();
            targetSR = target.GetComponent<SpriteRenderer>();
            if (shadowSR == null || targetSR == null)
            {
                Debug.LogError("Shadow or Target GameObject missing SpriteRenderer!", this);
                Destroy(gameObject);
                return;
            }
        }

        // --- Step 2: Set the Shadow's Local Transform ---

        // Determine the effective horizontal flip of the *root* entity (e.g., Player or Enemy).
        // This is crucial because the main GameObject (e.g., 'Player') holds the overall flip (localScale.x).
        float effectiveRootFlipX = 1f;
        // Check the localScale.x of the target's root transform (the main entity doing the flipping)
        if (target.root.localScale.x < 0)
        {
            effectiveRootFlipX = -1f;
        }

        // Calculate the adjusted X offset.
        // If the root entity is flipped, we flip the X offset too, so it stays on the screen's "right".
        float adjustedOffsetX = offset.x * effectiveRootFlipX;
        float adjustedOffsetY = offset.y; // Y offset typically doesn't flip

        // Set the shadow's local position relative to its parent (the original sprite).
        // This ensures the shadow maintains its "bottom right" visual position on screen
        // regardless of the parent entity's facing direction.
        transform.localPosition = new Vector3(adjustedOffsetX, adjustedOffsetY, 0f);

        // Explicitly set local rotation to identity (no rotation on the shadow itself).
        // This prevents the shadow from inheriting rotations like the player's wobble.
        transform.localRotation = Quaternion.identity;

        // Explicitly set local scale to one. The shadow will inherit the parent's (sprite's)
        // scale, including the flip from the root entity, maintaining its aspect ratio.
        transform.localScale = Vector3.one;

        // Set the shadow's color (as per your original script)
        if (shadowSR != null)
        {
            DropShadowAdder parentAdder = GetComponentInParent<DropShadowAdder>();
            if (parentAdder != null)
            {
                shadowSR.color = parentAdder.shadowColor;
            }
            else
            {
                shadowSR.color = Color.black; // Default if no DropShadowAdder is found
            }
        }
    }
}