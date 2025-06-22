using UnityEngine;
using System.Collections.Generic;

public class PlungerArrowEffect : MonoBehaviour
{
    public GameObject arrowPrefab; // assign arrow prefab in inspector
    public Transform playerTransform; // reference set by player or manager
    public int arrowCount = 5;
    public float spacing = 0.5f;
    public float arrowSpeed = 1f;

    private List<GameObject> arrows = new List<GameObject>();
    private bool isActive = false;

    void Update()
    {
        if (!isActive || playerTransform == null)
        {
            HideArrows();
            return;
        }

        Vector3 start = transform.position;
        Vector3 end = playerTransform.position;
        Vector3 direction = (end - start).normalized;
        float totalDistance = Vector3.Distance(start, end);

        // Position arrows evenly spaced along line plunger->player
        for (int i = 0; i < arrows.Count; i++)
        {
            float progress = ((Time.time * arrowSpeed) + i * (1f / arrowCount)) % 1f;
            float distanceAlong = progress * totalDistance;

            Vector3 pos = start + direction * distanceAlong;

            if (i < arrows.Count)
            {
                arrows[i].transform.position = pos;
                arrows[i].transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            }
        }
    }

    // Call this to activate arrows
    public void ShowArrows(Transform player)
    {
        if (isActive && playerTransform == player) return; // already active

        playerTransform = player;

        if (arrows.Count == 0)
        {
            for (int i = 0; i < arrowCount; i++)
            {
                GameObject arrow = Instantiate(arrowPrefab, transform);
                arrows.Add(arrow);
            }
        }

        foreach (var arrow in arrows)
        {
            arrow.SetActive(true);
        }

        isActive = true;
    }

    // Call this to hide arrows
    public void HideArrows()
    {
        if (!isActive) return;

        foreach (var arrow in arrows)
        {
            if (arrow != null)
                arrow.SetActive(false);
        }

        isActive = false;
        playerTransform = null;
    }
}
