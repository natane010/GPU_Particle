Shader "Custom/TestShader"
{
Properties
{
_Color("Color", Color) = (1, 1, 1, 1)
    _Intensity("Intensity", Range(0, 1)) = 0.1
    [IntRange] _Loop("Loop", Range(0, 128)) = 32
}

CGINCLUDE

#include "UnityCG.cginc"
struct TransformParticle
{
    int isActive;
    int targetId;
    float2 uv;
    float3 targetPosition;
    float speed;
    float3 position;
    float scale;
    float4 velocity;
    float3 horizontal;
};
struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    
};

struct v2f
{
    float4 vertex   : SV_POSITION;
    float3 worldPos : TEXCOORD1;
    float3 normal : NORMAL;
};

#define MAX_LOOP 100
float4 _Color;
float _Intensity;
int _Loop;

inline float densityFunction(float3 p)
{
    return 0.5 - length(p);
}

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    // �|���S���\�ʂ̍��W���t���O�����g�V�F�[�_�Ŏg����悤�ɂ���
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    return o;
}

float4 frag(v2f i) : SV_Target
{
    // ���[���h��Ԃł̃|���S���\�ʍ��W�Ƃ����ւ̃J��������̌���
    float3 worldPos = i.worldPos;
    float3 worldDir = normalize(worldPos - _WorldSpaceCameraPos);

    // �I�u�W�F�N�g��Ԃɕϊ�
    float3 localPos = mul(unity_WorldToObject, float4(worldPos, 1.0));
    float3 localDir = UnityWorldToObjectDir(worldDir);

    // �I�u�W�F�N�g��Ԃł̃��C�̃X�e�b�v��
    float step = 1.0 / _Loop;
    float3 localStep = localDir * step;

    // ���C��ʉ߂����ē����铧�ߗ�
    float alpha = 0.0;

    for (int i = 0; i < _Loop; ++i)
    {
        // �|���S�����S�قǑ傫�Ȓl���Ԃ��Ă���
        float density = densityFunction(localPos);

        // ���̊O���ł̓}�C�i�X�̒l���Ԃ��Ă���̂ł����e��
        if (density > 0.001)
        {
            // ���ߗ��̑������킹
            alpha += (1.0 - alpha) * density * _Intensity;
        }

        // �X�e�b�v��i�߂�
        localPos += localStep;

        // �|���S���̊O�ɏo����I���
        if (!all(max(0.5 - abs(localPos), 0.0))) break;
    }
    float4 color = _Color;
    

   
    color.a *= alpha;
    return color;
}

ENDCG

SubShader
{

    Tags
    {
        "Queue" = "Transparent"
        "RenderType" = "Transparent"
    }

    Pass
    {
        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
    Lighting Off

    CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        ENDCG
    }

}
}
