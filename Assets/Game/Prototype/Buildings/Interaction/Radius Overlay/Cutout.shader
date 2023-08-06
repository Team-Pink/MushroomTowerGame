Shader "Cutout"
{
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent+2"
		}

		Pass
		{
			Blend Zero One
		}
	}
	FallBack "Diffuse"
}