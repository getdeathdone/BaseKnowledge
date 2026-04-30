using UnityEngine;

namespace CoopPlatformer.Gameplay.Space
{
    public class ShipColorizer : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Renderer _colorTarget;
        [SerializeField] private Renderer _colorTarget1;
        [SerializeField] private Color[] _playerColors =
        {
            new Color(0.3f, 0.8f, 1f, 1f),
            new Color(1f, 0.45f, 0.3f, 1f),
            new Color(0.4f, 1f, 0.55f, 1f),
            new Color(1f, 0.9f, 0.25f, 1f)
        };

        private MaterialPropertyBlock _propertyBlock;

        public int ColorCount => _playerColors.Length;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        public void ApplyColorIndex(int colorIndex)
        {
            int normalizedIndex = Mathf.Abs(colorIndex) % _playerColors.Length;
            Color color = _playerColors[normalizedIndex];
            _colorTarget.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_Color", color);
            _propertyBlock.SetColor("_BaseColor", color);
            _propertyBlock.SetColor("_MainColor", color);
            _colorTarget.SetPropertyBlock(_propertyBlock);
            _colorTarget1.SetPropertyBlock(_propertyBlock);
        }
    }
}
