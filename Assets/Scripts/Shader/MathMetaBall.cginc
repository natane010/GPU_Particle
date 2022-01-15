#ifndef __MATHMETABALL_CGINC__
#define __MATHMETABALL_CGINC__

//•âŠ®‚Ì‚½‚ß‚ÌŠÖ”
float SmoothMin(float d1,float d2,float k)
{
	return -log(exp(-k*d1)+exp(-k*d2))/k;
}
//‹——£ŠÖ”
float ball(float3 p,float s)
{
	return length(p)-s;
}

#endif