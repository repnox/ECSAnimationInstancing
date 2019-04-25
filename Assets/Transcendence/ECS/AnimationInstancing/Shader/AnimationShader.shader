Shader "Transcendence/AnimationShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        // Y Axis represents vertex index, X Axis is time.
        [NoScaleOffset]
        _AnimTex ("Animation Texture", 2D) = "black" {}
        
        // Animation Params: (X Offset, Y Offset, Speed, Duration)
        // Duration: ex. if the animation spans the entire width of the texture, Duration is 1. 50% width of the texture, .5 etc
        _Anim ("Animation Params", Vector) = (0,0,0,0)
        
        // The starting point of the animation, to prevent synchronized animations
        _AnimPhase ("Animation Phase", Range(0,1)) = 0.0
 
        
    }
    
    CGINCLUDE
    
        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        
        sampler2D _AnimTex;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Anim)
            UNITY_DEFINE_INSTANCED_PROP(float, _AnimPhase)
        UNITY_INSTANCING_BUFFER_END(Props)

        // Requires vertex color red channel to represent vertex index.
        void vert (inout appdata_full v) {
            float4 currentAnimation = UNITY_ACCESS_INSTANCED_PROP(Props, _Anim);
            float phase = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimPhase);
        
            float2 offset = float2(currentAnimation.x, currentAnimation.y);
            
            float scaledTime = _Time.y * currentAnimation.z + phase;
            float loopTime = fmod(scaledTime, currentAnimation.a);
            
            float vertexIndex = v.color.r;
            
            float2 uv = float2(loopTime, vertexIndex) + offset;
            
            float3 position = tex2Dlod(_AnimTex, float4(uv.x, uv.y, 0, 0));
            
            v.vertex.xyz = position;
        }
    
    ENDCG
    
    SubShader
    {
        Tags {"LightMode"="ShadowCaster"}
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
        }
        ENDCG
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
