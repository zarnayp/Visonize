cbuffer Constants
{
    float4x4 g_WorldViewProj;
    float4x4 InvViewRot;
};

struct VSInput
{
    float3 Pos : ATTRIB0;
    float3 Col : ATTRIB1;
};


struct PSInput
{
    float4 pos : SV_POSITION;
    float4 tex : TEXCOORD0; // Position in model coordinates
};

void main(in  VSInput VSIn,
          out PSInput PSIn) 
{
    PSIn.pos = mul(float4(VSIn.Pos, 1.0), g_WorldViewProj);
    PSIn.tex = float4(VSIn.Col, 1.0);
    
}
