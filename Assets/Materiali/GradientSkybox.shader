Shader "Custom/ZenithNadirSkybox"
{
    Properties
    {
        _ColorZenith ("Colore Zenith (Alto)", Color) = (0.1, 0.1, 0.2, 1)
        _ColorNadir ("Colore Nadir (Basso)", Color) = (0.0, 0.0, 0.0, 1)
        _Intensity ("Luminosità", Range(0, 2)) = 1.0
        _Exponent ("Morbidezza Gradiente", Range(0.1, 5.0)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            fixed4 _ColorZenith;
            fixed4 _ColorNadir;
            half _Intensity;
            half _Exponent;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalizza la direzione (da -1 in basso a +1 in alto)
                float3 dir = normalize(i.texcoord);
                
                // Mappa il valore da ( -1 a 1 ) a ( 0 a 1 ) per sfumare i colori
                float t = (dir.y + 1.0) * 0.5;
                
                // Applica l'esponente per spostare il punto di fusione
                t = pow(t, _Exponent);

                // Miscela i due colori
                return lerp(_ColorNadir, _ColorZenith, t) * _Intensity;
            }
            ENDCG
        }
    }
}