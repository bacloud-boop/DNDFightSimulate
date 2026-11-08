Shader "Tavern/Miniature"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
            struct v2f { float4 position:SV_POSITION; float3 normal:TEXCOORD0; };
            v2f vert(appdata v) { v2f o; o.position=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);return o; }
            fixed4 frag(v2f i):SV_Target { float n=max(0,dot(normalize(i.normal),normalize(float3(-.4,.8,-1))));return fixed4(_Color.rgb*(.38+.62*n),1); }
            ENDCG
        }
    }
}
