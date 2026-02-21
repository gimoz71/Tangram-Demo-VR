using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIGradient : MonoBehaviour, IMeshModifier
{
    [Header("Colori del Gradiente")]
    public Color colorTop = Color.white;
    public Color colorBottom = new Color(0.8f, 0.8f, 0.8f);

    private Graphic graphic;

    void OnEnable()
    {
        graphic = GetComponent<Graphic>();
        // Forza l'aggiornamento visivo quando attivi lo script
        if (graphic != null) graphic.SetVerticesDirty();
    }

    void OnValidate()
    {
        // Aggiorna il gradiente in tempo reale mentre cambi i colori nell'Editor
        if (graphic == null) graphic = GetComponent<Graphic>();
        if (graphic != null) graphic.SetVerticesDirty();
    }

    public void ModifyMesh(Mesh mesh) { }

    public void ModifyMesh(VertexHelper vh)
    {
        if (!isActiveAndEnabled || graphic == null) return;

        Rect rect = graphic.rectTransform.rect;
        UIVertex vertex = new UIVertex();

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            // Calcola l'altezza
            float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, vertex.position.y);

            // Applica il colore
            vertex.color *= Color.Lerp(colorBottom, colorTop, normalizedY);

            vh.SetUIVertex(vertex, i);
        }
    }
}