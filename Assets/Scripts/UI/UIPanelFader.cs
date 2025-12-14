using System.Collections;
using UnityEngine;

[RequireComponent (typeof(CanvasGroup))]
public class UIPanelFader : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 0.75f;
    [SerializeField] private bool _enableOnFadeIn = false;
    private Coroutine _fadeCoroutine;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadePanel(bool isFadeIn)
    {
        if (!gameObject.activeSelf)
            if (_enableOnFadeIn) gameObject.SetActive(true);
            else return;

        if(_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = isFadeIn ? 0 : 1;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(Fading(isFadeIn));
    }

    private IEnumerator Fading(bool isFadeIn)
    {
        float progress = 0;

        _canvasGroup.interactable = false;
        
        while (progress < 1) 
        {
            progress += Time.unscaledDeltaTime / _fadeDuration;
            _canvasGroup.alpha = isFadeIn? progress : 1 - progress;
            yield return null;
        }
        _canvasGroup.alpha = isFadeIn ? 1 : 0;

        _canvasGroup.interactable = isFadeIn;
        gameObject.SetActive(isFadeIn);
        _fadeCoroutine = null;
    }
}
