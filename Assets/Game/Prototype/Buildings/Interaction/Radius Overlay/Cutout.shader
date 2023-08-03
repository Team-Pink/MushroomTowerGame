Shader "Cutout"
{
	SubShader
	{
		Tags
		{
			"Queue" = "Geometry+1"
		}

		Pass
		{
			Blend Zero One
		}
	}
	FallBack "Diffuse"
}