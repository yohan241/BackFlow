using UnityEngine;

public class PlungerShadowController : MonoBehaviour
{
    [Header("Shadow Settings")]
    [Tooltip("The offset of the shadow relative to the plunger's local origin.")]
    public Vector2 shadowOffset = new Vector2(0.1f, -0.1f);

    [Tooltip("The color and transparency of the shadow.")]
    public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

    [Tooltip("Adjusts the sorting order of the shadow. Lower numbers render behind the original.")]
    public int shadowSortingOrderOffset = -1;

    private GameObject shadowGO;
    private SpriteRenderer shadowSR;
    private SpriteRenderer plungerSR;

    void Awake()
    {
        plungerSR = GetComponent<SpriteRenderer>();
        if (plungerSR == null)
        {
            Debug.LogError("PlungerShadowController: No SpriteRenderer found on this GameObject!", this);
            enabled = false;
            return;
        }
        if (GetComponent<PlungerShadowController>() == null)
        {
            gameObject.AddComponent<PlungerShadowController>();
        }


        CreatePlungerShadow();
    }

    void CreatePlungerShadow()
    {
        shadowGO = new GameObject(gameObject.name + "_Shadow");
        // Make the shadow a sibling of the plunger
        shadowGO.transform.parent = transform.parent;

        shadowSR = shadowGO.AddComponent<SpriteRenderer>();
        shadowSR.sprite = plungerSR.sprite;
        shadowSR.sortingLayerID = plungerSR.sortingLayerID;
        shadowSR.sortingOrder = plungerSR.sortingOrder + shadowSortingOrderOffset;
        shadowSR.color = shadowColor;

        // Initial setup for shadow to match plunger's orientation
        shadowGO.transform.localRotation = transform.localRotation;
        shadowGO.transform.localScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (plungerSR == null || shadowGO == null || shadowSR == null)
        {
            if (shadowGO != null) Destroy(shadowGO);
            enabled = false;
            return;
        }

        // --- Update Shadow Sprite and Sorting Order ---
        shadowSR.sprite = plungerSR.sprite;
        shadowSR.sortingLayerID = plungerSR.sortingLayerID;
        shadowSR.sortingOrder = plungerSR.sortingOrder + shadowSortingOrderOffset;

        // --- Calculate the Shadow's World Position ---
        // This calculates the shadow's world position by offsetting from the plunger's local space.
        // It automatically accounts for the plunger's rotation and scale.
        Vector3 localOffset = new Vector3(shadowOffset.x, shadowOffset.y, 0f);
        // If the plunger's SpriteRenderer is flipped, we reverse the offset for the shadow to appear correctly
        if (plungerSR.flipX) localOffset.x *= -1;
        if (plungerSR.flipY) localOffset.y *= -1;


        // Transform the local offset into world space using the plunger's current transform.
        // This will correctly account for the plunger's position, rotation, and scale.
        shadowGO.transform.position = transform.TransformPoint(localOffset);

        // --- Allow Shadow to Rotate and Flip with Plunger ---
        // Copy the plunger's local rotation and local scale directly.
        // This will make the shadow mimic the plunger's orientation changes.
        shadowGO.transform.localRotation = transform.localRotation;
        shadowGO.transform.localScale = transform.localScale;

        // Set the shadow's color
        shadowSR.color = shadowColor;
    }

    void OnDestroy()
    {
        if (shadowGO != null)
        {
            Destroy(shadowGO);
        }
    }
}