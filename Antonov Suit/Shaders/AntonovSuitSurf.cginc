
#ifndef ANTONOV_SUIT_SURF_CGINC
#define ANTONOV_SUIT_SURF_CGINC


void AntonovSuiSurf(Input IN, inout AntonovSuitOutput OUT) 
{

		#define uv_metallic IN.uv_MainTex
		#define uv_base IN.uv_MainTex
		#define uv_diff IN.uv_MainTex
		#define uv_spec IN.uv_MainTex
		#define uv_bump IN.uv_MainTex
		#define uv_occlusion IN.uv_MainTex
		
		//Basic stuff
		half3 white = half3(1.0,1.0,1.0);
		half3 black = half3(0.0,0.0,0.0);

}

#endif