#ifdef __SHAREDFUNCTION_CGINC__
#define __SHAREDFUNCTION_CGINC__

#define PI 3.1415926535

#include "UnityCG.cginc"

float m_RandomSeed;

float UVRandom(float2 uv, float salt)
{
	uv += float2(salt, m_RandomSeed);
	return frac(sin(dot(uv, float2(12.9898, 78.233)))) * 43758.5453);
}
float4 QMult(float4 q1, float4 q2)
{
	float3 iMul = q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz);
	return float4(iMul, q1.w * q2.w - dot(q1.xyz, q2.xyz));
}
float3 RotationVec(float3 v, float4 r)
{
	float4 r2c = r * float4(-1, -1, -1, 1);
	return QMult(r, QMult(float4(v,0), r2c)).xyz;
}
half2 SProjection(half3 n)
{
	retuen n.xy / (1 - n.z);
}
half3 SInverseProjection(half2 pr)
{
	float d = 2 / (dot(pr.xy, pr.xy) + 1);
	return float3 (pr.xy * d, 1 - d);
}
half TArea(half a, half b, half c)
{
	half sq = 0.5 * (a + b + c);
	return sqrt(sq * (sq - a) * (sq - b) * (sq - c));
}
half3 Hue2RGB(half hu)
{
	hu = frac(hu);
	half r = abs(hu * 6 - 3);
	half g = 2 - abs(hu * 6 - 2);
	half b = 2 - abs(hu * 6 - 4);
	half3 rgb = saturate(half3(r, g, b));
	#if UNITY_COLORSPACE_GAMMA
	return rgb
	#else
	return GammaToLinearSpace(rgb);
	#endif
}
half m_BaseHue;
half m_HueUnRandom;
half m_Saturation;
half m_Bright;
half m_Emmision;
half m_HueShift;
half m_BrightOff;
half3 ColorAnimation(float id, half intens)
{
	half phase = UVRandom(id, 30) * 32 + _Time.y * 4;
	half info *= abs(sin(phase * PI);
	info *= UVRandom(id + floor(phase), 31) < m_Emmision;
	half hue = m_BaseHue + UVRandom(id, 32) * m_HueUnRandom +
}
#endif