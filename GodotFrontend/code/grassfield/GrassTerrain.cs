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
    DataNode2d drawNodeTexture;
	public override void _Ready()
	{
		UnitMovementManager.OnMoveUnit += UnitMoved;


        var viewport = GetNode<SubViewport>("DataTextureViewport");
        ViewportTexture textureviewport = viewport.GetTexture();
        drawNodeTexture = viewport.GetNode<DataNode2d>("DataNode2D");

        textureSize = new Vector2I(512,512);
        convertMatrix = createAffineTransform();
        
        MultiMeshInstance3D multiMeshInstance = GetNode<MultiMeshInstance3D>("quarter11/Multimesh");
		ShaderMaterial = multiMeshInstance.Multimesh.Mesh.SurfaceGetMaterial(0) as ShaderMaterial;

        ShaderMaterial.SetShaderParameter("data_texture", textureviewport);
    }
	private void UnitMoved(Unidad unit)
	{
        Vector2 pos = new Vector2((float)unit.coreUnit.Transform.offsetX, (float)unit.coreUnit.Transform.offsetY);	
		Vector2 size = new Vector2((float)unit.coreUnit.Transform.offsetY, (float)unit.coreUnit.Transform.offsetY);
        Rect2I rect2I = new Rect2I(convertCoord(pos.X,pos.Y),20,-20);

        // create polygon
        List<System.Numerics.Vector2> points = unit.coreUnit.polygonPointsWorld;

        updateDataTexture(points);
	}

    // rect update
    private void updateDataTexture(Rect2I rect)
    {
        drawNodeTexture.drawRectBlack(rect);
        
    }
    // polygon update
    private void updateDataTexture(List<System.Numerics.Vector2> points)
    {
        List<Vector2> pointsGodotFormat = new List<Vector2>();
        foreach (System.Numerics.Vector2 point in points)
        {
            pointsGodotFormat.Add(convertCoord(point.X,point.Y));
        }
        drawNodeTexture.drawPolygon(pointsGodotFormat);
        
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
