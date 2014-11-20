// Custom texture processor
// Made by Charles Greivelding 28.09.2013

#if UNITY_EDITOR
class AntonovSuitTextureProcessor extends AssetPostprocessor 
{
	function OnPreprocessTexture () 
	{
	
		var textureSize : int = 2048;
	
		if (assetPath.Contains("_COLOR") || assetPath.Contains("_ILLUM") || assetPath.Contains("_DIFF"))  
		{
        	var diffuseTextureImporter : TextureImporter = assetImporter;
        	diffuseTextureImporter.isReadable = true;
        	diffuseTextureImporter.textureType = TextureImporterType.Image;
        	diffuseTextureImporter.filterMode = FilterMode.Trilinear;
        	diffuseTextureImporter.anisoLevel = 9;
        	diffuseTextureImporter.textureFormat = TextureImporterFormat.DXT1;
        	diffuseTextureImporter.maxTextureSize  = textureSize;
		}
		if (assetPath.Contains("_COLORA") || assetPath.Contains("_DIFFA"))  
		{
        	var diffuseAlphaTextureImporter : TextureImporter = assetImporter;
        	diffuseAlphaTextureImporter.isReadable = true;
        	diffuseAlphaTextureImporter.textureType = TextureImporterType.Image;
        	diffuseAlphaTextureImporter.filterMode = FilterMode.Trilinear;
        	diffuseAlphaTextureImporter.anisoLevel = 9;
        	diffuseAlphaTextureImporter.textureFormat = TextureImporterFormat.DXT5;
        	diffuseAlphaTextureImporter.maxTextureSize  = textureSize;
		}
		if (assetPath.Contains("_SPEC"))  
		{
        	var specularTextureImporter : TextureImporter = assetImporter;
        	specularTextureImporter.isReadable = true;
        	specularTextureImporter.textureType = TextureImporterType.Image;
        	specularTextureImporter.filterMode = FilterMode.Trilinear;
        	specularTextureImporter.anisoLevel = 9;
        	specularTextureImporter.textureFormat = TextureImporterFormat.DXT1;
        	specularTextureImporter.maxTextureSize  = textureSize;
		}
		if (assetPath.Contains("_RGB")) 
		{
        	var RGBTextureImporter : TextureImporter = assetImporter;
        	RGBTextureImporter.isReadable = true;
        	RGBTextureImporter.textureType = TextureImporterType.Advanced;
        	RGBTextureImporter.filterMode = FilterMode.Trilinear;
        	RGBTextureImporter.anisoLevel = 9;
        	RGBTextureImporter.linearTexture = true;
        	RGBTextureImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
        	RGBTextureImporter.maxTextureSize  = textureSize;
		}
		if (assetPath.Contains("_LUT")) 
		{
        	var LUTTextureImporter : TextureImporter = assetImporter;
        	LUTTextureImporter.isReadable = true;
        	LUTTextureImporter.textureType = TextureImporterType.Advanced;
        	LUTTextureImporter.filterMode = FilterMode.Trilinear;
        	LUTTextureImporter.wrapMode = TextureWrapMode.Clamp;
        	LUTTextureImporter.anisoLevel = 9;
        	LUTTextureImporter.linearTexture = true;
        	LUTTextureImporter.mipmapEnabled = false;
        	LUTTextureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        	LUTTextureImporter.maxTextureSize  = 512;
		}
		if (assetPath.Contains("_JITTER")) 
		{
        	var JITTERTextureImporter : TextureImporter = assetImporter;
        	JITTERTextureImporter.isReadable = true;
        	JITTERTextureImporter.textureType = TextureImporterType.Advanced;
        	JITTERTextureImporter.filterMode = FilterMode.Trilinear;
        	JITTERTextureImporter.wrapMode = TextureWrapMode.Repeat;
        	JITTERTextureImporter.anisoLevel = 9;
        	JITTERTextureImporter.linearTexture = true;
        	JITTERTextureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        	JITTERTextureImporter.maxTextureSize  = 512;
		}
		if (assetPath.Contains("_NORM")) 
		{
        	var normalTextureImporter : TextureImporter = assetImporter;
        	normalTextureImporter.isReadable = true;
        	normalTextureImporter.textureType = TextureImporterType.Bump;
        	normalTextureImporter.filterMode = FilterMode.Trilinear;
        	normalTextureImporter.anisoLevel = 9;   
        	normalTextureImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
        	normalTextureImporter.maxTextureSize  = textureSize;	
		}
	}
}
#endif