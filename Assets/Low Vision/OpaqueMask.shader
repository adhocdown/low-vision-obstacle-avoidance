Shader "Custom/OpaqueMask"
{
	// Helpful Resources for Understanding Shaders
	// Martin Kraus's Wiki Book -  https://en.wikibooks.org/wiki/GLSL_Programming/Unity 
	// Coding in Black - https://www.codinblack.com/shader-development-tutorials-from-scratch/
	// Catlikecoding - https://catlikecoding.com/unity/tutorials/ 
	// Ronja's Shader Tutorials - https://www.ronja-tutorials.com/
	// Unity Gems - https://unitygem.wordpress.com/shader-part-1/ 
	
	// Note on Unity shader changes!! 
	// Unity 2021+ now uses HLSL instead of CG.. and other changes
	// Cg is only the default for the built-in renderer. URP/HDRP defaults to using HLSL code.
	// https://developer.arm.com/documentation/102487/0100/Migrating-built-in-shaders-to-the-Universal-Render-Pipeline

	// Block Structure for Shader
	// 1. Properties Block
	// 2. SubShader Block
	// 3. Pass Block 
	// 4. CG Program Block 
	//	  a. Vertex Shader Block
	//    b. Fragment Shader Block


	// Poperties Block - Defines properties you can use in the inspector 
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MaskTex("Mask Texture", 2D) = "white" {}
		_MaskColor("Mask Color", Color) = (0,0,0,1)
		_MaskValue("Mask Value", Range(0,6)) = 0.5
		_MaskSpread("Mask Spread", Range(0,1)) = 0.5

		[Toggle(INVERT_MASK)] INVERT_MASK("Mask Invert", Float) = 0

		_XScale("X Scale", Float) = 1
		_YScale("Y Scale", Float) = 1
		_XTrans("X Trans", Float) = 1
		_YTrans("Y Trans", Float) = 1
    }
	// SubShader Block - Defines the rendering pass
    SubShader
    {
		Tags{ "Queue" = "Overlay" }
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
		
		// Pass Block - determines actual rendernig configuration 
        Pass
        {
			// CG Program Block - uses CG to compile code into low level shader assembly 
            CGPROGRAM
			// Compiler Directives - define functions needed to run before program startup (e.g., vertex, fragment are compiler directives)            
			// UnityCG.cginc includes Unity Builtin Library 
			#pragma vertex vert
            #pragma fragment frag
			#pragma shader_feature INVERT_MASK
			#include "UnityCG.cginc"


			// Data Structures for holding our data
			float4 _MainTex_TexelSize;
			float _XScale;
			float _YScale;
			float _XTrans;
			float _YTrans;

			// appdata used as input for vertex shader f(x). 
            struct appdata						
            {
                float4 vertex	: POSITION;		// vertex position
                float2 uv		: TEXCOORD0;	// texture coordinate
				float2 uv2		: TEXCOORD1;	// "
            };

			// v2f used as input for fragment shader f(x). Stores vertex & uv
            struct v2f							
            {
				float4 vertex	: SV_POSITION;	// clip space position
				float2 uv		: TEXCOORD0;	// texture coordinate
				float2 uv2		: TEXCOORD1;	// "
            };

			// Vertex Shader Block -  
			// - input is appdata (vertex information) 
			// - returns v2f for use in fragment shader (later) 	

			// vertex output: transform the vertex position to clip space (3d to 2d)
			// by multiplying the vertex position with the model*view*projection matrix.
			// uv output: pass down uv mapping data from mesh to the v2f data structure. 
			// uv2 output: scale and translate texture map
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);                
				// manipulate based on eye screen dimensions/resolution
				o.uv = v.uv;
				o.uv2 = float2((v.uv2.x*_XScale) - _XTrans, (v.uv2.y*_YScale) - _YTrans); // import this factor from start

				#if UNITY_UV_STARTS_AT_TOP	//VERTICAL FLIP IF NECESSARY 
					if (_MainTex_TexelSize.y < 0) {
						o.uv.y = 1 - o.uv.y;
						o.uv2.y = 1 - o.uv2.y;
					}
				#endif

                return o;
            }

			// More Data Structures  (texture and mask information) for fragment shader
			sampler2D _MainTex;
			sampler2D _MaskTex;
			float4 _MaskColor;
			float _MaskValue;
			float _MaskSpread;

			// Fragment Shader Block - 
			// returns fixed4 - the color of the pixel (r,g,b, a) // NOTE: could probably use just fixed1 for single channel texture... 
			// use tex2D to map textures (_MainTex, _MaskTex) to the uv map passed from v2f input
			// 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				float4 mask = tex2D(_MaskTex, i.uv2);
				float4 p = i.vertex;	   // true pixel value

				// Scale 0..255 to 0..254 range.
				float alpha = mask.a * (1 - 1 / 255.0);
				float weight = 0;
			
				if (mask.a >= 0.01) {
					weight = smoothstep(_MaskValue - _MaskSpread, _MaskValue, alpha);
				}

				// If the mask value is greater than the alpha value,
				// we want to draw the mask.
				#if INVERT_MASK
					weight = 1 - weight;
				#endif

				// Blend in mask color depending on the weight
				col.rgb = lerp(_MaskColor, col.rgb, weight);
				// Additionally also apply a blend between mask and scene
				//col.rgb = lerp(col.rgb, lerp(_MaskColor.rgb, col.rgb, weight), _MaskColor.a);


                return col;
            }
            ENDCG // end of CG PROGRAM Block
        }
    }
}
