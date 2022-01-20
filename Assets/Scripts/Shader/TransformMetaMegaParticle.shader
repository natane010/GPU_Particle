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
            //Cull Back
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
                    uint id : SV_InstanceID;
                };

                StructuredBuffer<TransformParticle> _Particles;

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed _BaseScale;
                uniform float _ypos;

                #define PI 3.1415926535
                

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

                float getDistance(float3 pos, float size)
                {
                    float dist = 100000;
                    for (int i = 0; i < _SphereCount; i++)
                    {
                        dist = SmoothMin(dist, sphereDistanceFunction(_Spheres[i], pos), size);
                    }
                    return dist;
                }

                // –@ü‚ÌŽZo
                float3 getNormal(float3 pos, float size)
                {
                   float d = 0.0001;
                   return normalize(float3(
                       getDistance(pos, size) - getDistance(pos + float3(-d, 0.0, 0.0), size),
                       getDistance(pos, size) - getDistance(pos + float3(0.0, -d, 0.0), size),
                       getDistance(pos, size) - getDistance(pos + float3(0.0, 0.0, -d), size)
                   ));
                }

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
                fixed3 _Colors[MAX_SPHERE_COUNT];

                fixed3 getColor(const float3 pos)
                {
                   fixed3 color = fixed3(0, 0, 0);
                   float weight = 0.01;
                   for (int i = 0; i < _SphereCount; i++)
                   {
                       const float distinctness = 0.7;
                       const float4 sphere = _Spheres[i];
                       const float x = clamp((length(sphere.xyz - pos) - sphere.w) * distinctness, 0, 1);
                       const float t = 1.0 - x * x * (3.0 - 2.0 * x);
                       color += t * _Colors[i];
                       weight += t;
                   }
                   color /= weight;
                   return float4(color, 1);
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
                    o.id = id;

                    return o;
                }

                output frag(v2f i)
                {

                    TransformParticle p = _Particles[i.id];
                    output o;
                    fixed4 col;
                    float3 pos = i.pos.xyz;
                    const float3 rayDir = normalize(pos.xyz - _WorldSpaceCameraPos);
                    const half3 halfDir = normalize(_WorldSpaceLightPos0.xyz - rayDir);
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
                        col.w = 1;
                    }

                    o.col = col;

                    
                    //float dist = sphereDistanceFunction(sphere, pos);
                    

                    for (int i = 0; i < 30; i++)
                    {
                        float dist = getDistance(pos);
                        
                        
                        if (dist < 0.001)
                        {
                            fixed3 norm = getNormal(pos);
                            fixed3 baseColor = getColor(pos);
                            baseColor = (col + baseColor) / 2;
                            //fixed3 color = col;
                            const float rimPower = 2;
                            const float rimRate = pow(1 - abs(dot(norm, rayDir)), rimPower);
                            const fixed3 rimColor = fixed3(1.5, 1.5, 1.5);
                            float highlight = dot(norm, halfDir) > 0.99 ? 1 : 0;
                            fixed3 color = clamp(lerp(baseColor, rimColor, rimRate) + highlight, 0, 1);
                            float alpha = clamp(lerp(0.2, 4, rimRate) + highlight, 0, 1);
                            o.col += fixed4(color, alpha);
                            o.depth = getDepth(pos);
                            return o;
                        }
                        pos += dist * rayDir;
                    }

                    //o.col.a = ;
                    //.depth = 0;
                    return o;
                    //return col;
                }
                ENDCG
        }
    }
}