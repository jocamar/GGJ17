﻿Shader "Custom/TextureMask" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Alpha("Alpha (A)", 2D) = "black" {}
		}
		SubShader{
			Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

			ZWrite Off

			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB

			Pass{
				SetTexture[_MainTex]{
				Combine texture
			}
				SetTexture[_Alpha]{
				Combine previous, texture
			}
		}
	}
}
