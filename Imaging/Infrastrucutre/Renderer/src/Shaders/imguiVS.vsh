// ImGui Vertex Shader
cbuffer Constants : register(b0)
{
    float4x4 modelMatrix;
    float4x4 viewMatrix;
    float4x4 projectionMatrix;
}

struct VSInput
{
    float2 pos : ATTRIB0;
    float2 uv  : ATTRIB1;
    float4 col : ATTRIB2;
};

struct PSInput
{
    float4 pos : SV_POSITION;
    float4 col : COLOR0;
    float2 uv  : TEXCOORD0;
};

PSInput main(VSInput input)
{
    float4 pos = float4(input.pos, 0.0, 1.0);
    pos = mul(viewMatrix, pos);
    pos = mul(projectionMatrix, pos);

   // output.pos = mul(output.pos, viewMatrix);
    
    PSInput output;
    output.pos = pos;
    output.col = input.col;
    
    //output.col = mul(projectionMatrix, float4(input.pos, 0.0, 1.0));
    //output.pos = float4(input.pos, 0.0, 1.0);
    
    output.uv = input.uv;
    
    return output;
}