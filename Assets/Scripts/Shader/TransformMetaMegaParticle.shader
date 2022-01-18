Shader "Custom/TransformMetaMegaParticle"
{
    Properties
        {
            _MainTex("Texture", 2D) = "white" { }
        }
    SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "RenderType" = "TransParent"
                "LightMode" = "ForwardBase"
            }
            LOD 100
            ZWrite On
            //Cull Front
            //Blend OneMinusDstColor One
            Blend SrcAlpha OneMinusSrcAlpha
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                # include "UnityCG.cginc"
                # include "NoiseMath.cginc"
                # include "MathMetaBall.cginc"

                struct TransformParticle
                {
                    int isActive;
                    int targetId;
                    float2 uv;

                    float3 targetPosition;

                    float speed;
                    float3 position;

                    int useTexture;
                    float scale;

                    float4 velocity;

                    float3 horizontal;
                };

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float3 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float3 pos : TEXCOORD1;
                    float3 normal : NORMAL;
                    int useTex : TEXCOORD2;
                };

                StructuredBuffer<TransformParticle> _Particles;

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed _BaseScale;
                uniform float _ypos;

                #define PI 3.1415926535
                #define MAX_SPHERE_COUNT 256
                float4 _Spheres[MAX_SPHERE_COUNT];
                int _SphereCount;

                UNITY_DECLARE_TEX2DARRAY(_Textures);

                fixed3 rotate(fixed3 p, fixed3 rotation)
                {
                    fixed3 a = normalize(rotation);
                    fixed angle = length(rotation);

                    if (abs(angle) < 0.001)
                    {
                        return p;
                    }

                    fixed s = sin(angle);
                    fixed c = cos(angle);
                    fixed r = 1.0 - c;
                    fixed3x3 m = fixed3x3(
                        a.x * a.x * r + c,
                        a.y * a.x * r + a.z * s,
                        a.z * a.x * r - a.y * s,
                        a.x * a.y * r - a.z * s,
                        a.y * a.y * r + c,
                        a.z * a.y * r + a.x * s,
                        a.x * a.z * r + a.y * s,
                        a.y * a.z * r - a.x * s,
                        a.z * a.z * r + c
                    );
                    return mul(m, p);
                }
                struct output
                {
                    float4 col : SV_Target;
                    float depth : SV_Depth;
                };


                // [“xŒvŽZ
                inline float getDepth(float3 pos)
                {
                    const float4 vpPos = mul(UNITY_MATRIX_VP, float4(pos, 1.0));
    
                    float z = vpPos.z / vpPos.w;
    
                    #if defined(SHADER_API_GLCORE) || \
                    defined(SHADER_API_OPENGL) || \
                    defined(SHADER_API_GLES) || \
                    defined(SHADER_API_GLES3)
                    return z * 0.5 + 0.5;
                    #else
                    return z;
                    #endif
                }
                float getDistance(float3 pos)
                {
                    float dist = 100000;
                    for (int i = 0; i < _SphereCount; i++)
                    {
                        dist = SmoothMin(dist, sphereDistanceFunction(_Spheres[i], pos), 3);
                    }
                    return dist;
                }

                v2f vert(appdata v, uint id : SV_InstanceID)
                {
                    TransformParticle p = _Particles[id];

                    v2f o;
                    float s = _BaseScale * p.scale * p.isActive;

                    fixed r = 2.0 * (rand(p.targetPosition.xy) - 0.5);
                    fixed3 r3 = fixed3(r, r, r) * (PI * 0.5 + _Time.y) * (p.speed * 0.1) * (1 - p.useTexture);
                    v.vertex.xyz = rotate(v.vertex.xyz, r3);

                    v.normal = rotate(v.normal, r3);

                    v.vertex.xyz = (v.vertex.xyz * s) + p.position;

                    o.vertex = mul(UNITY_MATRIX_VP, float4(v.vertex.xyz, 1.0));
                    o.normal = UnityObjectToWorldNormal(v.normal);
                    o.pos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.uv.xy = p.uv.xy;
                    o.uv.z = p.targetId;
                    o.useTex = p.useTexture;


                    return o;
                }

                output frag(v2f i, uint id : SV_InstanceID) : SV_Target
                {

                    TransformParticle p = _Particles[id];
                    output o;
                    fixed4 col;
                    float3 pos = i.pos.xyz;
                    const float3 rayDir = normalize(pos.xyz - _WorldSpaceCameraPos);

                    float4 sphere;
                    sphere.xyz = p.targetPosition;
                    sphere.w = p.scale;

                    if (i.useTex == 1)
                    {
                        col = UNITY_SAMPLE_TEX2DARRAY(_Textures, i.uv);
                        col = pow(col, 2.2);
                    }
                    else
                    {
                        float diff = clamp(dot(i.normal, normalize(float3(0.1, -1.0, 0))), 0.05, 0.8);
                        col.rgb = diff.xxx;
                        col.z = 0;
                    }

                    o.col = col;

                    for (int i = 0; i < _SphereCount; i++)
                    {
                        
                    }

                    for (int i = 0; i < 30; i++)
                    {
                        float dist = getDistance(pos);
                        
                        float dist = sphereDistanceFunction(sphere, pos);
                        if (dist < 0.001)
                        {
                            o.depth = getDepth(pos);
                            return o;
                        }
                        pos += dist * rayDir;
                    }

                    o.col = 0;
                    o.depth = 0;
                    return o;
                    //return col;
                }
                ENDCG
        }
    }
}