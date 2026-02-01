struct VertexInput
{
    float3 position : ATTRIB0;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float2 texCoord : TEXCOORD0;
};

PixelInput main(VertexInput input)
{
    PixelInput output;

    // Pass through vertex position
    output.position = float4(input.position, 1);

    // Calculate texture coordinate for full-screen quad
    output.texCoord = input.position.xy * 0.5 + 0.5; // Assuming input.position is in range [-1, 1]

    return output;
}