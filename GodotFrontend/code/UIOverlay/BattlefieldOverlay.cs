using Core.DB.Models;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GodotFrontend.code.UIOverlay
{
    internal class BattlefieldOverlay
    {
        //singleton
        private static BattlefieldOverlay instance;

        public static BattlefieldOverlay Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BattlefieldOverlay();
                }
                return instance;
            }
        }
        private Node3D battlefield;
        public Node3D shootingRangeOverlay { get; set; }
        private float zPosOverlay = 0.025f;
        
        // init the battlefield overlay
        public void vinculateBattlefield(Node3D _battlefield)
        {
            battlefield = _battlefield;
            createOverlaySprite();
        }
        private void createOverlaySprite()
        {
            shootingRangeOverlay = new Node3D();
            MeshInstance3D shootingRangeOverlayMesh = new MeshInstance3D();
            var mesh = new PlaneMesh();
            mesh.Size = new Vector2(20, 20);
            shootingRangeOverlayMesh.Mesh = mesh;
            //load shader from file
            
            ShaderMaterial overlayShader = new ShaderMaterial();           
            overlayShader.Shader = GD.Load<Shader>("res://shaders/shoot_range.gdshader");
            shootingRangeOverlayMesh.MaterialOverride = overlayShader;

            shootingRangeOverlayMesh.Position = new Vector3(0, mesh.Size.Y/2, zPosOverlay); // offset by half plane size
            shootingRangeOverlayMesh.RotationDegrees = new Vector3(90, 0, 0); // Girar para que quede plano
            shootingRangeOverlay.AddChild(shootingRangeOverlayMesh);
            battlefield.CallDeferred("add_child", shootingRangeOverlay);
            shootingRangeOverlay.Visible = false;

        }
        public void drawShootLine(UnitGodot selectedUnit)
        {
            shootingRangeOverlay.Visible = true;
           
            shootingRangeOverlay.Rotation = selectedUnit.Rotation;
            shootingRangeOverlay.Position = new Vector3(selectedUnit.coreUnit.centerTroop.X, selectedUnit.Position.Y + selectedUnit.offsetTroop.Y/2f + selectedUnit.offsetTroop.Y / 4f, zPosOverlay);


        }
    }
}
