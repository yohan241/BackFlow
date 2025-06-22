using UnityEngine;

public class HighlightablePlunger : MonoBehaviour
{
    private GameObject outlineObject;
    private PlungerArrowEffect arrowEffect;

    void Awake()
    {
        // Find the outline child GameObject by name
        outlineObject = transform.Find("Outline")?.gameObject;

        if (outlineObject == null)
        {
            Debug.LogWarning("Outline child object not found! Please add an 'Outline' child with white sprite.");
        }
        else
        {
            outlineObject.SetActive(false); // start hidden
        }
        arrowEffect = GetComponent<PlungerArrowEffect>();
        if (arrowEffect == null)
        {
            Debug.LogWarning("PlungerArrowEffect component missing on plunger!");
        }
    }
    public void SetHighlight2(bool highlight, Transform player = null)
    {
        if (arrowEffect == null) return;
        if (highlight)
        {
            if (player != null)
                arrowEffect.ShowArrows(player);
        }
        else
        {
            arrowEffect.HideArrows();
        }
    }
    // Call to toggle outline highlight
    public void SetHighlight(bool highlight)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(highlight);
        }
    }
}
