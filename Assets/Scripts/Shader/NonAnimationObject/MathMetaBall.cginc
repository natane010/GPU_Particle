#ifndef __MATHMETABALL_CGINC__
#define __MATHMETABALL_CGINC__

//補完のための関数
float SmoothMin(float d1,float d2,float k)
{
	return -log(exp(-k*d1)+exp(-k*d2))/k;
}
//距離関数
float ball(float3 p,float s)
{
	return length(p)-s;
}
//球の距離関数↑より正確
float4 sphereDistanceFunction(float4 sphere, float3 pos)
{
    return length(sphere.xyz - pos) - sphere.w;
}
#endif