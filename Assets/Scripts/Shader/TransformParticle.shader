Shader "Custom/TransformParticle"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Intensity("Intensity", Range(0, 1)) = 0.1
        [IntRange]_Loop("_Loop", Range(0, 128)) = 32
    }

    
    
    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            }
        LOD 200
        

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
        #pragma fragment frag

        # include "UnityCG.cginc"
        # include "NoiseMath.cginc"

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
            float3 normal : NORMAL;
            float3 wpos : TEXCOORD1;
            int useTex : TEXCOORD2;
        };
        #define MAX_LOOP 100
        float _Intensity;
        int _Loop;
        StructuredBuffer<TransformParticle> _Particles;
        sampler2D _MainTex;
        float4 _MainTex_ST;
        fixed _BaseScale;
        #define PI 3.1415926535
        UNITY_DECLARE_TEX2DARRAY(_Textures);
        float densityFunction(float3 p)
        {
            return 0.5 - length(p);
        }
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
            o.uv.xy = p.uv.xy;
            o.uv.z = p.targetId;
            o.useTex = p.useTexture;
            o.wpos = mul(unity_ObjectToWorld, v.vertex);
            return o;
        }
        fixed4 frag(v2f i) : SV_Target
        {
            fixed4 col;
            float3 wpos = i.wpos;
            float3 wdir = normalize(wpos - _WorldSpaceCameraPos);
            float3 localPos = mul(unity_WorldToObject, float4(wpos, 1.0));
            float3 localDir = UnityWorldToObjectDir(wdir);
            float step = 1.0 / _Loop;
            float3 localStep = localDir * step;

            float alpha = 0.0;
            for (int a = 0; a < _Loop; ++a)
            {
                // ポリゴン中心ほど大きな値が返ってくる
                float density = densityFunction(localPos);

                // 球の外側ではマイナスの値が返ってくるのでそれを弾く
                if (density > 0.001)
                {
                    // 透過率の足し合わせ
                    alpha += (1.0 - alpha) * density * _Intensity;
                }

                // ステップを進める
                localPos += localStep;

                // ポリゴンの外に出たら終わり
                if (!all(max(0.5 - abs(localPos), 0.0)))
                {
                    break;
                }
            }

            if (i.useTex == 1)
            {
                col = UNITY_SAMPLE_TEX2DARRAY(_Textures, i.uv);
                col = pow(col, 2.2);
                col.a = alpha;
            }
            else
            {
                float diff = clamp(dot(i.normal, normalize(float3(0.1, -1.0, 0))), 0.05, 0.8);
                col = diff.xxxx;
                col.a = alpha;
            }
            
            return col;
        }
        ENDCG

        
        }
    }
}
