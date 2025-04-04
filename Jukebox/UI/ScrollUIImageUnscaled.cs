using UnityEngine;
using UnityEngine.UI;

namespace Jukebox.UI
{
    public class ScrollUIImageUnscaled : MonoBehaviour
    {
        private RawImage img;
        public float xSpeed;
        public float ySpeed;

        private void Start() => img = GetComponent<RawImage>();

        private void Update()
        {
            var vector2 = img.uvRect.position + new Vector2(xSpeed, ySpeed) * Time.unscaledDeltaTime;
            while (vector2.x > 1.0)
                --vector2.x;
            while (vector2.x < -1.0)
                ++vector2.x;
            while (vector2.y > 1.0)
                --vector2.y;
            while (vector2.y < -1.0)
                ++vector2.y;
            img.uvRect = new Rect(img.uvRect.position + new Vector2(xSpeed, ySpeed) * Time.unscaledDeltaTime, img.uvRect.size);
        }
    }
}