using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class UIGradient : BaseMeshEffect
{
    public Color leftColor = Color.white;
    public Color rightColor = Color.black;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0)
            return;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        UIVertex vertex = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            if (vertex.position.x < minX) minX = vertex.position.x;
            if (vertex.position.x > maxX) maxX = vertex.position.x;
        }

        float width = maxX - minX;
        if (width <= 0f) return;

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            float t = (vertex.position.x - minX) / width;
            vertex.color *= Color.Lerp(leftColor, rightColor, t);
            vh.SetUIVertex(vertex, i);
        }
    }
}   