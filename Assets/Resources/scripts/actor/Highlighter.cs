using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ActorSystem
{
    /// <summary>
    /// Class to highlight actors, based on Highlightable.
    /// </summary>
    public class Highlighter
    {
        private static Material highlightPrototype;
        public static Material GetHighlightPrototypeMat()
        {
            if (highlightPrototype != null)
                return highlightPrototype;

            highlightPrototype = Resources.Load<Material>("shared_materials/mat_highlight");
            return highlightPrototype;
        }

        private Dictionary<Renderer, Material[]> highlightMaterials;
        private Dictionary<Renderer, Material[]> initialMaterials;

        private GameObject parent;

        public Highlighter(GameObject parent)
        {
            this.parent = parent;
            highlightPrototype = Highlighter.GetHighlightPrototypeMat();
            Start();
        }
        
        void Start()
        {
            initialMaterials = new Dictionary<Renderer, Material[]>();
            highlightMaterials = new Dictionary<Renderer, Material[]>();
            Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                List<Material> mat = new List<Material>(rend.sharedMaterials);
                initialMaterials.Add(rend, mat.ToArray());
                // Material hmat = Material.Instantiate(highlightPrototype);
                // hmat.CopyPropertiesFromMaterial(mat);
                // copy over properties from material
                Material hmat = Material.Instantiate(highlightPrototype);
                if (mat[0].IsKeywordEnabled("_DETAIL_MULX2"))
                    hmat.EnableKeyword("_DETAIL_MASK");

                List<string> textures = new List<string>() { "_MainTex", "_Colormask", "_DetailMask" };
                List<string> colors = new List<string>() { "_PriColor", "_SecColor", "_TerColor", "_DetailColor" };

                foreach (string text in textures)
                {
                    if (mat[0].HasProperty(text))
                        hmat.SetTexture(text, mat[0].GetTexture(text));
                }
                foreach (string col in colors)
                {
                    if (mat[0].HasProperty(col))
                        hmat.SetColor(col, mat[0].GetColor(col));
                }
                List<Material> hmats = new List<Material>(mat);
                hmats.Add(hmat);
                highlightMaterials.Add(rend, hmats.ToArray());
            }
        }

        bool highlighted = false;
        public bool Highlighted
        {
            get { return highlighted; }
            set
            {
                highlighted = value;
                SetHighlighted(value);
            }
        }

        private void SetHighlighted(bool highlighted)
        {
            foreach (Renderer rend in initialMaterials.Keys)
            {
                if (highlighted)
                {
                    rend.materials = highlightMaterials[rend];
                }
                else
                {
                    rend.materials = initialMaterials[rend];
                }
            }
        }
    }

}
