using UnityEngine;

namespace AstroIdle.Gameplay.Arena
{
    /// <summary>Procedurally generated static star background sized to the play area.</summary>
    public class Starfield : MonoBehaviour
    {
        private const int TextureWidth = 256;
        private const int PixelsPerStar = 180;

        private SpriteRenderer spriteRenderer;
        private Texture2D texture;
        private Sprite sprite;

        public void Build(Rect bounds, int seed = 1337)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingOrder = -100;
            }

            ReleaseGenerated();

            int height = Mathf.Clamp(Mathf.RoundToInt(TextureWidth * bounds.height / Mathf.Max(0.01f, bounds.width)), 64, 1024);
            texture = new Texture2D(TextureWidth, height, TextureFormat.RGBA32, false)
            {
                name = "Starfield",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };

            var pixels = new Color32[TextureWidth * height];
            var random = new System.Random(seed);
            int starCount = TextureWidth * height / PixelsPerStar;
            for (int i = 0; i < starCount; i++)
            {
                int index = random.Next(0, pixels.Length);
                byte brightness = (byte)random.Next(60, 256);
                pixels[index] = new Color32(brightness, brightness, (byte)Mathf.Min(255, brightness + 25), 255);
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            float pixelsPerUnit = TextureWidth / bounds.width;
            sprite = Sprite.Create(texture, new Rect(0, 0, TextureWidth, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            spriteRenderer.sprite = sprite;

            transform.position = new Vector3(bounds.center.x, bounds.center.y, 1f);
            float spriteHeight = height / pixelsPerUnit;
            transform.localScale = new Vector3(1f, bounds.height / spriteHeight, 1f);
        }

        private void OnDestroy()
        {
            ReleaseGenerated();
        }

        private void ReleaseGenerated()
        {
            if (sprite != null) Destroy(sprite);
            if (texture != null) Destroy(texture);
            sprite = null;
            texture = null;
        }
    }
}
