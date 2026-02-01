cbuffer Constants
{
    float4x4 g_WorldViewProj;
    float4x4 InvView;
};


struct VSInput
{
    float4 Pos : ATTRIB0;
};

struct PSInput
{
    float4 pos : SV_POSITION;
    float xVal : TEXCOORD0;

};


void main(in VSInput VSIn,
          out PSInput PSIn)
{
    PSIn.pos = mul(float4(VSIn.Pos), g_WorldViewProj);
    
    float3x3 camRot = (float3x3) InvView;
    float3 rotatedPos = mul(float3(VSIn.Pos.x, VSIn.Pos.y, VSIn.Pos.z), (float3x3) InvView);
    PSIn.xVal = rotatedPos.z;
}