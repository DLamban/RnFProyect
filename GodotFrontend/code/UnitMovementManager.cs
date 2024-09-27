using Godot;
using System;
using Core.GeometricEngine;

// class that we use to move units and keep tracking of movement
// geometry heavy, maybe we will split this in user interactions and the pure geomtery that give the constrains

public class UnitMovementManager
{
	// curryfying/overloadind
	public static void ApplyAffineTransformation(Unidad unidad)
	{
		ApplyAffineTransformation(unidad.affTrans, unidad);
	}
	public static void ApplyAffineTransformation(AffineTransformCore affTrans, Node3D node3d)
	{
		// Definir la matriz afín 2D
		Transform2D affine2D = new Transform2D();
		affine2D[0] = new Vector2((float)affTrans.m11, (float)affTrans.m21);
		affine2D[1] = new Vector2((float)affTrans.m12, (float)affTrans.m22);
		affine2D[2] = new Vector2((float)affTrans.offsetX, (float)affTrans.offsetY);


		// Convertir la matriz afín 2D a una matriz 3D
		Transform3D affine3D = ConvertAffine2DTo3D(affine2D);

		// Obtener el nodo MeshInstance3D y aplicar la transformación
		node3d.Transform = affine3D;
    }

	private static Transform3D ConvertAffine2DTo3D(Transform2D affine2D)
	{
		// Crear una matriz de transformación 3D
		Transform3D affine3D = new Transform3D();
		// COMO SOLO ROTAMOS SOBRE EJE Z, PODEMOS CONVERTIR LA MATRIZ 2D A 3D DE LA SIGUIENTE MANERA
		affine3D[0] = new Vector3(affine2D[0].X, affine2D[0].Y, 0); // X
		affine3D[1] = new Vector3(affine2D[1].X, affine2D[1].Y, 0); // Y
		affine3D[2] = new Vector3(0, 0, 1); // Z
		affine3D[3] = new Vector3(affine2D[2].X, affine2D[2].Y, 0); // OFFSET
		
		return affine3D;
	}
	
}
