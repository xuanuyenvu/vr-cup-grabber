using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DissolveController : MonoBehaviour
{
    [Tooltip("How long it takes to fully dissolve / reappear.")]
    public float transitionDuration = 1f;

    [Tooltip("How long to wait after the dissolve is complete before reversing.")]
    public float holdTime = 2f;

    [Tooltip("Name of the shader property controlling the dissolve amount.")]
    public string dissolveProperty = "_DissolveAmount";

    private Material _material;

    private void Awake()
    {
        // grab a unique material instance so we don't affect other renderers
        _material = GetComponent<SpriteRenderer>().material;
    }

    private void OnEnable()
    {
        // ensure material starts dissolved so it fades in first
        _material.SetFloat(dissolveProperty, 1f);
        StartCoroutine(DissolveLoop());
    }

    private IEnumerator DissolveLoop()
    {
        while (true)
        {
            // fade out -> visible (1 -> 0) so it comes in
            yield return AnimateDissolve(1f, 0f, transitionDuration);

            // hold at fully visible
            yield return new WaitForSeconds(holdTime);

            // fade back out (0 -> 1)
            yield return AnimateDissolve(0f, 1f, transitionDuration);

            // hold at fully dissolved
            yield return new WaitForSeconds(holdTime);
        }
    }

    private IEnumerator AnimateDissolve(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _material.SetFloat(dissolveProperty, Mathf.Lerp(from, to, t));
            yield return null;
        }

        // ensure final value is set exactly
        _material.SetFloat(dissolveProperty, to);
    }
}
