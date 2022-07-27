// https://forum.unity.com/threads/make-a-seethrough-window-without-making-hole-in-the-wall.286393/ 
Shader "Custom/Wall" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_DetailTex("Detail Albedo", 2D) = "gray" {}
		_Color("Color", Color) = (1, 0, 0, 1)
		_StencilVal("stencilVal", Int) = 1
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			Stencil {
				Ref[_StencilVal]
				Comp NotEqual
			}

			CGPROGRAM
			#pragma surface surf Lambert

			sampler2D _MainTex;
			sampler2D _DetailTex;
			uniform fixed4 _Color;

			struct Input {
				float2 uv_MainTex;
				float2 uv_DetailTex;
			};


			void surf(Input IN, inout SurfaceOutput o) {
				half4 c1 = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				half4 c2 = tex2D(_DetailTex, IN.uv_DetailTex) * _Color;
				half4 c = lerp(c1, c2, 0.4); 
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}