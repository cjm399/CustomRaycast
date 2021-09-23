Shader "Unlit/Buildings"
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
        Tags { "RenderType"="Opaque"}
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

            uint pixelIndex;
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

            float4 frag(v2f i) : SV_Target
            {
                const float bit255 = 1.0f / 255.0f;

                //Due to rounding point errors, we need to multiply by spomething a little larger than 255; (8 bits go up to 255, colors range 0-255)
				uint r = i.color.r * 255.1f;
				uint g = i.color.g * 255.1f;
                //Green value increments when Red gets to 255, go g == 1, is the 256 element 
				float gVal = g * 256.0f;
				pixelIndex = gVal + r;

                //I know here that my lookup texture is 256x256 dimensions.
				float x = pixelIndex % 256;
				float y = pixelIndex / 256;
                
                //Divide pixelPos/textureDim to get from pixel space into UV space (0-1)
				pixelPos = float2(x / 256.0f, y / 256.0f);

				pixelColor = tex2D(_LookupTex, pixelPos);


				float4 col = float4(0,0,0,1);

                // Setup textures
				float4 texResidential = tex2D(_ResidentialTex, i.uvResidential);
				float4 texCommercial = tex2D(_CommercialTex, i.uvCommercial);
				float4 texIndustrial = tex2D(_IndustrialTex, i.uvIndustrial);
				float4 texSpecial = tex2D(_SpecialTex, i.uvSpecial);

                //Unknown Zone
                col = pixelColor.b == 0 ? float4(1, 0, 1, 1) : col;

                // Commercial
                col = pixelColor.b == bit255 ? _CommercialTint : col;

                // Industrial
                col = pixelColor.b == 2 * bit255 ? _IndustrialTint : col;

                // Residential
                col = pixelColor.b == 3 * bit255 ? _ResidentialTint : col;

                //Special Purpose
                col = pixelColor.b == 4 * bit255 ? _SpecialTint : col;

                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
