Shader "Custom/TransformMetaParticle"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ypos("floor height",float)=-0.25

    }
    SubShader
    {
        Tags 
        {
            "Queue" = "Transparent"
            "RenderType" = "TransParent"
            "LightMode" = "ForwardBase"
        }
        LOD 300
        ZWrite On
        Cull Front
        Blend OneMinusDstColor One
        //Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
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
                uint id : TEXCOORD3;
            };

            struct pout
            {
                float4 pixel: SV_Target;
	            float depth : SV_Depth;

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
            //メタボール位置決定
            float4 Metaballvalue(float i, TransformParticle p)
            {
	            float3 ballpos = 0.3 * p.position;
	            float scale = p.scale;
	            return  float4(ballpos,scale);
            }
            //ボールステータス反映関数
            float Metaballone(float3 p, float i, TransformParticle t)
            {	
	            float4 value = Metaballvalue(i, t);
	            float3 ballpos = p-value.xyz;
	            float scale = value.w;
	            return  ball(ballpos,scale);
            }

            //メタボール本体
            float Metaball(float3 p, TransformParticle t)
            {
	            float d1;
	            float d2 =  Metaballone(p, 0, t);
	            for (int i = 1; i < 6; ++i) {
				
		            d1 = Metaballone(p, i, t);
		            d1 = SmoothMin(d1,d2,t.scale);
		            d2 =d1;
		            }
	            return d1;
            }

            

            //最終的な距離
            float dist(float3 p, TransformParticle t)
            {	
	            float y = p.y;
	            float d1 = Metaball(p, t);
	            float d2 = y - (_ypos); //For floor
	            d1 = SmoothMin(d1, d2, t.scale);
	            return d1;
            }
            //レイマーチ
            float raymarch (float3 ro,float3 rd, TransformParticle p)
            {
	            float previousradius = 0.0;
	            float maxdistance = 3;
	            float outside = dist(ro, p) < 0 ? -1 : +1;//レイのスタート地点が物体の中にあるか外にあるか
	            float pixelradius = 0.02;
	            float omega = 1.2;
	            float t =0.0001;
	            float step = 0;
	            float minpixelt =999999999;
	            float mint = 0;
	            float hit = 0.01;
		        for (int i = 0; i < 60; ++i) 
                {

			        float radius = outside * dist(ro + rd * t, p);
			        bool fail = omega>1 &&
				        step>(abs(radius)+abs(previousradius));
			        if(fail)
                    {
				        step -= step *omega;
				        omega =1.0;
			        }
			        else
                    {
				        step = omega * radius;
			        }
			        previousradius = radius;
			        float pixelt = radius/t;
			        if(!fail&&pixelt<minpixelt)
                    {
				        minpixelt = pixelt;
				        mint = t;
			        }
			        if(!fail&&pixelt<pixelradius||t>maxdistance)
			        break;
			        t += step;
		        }
				
		        if ((t > maxdistance || minpixelt > pixelradius)&&(mint>hit)){
		        return -1;
		        }
		        else{
		        return mint;
		        }
				
            }
            //法線を求める
            float3 getnormal( in float3 p, TransformParticle t)
            {
	            static const float2 e = float2(0.5773,-0.5773)*0.0001;
	            float3 nor = normalize( e.xyy*dist(p+e.xyy, t) +
 		            e.yyx*dist(p+e.yyx, t) + e.yxy*dist(p+e.yxy, t) + e.xxx*dist(p+e.xxx, t));
	            nor = normalize(float3(nor));
	            return nor ;
            }
            //影をつける。
            float softray( float3 ro, float3 rd , float hn, TransformParticle p)
            {
	            float t = 0.000001;
	            float jt = 0.0;
	            float res = 1;
	            for (int i = 0; i < 20; ++i) {
		            jt = dist(ro+rd*t, p);
		            res = min(res,jt*hn/t);
		            t = t+ clamp(0.02,2,jt);
	            }
	            return saturate(res);
            }
            //最終的な色を計算
            float4 material(float3 pos, TransformParticle t)
            {
	            float4 ballcol[6]={float4(0.5,0,0,1),
					            float4(0.0,0.5,0,1),
					            float4(0,0,0.5,1),
					            float4(0.25,0.25,0,1),
					            float4(0.25,0,0.25,1),
					            float4(0.0,0.25,0.25,1)};
	            float3 mate = float3(0,0,0);
	            float w = 0.01;
		        // Making ball color
		        for (int i = 0; i < 6; ++i) {
			        float x = clamp( (length( Metaballvalue(i, t).xyz - pos) - Metaballvalue(i, t).w) * 10,0,1 ); 
			        float p = 1.0 - x*x*(3.0-2.0*x);
			        mate += p*float3(ballcol[i].xyz);
			        w += p;
		        }
	            // Making floor color
	            float x = clamp(  (pos.y-_ypos)*10,0,1 );
	            float p = 1.0 - x*x*(3.0-2.0*x);
	            mate += p*float3(0.2,0.2,0.2);
	            w += p;
	            mate /= w;
	            return float4(mate,1);
            }
            //ライティング
            float4 lighting(float3 pos, TransformParticle t)
            {	
	            float3 mpos = pos;
	            float3 normal = getnormal(mpos, t);
	
	            pos =  mul(unity_ObjectToWorld,float4(pos,1)).xyz;
	            normal =  normalize(mul(unity_ObjectToWorld,float4(normal,0)).xyz);
					
	            float3 viewdir = normalize(pos-_WorldSpaceCameraPos);
	            half3 lightdir = normalize(UnityWorldSpaceLightDir(pos));	
	            float sha = softray(mpos,lightdir,3.3, t);
                
				
	            float NdotL = max(0,dot(normal,lightdir));
	            float3 R = -normalize(reflect(lightdir,normal));
	            float3 spec =pow(max(dot(R,-viewdir),0),10);

	            float4 col =  sha*(NdotL+float4(spec,0));
	            return col;
            }

            v2f vert(appdata v, uint id : SV_InstanceID)
            {
                TransformParticle p = _Particles[id];

                v2f o;
                o.id = id;
                float s = _BaseScale * p.scale * p.isActive;

                fixed r = 2.0 * (rand(p.targetPosition.xy) - 0.5);
                fixed3 r3 = fixed3(r, r, r) * (PI * 0.5 + _Time.y) * (p.speed * 0.1) * (1 - p.useTexture);
                v.vertex.xyz = rotate(v.vertex.xyz, r3);

                v.normal = rotate(v.normal, r3);

                v.vertex.xyz = (v.vertex.xyz * s) + p.position;

                o.vertex = mul(UNITY_MATRIX_VP, float4(v.vertex.xyz, 1.0));
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.pos = mul(unity_ObjectToWorld,v.vertex).xyz;
                o.uv.xy = p.uv.xy;
                o.uv.z = p.targetId;
                o.useTex = p.useTexture;
                

                return o;
            }

            pout frag(v2f i)
            {
                TransformParticle p = _Particles[i.id];

                fixed4 col;

                float3 ro = mul( unity_WorldToObject,float4(_WorldSpaceCameraPos,1)).xyz;
	            float3 rd = normalize(mul( unity_WorldToObject,float4(i.pos,1)).xyz
			                -mul( unity_WorldToObject,float4(_WorldSpaceCameraPos,1)).xyz); 
	            float t = raymarch(ro,rd,p);

                if (i.useTex == 1)
                {
                    col = UNITY_SAMPLE_TEX2DARRAY(_Textures, i.uv);
                    col = pow(col, 2.2);
                }
                else
                {
                    float diff = clamp(dot(i.normal, normalize(float3(0.1, -1.0, 0))), 0.05, 0.8);
                    col = diff.xxxx;
                    //col.w = 0;
                }

                if (t == -1) 
                {
	                clip(-1);
	            }
	            else
                {
	                float3 pos = ro + rd * t;
	                col += lighting(pos, p);
                    
	            }

                pout o;
                o.pixel = col / 2;
                float4 curp = UnityObjectToClipPos(float4(ro + rd * t, 1));
                o.depth = (curp.z) / (curp.w); //Drawing depth

                return o;
                //return col;
            }
            ENDCG
        }
    }
}
