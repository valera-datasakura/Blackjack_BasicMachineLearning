
Shader "OutLine" {

	Properties{

		_OutlineColor("Outline Color", Color) = (0,0,0,1)
	}// property

		SubShader{

		Color[_OutlineColor]

		Pass{

		Blend SrcAlpha OneMinusDstAlpha
		ZWrite Off
		Lighting Off
		Cull Front
	}
		
		 }

}