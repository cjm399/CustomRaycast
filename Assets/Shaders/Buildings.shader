﻿Shader "Unlit/Buildings"
{
    Properties
    {
        _LookupTex ("Texture", 2D) = "white" {}

        _ResidentialTex ("ResidentialTexture", 2D) = "white" {}
        _ResidentialTint ("ResidentialTint", Color) = (1.0, 1.0, 1.0, 1.0)

        _CommercialTex ("CommercialTexture", 2D) = "white" {}
        _CommercialTint ("CommercialTint", Color) = (1.0, 1.0, 1.0, 1.0)

        _IndustrialTex ("IndustrialTexture", 2D) = "white" {}
        _IndustrialTint ("IndustrialTint", Color) = (1.0, 1.0, 1.0, 1.0)

        _SpecialTex ("SpecialTexture", 2D) = "white" {}
        _SpecialTint ("SpecialTint", Color) = (1.0, 1.0, 1.0, 1.0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uvResidential : TEXCOORD0;
                float2 uvCommercial : TEXCOORD1;
                float2 uvIndustrial : TEXCOORD2;
                float2 uvSpecial : TEXCOORD3;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uvResidential : TEXCOORD0;
                float2 uvCommercial : TEXCOORD1;
                float2 uvIndustrial : TEXCOORD2;
                float2 uvSpecial : TEXCOORD3;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _LookupTex;
            float4 _LookupTex_ST;
            float4 _LookupTex_TexelSize;
            
            sampler2D _ResidentialTex;
            float4 _ResidentialTex_ST;
            float4 _ResidentialTint;

            sampler2D _CommercialTex;
            float4 _CommercialTex_ST;
            float4 _CommercialTint;

            sampler2D _IndustrialTex;
            float4 _IndustrialTex_ST;
            float4 _IndustrialTint;

            sampler2D _SpecialTex;
            float4 _SpecialTex_ST;
            float4 _SpecialTint;

            float pixelIndex;
            float2 pixelPos;
            float4 pixelColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvResidential = TRANSFORM_TEX(v.uvResidential, _ResidentialTex);
                o.uvCommercial = TRANSFORM_TEX(v.uvCommercial, _CommercialTex);
                o.uvIndustrial = TRANSFORM_TEX(v.uvIndustrial, _IndustrialTex);
                o.uvSpecial = TRANSFORM_TEX(v.uvSpecial, _SpecialTex);
                o.color = v.color;
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Setup the vertex color to pixel map
                pixelIndex = i.color.a + (i.color.b * 255);// + (i.color.g*255*255) + (i.color.r*255*255*255);
                pixelPos = float2(int(pixelIndex)%512, floor(pixelIndex/512));
                pixelColor = tex2D(_LookupTex, pixelPos / float2(512,512));

                // Setup textures
                fixed4 texResidential = tex2D(_ResidentialTex, i.uvResidential);
                fixed4 texCommercial = tex2D(_CommercialTex, i.uvCommercial);
                fixed4 texIndustrial = tex2D(_IndustrialTex, i.uvIndustrial);
                fixed4 texSpecial = tex2D(_SpecialTex, i.uvSpecial);

                // Check the pixel color and assign textures/colors based on classification
                fixed4 col = fixed4(0, 0, 0, 0);// i.color;

                // Commercial
                col = pixelColor.b >= .4f ? _CommercialTint : col;

                // Industrial
                col = pixelColor.b >= .7f ? _IndustrialTint: col;

                // Residential
                col = pixelColor.b >= .9f ? _ResidentialTint : col;

                // Special
                //col = pixelColor.b <= .4f ? _SpecialTint : col;


                //col = pixelColor.b <= 0 ? 0 : col;




                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
