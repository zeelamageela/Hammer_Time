
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class InvertMask : Image
{
    private Material m_CachedMaterial;
    
    public override Material materialForRendering 
    {
        get 
        {
            Material baseMat = base.materialForRendering;
            
            // Create a copy of the material if we haven't already
            if (m_CachedMaterial == null)
            {
                m_CachedMaterial = new Material(baseMat);
                
                // Set stencil properties if the shader supports them
                if (m_CachedMaterial.HasProperty("_StencilComp"))
                {
                    // Show where stencil value is NOT equal to 1
                    m_CachedMaterial.SetInt("_Stencil", 1);  // Reference value (what the Mask writes)
                    m_CachedMaterial.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
                    m_CachedMaterial.SetInt("_StencilOp", (int)StencilOp.Keep);
                    m_CachedMaterial.SetInt("_StencilReadMask", 255);
                    m_CachedMaterial.SetInt("_StencilWriteMask", 255);
                }
            }
            
            return m_CachedMaterial;
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // Clean up the cached material
        if (m_CachedMaterial != null)
        {
            DestroyImmediate(m_CachedMaterial);
            m_CachedMaterial = null;
        }
    }
}
