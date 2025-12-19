using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class StyledButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.black;
        [SerializeField] private Color _hoverColor = new Color(0.2f, 0.78f, 0.55f, 1f); // Manjaro green
        [SerializeField] private Color _borderColor = Color.white;

        [Header("Transition")]
        [SerializeField] private float _transitionDuration = 0.2f;

        private Image _backgroundImage;
        private Outline _outline;
        private Color _targetColor;
        private Color _currentColor;
        private float _transitionProgress = 1f;

        private void Awake()
        {
            _backgroundImage = GetComponent<Image>();
            _outline = GetComponent<Outline>();

            if (_outline == null)
            {
                _outline = gameObject.AddComponent<Outline>();
            }

            _outline.effectColor = _borderColor;
            _outline.effectDistance = new Vector2(2f, 2f);

            _currentColor = _normalColor;
            _targetColor = _normalColor;
            _backgroundImage.color = _normalColor;
        }

        private void Update()
        {
            if (_transitionProgress < 1f)
            {
                _transitionProgress += Time.unscaledDeltaTime / _transitionDuration;
                _currentColor = Color.Lerp(_currentColor, _targetColor, _transitionProgress);
                _backgroundImage.color = _currentColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _targetColor = _hoverColor;
            _transitionProgress = 0f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _targetColor = _normalColor;
            _transitionProgress = 0f;
        }

        public void SetBorderWidth(float width)
        {
            if (_outline != null)
            {
                _outline.effectDistance = new Vector2(width, width);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Reset to Default Colors")]
        private void ResetToDefaults()
        {
            _normalColor = Color.black;
            _hoverColor = new Color(0.2f, 0.78f, 0.55f, 1f);
            _borderColor = Color.white;
            _transitionDuration = 0.2f;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
