struct VS_INPUT {
	float4 position: POSITION;
	float2 uv : TEXCOORD;
};

struct PS_INPUT {
	float4 position : SV_POSITION;
	float2 uv : TEXCOORD;
};

Texture2D g_texture : register(t0);
SamplerState g_sampler : register(s0);

PS_INPUT VS(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;
	output.position = input.position;
	output.uv = input.uv;

	return output;
}


float4 PS(PS_INPUT input) : SV_TARGET
{
    // General constants
    float PI = 3.14159265359;
    float4 BLANK_COLOR = float4(0.0, 0.0, 0.2, 1.0);

    // Constants for image distortion
    float2 CENTER = float2(0.5, 0.5);
    float2 EXP = float2(2.0, 1.5);
    float2 ZOOM = float2(0.5, 0.5);

    float2 uv = input.uv;
    float2 off_center = uv - CENTER;

    float scale_x = off_center.x < 0.0 ? CENTER.x : 1.0 - CENTER.x;
    off_center.x *= (1.0 + ZOOM.x * pow(abs(off_center.x / scale_x), EXP.x)) / (1.0 + ZOOM.x);

    float scale_y = off_center.y < 0.0 ? CENTER.y : 1.0 - CENTER.y;
    off_center.y *= (1.0 + ZOOM.y * pow(abs(off_center.y / scale_y), EXP.y)) / (1.0 + ZOOM.y);
    
    uv = CENTER + off_center;
    
    
    //uv.x = sin(uv.x * PI / 2).x;
    //sin(uv * PI / 2);
	
    /*if (uv.x > 1.0 || uv.x < 0.0 || uv.y > 1.0 || uv.y < 0.0)
    {
        return BLANK_COLOR;
    }*/

    return g_texture.Sample(g_sampler, uv);
}