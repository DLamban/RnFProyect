using Core.GeometricEngine;
using Core.Rules;
using Godot;
using System;
using System.Collections.Generic;



public partial class GrassTerrain : Node3D
{
	//*******************************************************************************
	//* TODO: if performances drops, we can use a viewport as FRAMEBUFFER (Keyword) *
	//*******************************************************************************
	CompressedTexture2D dataTexture;
	Image baseImage;
	Vector2I textureSize;
	MatrixAffine convertMatrix;
	private float battlefieldWidthdm = Constants.battleFieldSizeWidthdm;
	private float battlefieldHeightdm = Constants.battleFieldSizeHeightdm;
	private ShaderMaterial ShaderMaterial;
	public override void _Ready()
	{
		UnitMovementManager.OnMoveUnit += UnitMoved;



        textureSize = new Vector2I(512,512);
        convertMatrix = createAffineTransform();
        
        MultiMeshInstance3D multiMeshInstance = GetNode<MultiMeshInstance3D>("quarter11/Multimesh");
		ShaderMaterial = multiMeshInstance.Multimesh.Mesh.SurfaceGetMaterial(0) as ShaderMaterial;

        baseImage = new Image();
        baseImage = Image.CreateEmpty(textureSize.X, textureSize.Y, false, Image.Format.Rgba8);
		baseImage.Fill(new Color(1f, 1f, 1f));


        Vector2I position1 = convertCoord(-7.62f,-5.585f);
        Rect2I rect1 = new Rect2I(position1,new Vector2I(20,20));

        Vector2I position2 = convertCoord(7f, -5.5f);
        Rect2I rect2 = new Rect2I(position2, new Vector2I(20, 20));
        
        Vector2I position3 = convertCoord(7f, 5f);
        Rect2I rect3 = new Rect2I(position3, new Vector2I(20, 20));

        updateDataTexture(rect1);
        updateDataTexture(rect2);
        updateDataTexture(rect3);
        updateTexture();
	
	   
		

	
	}
	private void UnitMoved(Unidad unit)
	{
	
		var i = 0;
	}
	private void updateTexture()
	{
        ImageTexture texture = new ImageTexture();
        texture = ImageTexture.CreateFromImage(baseImage);
        ShaderMaterial.SetShaderParameter("data_texture", texture);
    }

    // rect update
    private void updateDataTexture(Rect2I rect)
    {
        baseImage.FillRect(rect, new Color(0f, 0f, 0f));
    }
    private void updateDataTexture(int x, int y, int width, int height)
	{		
        baseImage.FillRect(new Rect2I(x,y,width,height), new Color(0f, 0f, 0f));
    }
    private void updateDataTexture(List<System.Drawing.Point> polygonPoints)
    {

    }
    private Vector2I convertCoord(float x, float y)
    {
        // simplified affine conversion
        int xconverted = (int)Math.Round(x * convertMatrix.m11 +  convertMatrix.offsetX);
        int yconverted = (int)Math.Round(y * convertMatrix.m22 +  convertMatrix.offsetY);
        return new Vector2I(xconverted, yconverted);
    }
    private MatrixAffine createAffineTransform()
    {
        float offsetX = (textureSize.X / 2);
        float offsetY = (textureSize.Y / 2);

        float scaleX = textureSize.X / battlefieldWidthdm;
        float scaleY = textureSize.Y / battlefieldHeightdm;
        return new MatrixAffine(scaleX, 0, 0, scaleY, offsetX, offsetY);
    }
}
