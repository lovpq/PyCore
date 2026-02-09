using UnityEngine;

namespace Core
{
    public class OutlineEffect : MonoBehaviour
    {
        [Header("Outline Settings")]
        [SerializeField] private Color outlineColor = Color.white;
        [SerializeField] private float outlineWidth = 0.03f;

        private Material[][] originalMaterials;
        private Material[][] outlineMaterials;
        private Renderer[] renderers;
        private bool isOutlineEnabled = false;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            SetupOutlineMaterials();
        }

        private void SetupOutlineMaterials()
        {
            // Сохраняем оригинальные материалы для восстановления
            originalMaterials = new Material[renderers.Length][];
            outlineMaterials = new Material[renderers.Length][];

            for (int r = 0; r < renderers.Length; r++)
            {
                Material[] mats = renderers[r].materials;
                originalMaterials[r] = mats;
                outlineMaterials[r] = new Material[mats.Length];

                for (int i = 0; i < mats.Length; i++)
                {
                    Material outlineMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    outlineMat.CopyPropertiesFromMaterial(mats[i]);
                    outlineMat.SetColor("_BaseColor", outlineColor);
                    outlineMat.SetFloat("_Mode", 0);
                    outlineMaterials[r][i] = outlineMat;
                }
            }
        }

        public void SetOutlineEnabled(bool enabled)
        {
            if (isOutlineEnabled == enabled) return;
            if (originalMaterials == null) return;

            isOutlineEnabled = enabled;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (enabled)
                {
                    // Переключаем на заранее созданные outline материалы
                    renderers[i].materials = outlineMaterials[i];
                }
                else
                {
                    // Возвращаем оригинальные
                    renderers[i].materials = originalMaterials[i];
                }
            }
        }

        private void OnDisable()
        {
            SetOutlineEnabled(false);
        }

        private void OnDestroy()
        {
            if (outlineMaterials != null)
            {
                foreach (var mats in outlineMaterials)
                {
                    if (mats == null) continue;
                    foreach (var mat in mats)
                    {
                        if (mat != null) Destroy(mat);
                    }
                }
            }
        }
    }
}
