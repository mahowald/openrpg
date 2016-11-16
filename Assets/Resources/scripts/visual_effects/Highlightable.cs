using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Highlightable : MonoBehaviour {

    public Material highlightPrototype;

    private Dictionary<Renderer, Material> highlightMaterials;
    private Dictionary<Renderer, Material> initialMaterials;

	// Use this for initialization
	void Start () {

        initialMaterials = new Dictionary<Renderer, Material>();
        highlightMaterials = new Dictionary<Renderer, Material>();
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach(Renderer rend in renderers)
        {
            Material mat = rend.sharedMaterial;
            initialMaterials.Add(rend, mat);
            // Material hmat = Material.Instantiate(highlightPrototype);
            // hmat.CopyPropertiesFromMaterial(mat);
            // copy over properties from material
            Material hmat = Material.Instantiate(highlightPrototype);
            if (mat.IsKeywordEnabled("_DETAIL_MULX2"))
                hmat.EnableKeyword("_DETAIL_MASK");

            List<string> textures = new List<string>() { "_MainTex", "_Colormask", "_DetailMask" };
            List<string> colors = new List<string>() { "_PriColor", "_SecColor", "_TerColor", "_DetailColor" };

            foreach(string text in textures)
            {
                if (mat.HasProperty(text))
                    hmat.SetTexture(text, mat.GetTexture(text));
            }
            foreach(string col in colors)
            {
                if (mat.HasProperty(col))
                    hmat.SetColor(col, mat.GetColor(col));
            }
            
            highlightMaterials.Add(rend, hmat);
        }
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetKeyUp("h"))
        {
            Highlighted = !Highlighted;
        }
	}

    bool highlighted = false;
    bool Highlighted
    {
        get { return highlighted; }
        set {
            highlighted = value;
            SetHighlighted(value);
        }
    }

    void SetHighlighted(bool highlighted)
    {
        foreach(Renderer rend in initialMaterials.Keys)
        {
            if(highlighted)
            {
                rend.material = highlightMaterials[rend];
            }
            else
            {
                rend.material = initialMaterials[rend];
            }
        }
    }
}
