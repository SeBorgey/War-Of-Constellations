using UnityEngine;
using TMPro;

namespace Gameplay.Map.Visualization
{
    /// <summary>
    /// Visual representation of a Star.
    /// This component is added to the same GameObject as Star.
    /// Handles sprite rendering, color updates, and animations.
    /// </summary>
    [RequireComponent(typeof(Star))]
    public class StarView : MonoBehaviour
    {
        private Star _star;
        private MapVisualizationSettings _settings;
        private SpriteRenderer _spriteRenderer;
        private TextMeshPro _label;
        private CircleCollider2D _collider;

        private StarState _lastState;
        private float _lastBlueProgress;
        private float _lastRedProgress;
        private float _baseScale;
        private int _lastHP = -1; // -1 чтобы гарантировать обновление при первом вызове

        // Cached default sprite (shared across all instances)
        private static Sprite _defaultCircleSprite;

        public Star Star => _star;

        /// <summary>
        /// Initialize the StarView with settings.
        /// Star reference is obtained from the same GameObject.
        /// </summary>
        public void Initialize(MapVisualizationSettings settings)
        {
            _star = GetComponent<Star>();
            _settings = settings;

            if (_star == null)
            {
                Debug.LogError("[StarView] No Star component found on GameObject!");
                return;
            }

            SetupSprite();
            SetupLabel();
            SetupCollider();

            Refresh();
        }

        private void SetupSprite()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (_settings != null && _settings.starSprite != null)
            {
                _spriteRenderer.sprite = _settings.starSprite;
            }
            else if (_spriteRenderer.sprite == null)
            {
                // Create a default circle sprite if none assigned
                _spriteRenderer.sprite = GetOrCreateDefaultCircleSprite();
            }

            // Set sorting order so stars render above edges
            _spriteRenderer.sortingOrder = 10;
        }

        private static Sprite GetOrCreateDefaultCircleSprite()
        {
            if (_defaultCircleSprite != null) return _defaultCircleSprite;

            // Create a simple white circle texture
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        // Soft edge
                        float alpha = Mathf.Clamp01((radius - dist) / 2f);
                        pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            _defaultCircleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _defaultCircleSprite;
        }

        private void SetupLabel()
        {
            // Check if label already exists
            Transform existingLabel = transform.Find("Label");
            if (existingLabel != null)
            {
                _label = existingLabel.GetComponent<TextMeshPro>();
                if (_label != null)
                {
                    UpdateLabel();
                    return;
                }
            }

            // Create label child object
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(transform);
            labelObj.transform.localPosition = new Vector3(0, -0.8f, 0);

            _label = labelObj.AddComponent<TextMeshPro>();
            _label.fontSize = 3;
            _label.alignment = TextAlignmentOptions.Center;
            _label.color = Color.black;
            _label.sortingOrder = 11;
            UpdateLabel();
        }

        private void SetupCollider()
        {
            _collider = GetComponent<CircleCollider2D>();
            if (_collider == null)
            {
                _collider = gameObject.AddComponent<CircleCollider2D>();
                _collider.radius = 0.5f;
            }
        }

        private void Update()
        {
            if (_star == null || _settings == null) return;

            // Check for state changes and update visuals
            float blueProgress = GetBlueProgress();
            float redProgress = GetRedProgress();
            StarState currentState = _star.State;
            int currentHP = _star.HP;

            bool needsColorUpdate = currentState != _lastState ||
                               !Mathf.Approximately(blueProgress, _lastBlueProgress) ||
                               !Mathf.Approximately(redProgress, _lastRedProgress);

            if (needsColorUpdate)
            {
                UpdateColor();
                _lastState = currentState;
                _lastBlueProgress = blueProgress;
                _lastRedProgress = redProgress;
            }

            // Update label when HP changes
            if (currentHP != _lastHP)
            {
                UpdateLabel();
                _lastHP = currentHP;
            }

            // Apply pulse animation
            UpdatePulseAnimation();
        }

        /// <summary>
        /// Force refresh all visual elements.
        /// </summary>
        public void Refresh()
        {
            if (_star == null || _settings == null) return;

            _baseScale = _settings.GetStarScale(_star.Size);
            UpdateColor();
            UpdateScale();
            UpdateLabel();

            // Сбрасываем кэшированные значения чтобы Update мог их обновить
            _lastHP = -1;
            _lastState = StarState.White;
            _lastBlueProgress = -1;
            _lastRedProgress = -1;
        }

        private void UpdateColor()
        {
            if (_star == null || _settings == null || _spriteRenderer == null) return;

            float blueProgress = GetBlueProgress();
            float redProgress = GetRedProgress();

            Color color = _settings.GetStarColor(_star.State, blueProgress, redProgress);
            _spriteRenderer.color = color;
        }

        private void UpdateScale()
        {
            if (_star == null || _settings == null) return;

            _baseScale = _settings.GetStarScale(_star.Size);
            transform.localScale = Vector3.one * _baseScale;
        }

        private void UpdatePulseAnimation()
        {
            if (_star == null || _settings == null) return;

            float captureProgress = Mathf.Max(GetBlueProgress(), GetRedProgress());

            // Only pulse when capture is in progress (not complete)
            if (_star.State == StarState.White && captureProgress > 0)
            {
                float pulse = 1f + Mathf.Sin(Time.time * _settings.pulseSpeed) * _settings.pulseAmplitude * captureProgress;
                transform.localScale = Vector3.one * _baseScale * pulse;
            }
            else
            {
                transform.localScale = Vector3.one * _baseScale;
            }
        }

        private void UpdateLabel()
        {
            if (_label != null && _star != null)
            {
                // Show HP info
                _label.text = $"{_star.HP}";
            }
        }

        private float GetBlueProgress()
        {
            if (_star == null || _star.HP <= 0) return 0f;
            return (float)_star.BlueDamage / _star.HP;
        }

        private float GetRedProgress()
        {
            if (_star == null || _star.HP <= 0) return 0f;
            return (float)_star.RedDamage / _star.HP;
        }

        private void OnDestroy()
        {
            _star = null;
            _settings = null;
        }
    }
}
