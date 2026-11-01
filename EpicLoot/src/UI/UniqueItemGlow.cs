using UnityEngine;
using UnityEngine.UI;

namespace EpicLoot;

/// <summary>
/// A smoky glow inside a unique item's inventory cell: a haze that clings to the cell's rim, fades toward
/// the middle and drifts slowly along the edge.
/// </summary>
public class UniqueItemGlow : UniqueCellEffect
{
    private const float EdgeAlpha = 0.4f;
    private const float Depth = 16f;
    private const float DepthWobble = 0.35f;
    private const float DensityWobble = 0.45f;
    private const float BreathDepth = 0.15f;
    private const float BreathSpeed = 0.25f;
    private const float DriftSpeed = 0.5f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        BuildRoundedRect(rect.width * 0.5f - RimWidth, rect.height * 0.5f - RimWidth, RimRadius - RimWidth);
        Vector2 center = rect.center;

        float time = Time.unscaledTime;
        float breath = 1f - BreathDepth * (0.5f + 0.5f * Mathf.Sin(time * BreathSpeed * 2f * Mathf.PI));
        float drift = time * DriftSpeed;

        Color edge = Tint;
        Color clear = Tint;
        clear.a = 0f;

        foreach (Sample sample in Samples)
        {
            float density = 1f + DensityWobble * Wisp(sample.P, drift, 0f);
            float depth = Depth * breath * (1f + DepthWobble * Wisp(sample.P, drift * 0.7f, 2.4f));

            edge.a = Tint.a * EdgeAlpha * density * breath;
            Color mid = edge;
            mid.a *= 0.4f;

            vh.AddVert(center + sample.Point, edge, Vector2.zero);
            vh.AddVert(center + sample.Point + sample.Inward * (depth * 0.3f), mid, Vector2.zero);
            vh.AddVert(center + sample.Point + sample.Inward * depth, clear, Vector2.zero);
        }

        AddRingTriangles(vh, 3);
    }

    // Periodic in p, so the wisps meet themselves around the cell; -1..1.
    private static float Wisp(float p, float drift, float phase)
    {
        float turn = p * 2f * Mathf.PI;
        return 0.5f * Mathf.Sin(3f * turn + drift + phase)
            + 0.3f * Mathf.Sin(5f * turn - drift * 1.6f + phase * 1.7f)
            + 0.2f * Mathf.Sin(2f * turn + drift * 0.6f + phase * 0.3f);
    }
}
