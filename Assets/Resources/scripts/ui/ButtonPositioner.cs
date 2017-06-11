using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UserInterface
{
    // nicely layout buttons on a canvas
    public static class ButtonPositioner
    {
        public static void layout(List<DynamicButton> items)
        {
            List<RectTransform> rectitems = new List<RectTransform>();
            foreach(DynamicButton btn in items)
            {
                rectitems.Add(btn.RectTransform);
            }
            layout(rectitems);
        }

        public static void layout(List<RectTransform> items)
        {
            
            float maxWidth = -1f;
            float maxHeight = -1f;

            foreach(RectTransform item in items)
            {
                if(item.rect.width > maxWidth)
                    maxWidth = item.rect.width/2f;
                if (item.rect.width > maxHeight)
                    maxHeight = item.rect.height/2f;
            }
            
            // had to look up the radius of a regular polygon...
            float radius = maxWidth / (2*Mathf.Sin(Mathf.PI / items.Count));
            float ratio = maxHeight / maxWidth;
            
            List<Vector2> arcPos = arcPositions(items.Count);
            // scale by the side length, and then offset each button by their center
            arcPos = transform(arcPos, new Vector4(2f*radius, 0f, 0f, 2f*radius), new Vector2(-1*maxWidth, -1*maxHeight));
            

            for(int i = 0; i < items.Count; i++)
            {
                RectTransform item = items[i];
                item.anchoredPosition = arcPos[i];
            }
        }

        /// <summary>
        /// Create a list of positions in a circle.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static List<Vector2> arcPositions(int n)
        {
            if (n <= 0)
            {
                throw new System.Exception("Invalid integer.");
            }
            if(n == 1)
            {
                List<Vector2> pos = new List<Vector2>(n);
                pos.Add(new Vector2(0f, 0f));
                return pos;
            }
            float baseangle = 2 * Mathf.PI / n;

            List<Vector2> positions = new List<Vector2>(new Vector2[n]);
            for(int i = 0; i < n; i++)
            {
                float angle = i * baseangle;
                float sine = Mathf.Sin(angle);
                float cosine = Mathf.Cos(angle);
                positions[i] = new Vector2(sine, cosine);
            }

            return positions;
        }

        /// <summary>
        /// Apply a linear transformation to every item in the list.
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="matrix">A 2x2 matrix of transformations. Format is (x, y, z, w)</param>
        /// <returns></returns>
        private static List<Vector2> transform(List<Vector2> positions,  Vector4 matrix, Vector2 offset)
        {
            List<Vector2> newPositions = new List<Vector2>(new Vector2[positions.Count]);
            for (int i = 0; i < positions.Count; i++)
            {
                Vector2 v = positions[i];
                float newx = v.x * matrix.x + v.y * matrix.y + offset.x;
                float newy = v.x * matrix.z + v.y * matrix.w + offset.y;
                newPositions[i] = new Vector2(newx, newy);
            }
            return newPositions;
        }

        private static List<Vector2> transform(List<Vector2> positions, Vector4 matrix)
        {
            return transform(positions, matrix, new Vector2(0f, 0f));
        }

        private static List<Vector2> transform(List<Vector2> positions, Vector2 offset)
        {
            return transform(positions, new Vector4(1f, 0f, 1f, 0f), offset);
        }
    }

}
