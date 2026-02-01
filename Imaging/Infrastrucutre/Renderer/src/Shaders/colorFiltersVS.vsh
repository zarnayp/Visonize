// Vertex Shader
// Fullscreen triangle vertex shader
struct VSOut
{
    float4 Pos : SV_Position;
    float2 UV : TEXCOORD0;
};

VSOut main(uint vid : SV_VertexID)
{
    VSOut output;

    float2 positions[3] =
    {
        float2(-1.0, -1.0),
        float2(-1.0, 3.0),
        float2(3.0, -1.0)
    };

    float2 uvs[3] =
    {
        float2(0.0, 0.0),
        float2(0.0, 2.0),
        float2(2.0, 0.0)
    };

    output.Pos = float4(positions[vid], 0.0, 1.0);
    output.UV = uvs[vid];
    return output;
}