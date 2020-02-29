using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using AxEngine;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;

namespace AxEngine
{
    public class MarblesApplication : RenderApplication
    {
        public MarblesApplication(RenderApplicationStartup startup) : base(startup)
        {
        }

        public override IRenderPipeline PrimaryRenderPipeline => ctx.GetPipeline<ForwardRenderPipeline>();

        protected override void SetupScene()
        {

            ctx.Camera = new OrthographicCamera(new Vector3(0, 0, 25))
            {
                NearPlane = 0.01f,
                FarPlane = 100.0f,
                Facing = (float)Math.PI / 2,
                Pitch = -((float)(Math.PI / 2) - 0.001f),
            };

            var material = new Material()
            {
                DiffuseImagePath = "Ressources/woodenbox.png",
                SpecularImagePath = "Ressources/woodenbox_specular.png",
                Color = new Vector3(1.0f, 1.0f, 0.0f),
                Ambient = 1f,
                Shininess = 32.0f,
                SpecularStrength = 0.5f,
            };

            ctx.AddObject(new CubeObject()
            {
                Name = "Ground",
                Material = material,
                Scale = new Vector3(50, 50, 1),
                Position = new Vector3(0f, 0f, -0.5f),
                // RenderShadow = false,
                PrimaryRenderPipeline = ctx.GetPipeline<ForwardRenderPipeline>(),
            });
            ctx.AddObject(new GridObject()
            {
                Name = "Grid",
                ModelMatrix = Matrix4.CreateTranslation(0f, 0f, 0.01f),
                //Debug = true,
            });
            ctx.AddObject(new GridObject()
            {
                Name = "Grid",
                ModelMatrix = Matrix4.CreateRotationY((float)Math.PI / 2) * Matrix4.CreateTranslation(-10f, 0f, 0.01f),
                //Debug = true,
            });
            ctx.AddObject(new GridObject()
            {
                Name = "Grid",
                ModelMatrix = Matrix4.CreateRotationX((float)Math.PI / 2) * Matrix4.CreateTranslation(0f, 10f, 0.01f),
                //Debug = true,
            });
            ctx.AddObject(new CrossLinesObject()
            {
                Name = "CenterCross",
                ModelMatrix = Matrix4.CreateScale(2.0f) * Matrix4.CreateTranslation(0f, 0f, 0.02f),
                //Debug = true,
            });

            ctx.AddObject(new TestObject()
            {
                Name = "GroundCursor",
                Material = material,
                Position = new Vector3(0, 1, 0.05f),
                Scale = new Vector3(1.0f, 1.0f, 0.1f),
                // Enabled = false,
            });

            ctx.AddObject(new CubeObject()
            {
                Name = "Box1",
                Material = material,
                Scale = new Vector3(1),
                Position = new Vector3(0, 0, 0.5f),
                // Enabled = false,
            });
        }

        public MarbleBoard Board;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (Board == null)
            {

                Board = new MarbleBoard();
                Board.NewGame();
            }
            foreach (var marble in Board.Marbles)
            {
                if (marble.RenderObject == null)
                {
                    marble.RenderObject = new CubeObject()
                    {
                        Position = GetMarblePos(marble.Position),
                    };
                    ctx.AddObject(marble.RenderObject);
                }
            }
        }

        private Vector3 GetMarblePos(Vector2i marblePos)
        {
            return new Vector3(marblePos.X, -marblePos.Y, 1f);
        }

        public override void OnRenderFrame(FrameEventArgs e)
        {
            if (CurrentMouseWorldPositionIsValid)
            {
                var cursor = ctx.GetObjectByName<IPosition>("GroundCursor");
                cursor.Position = new Vector3(CurrentMouseWorldPosition.X, CurrentMouseWorldPosition.Y, cursor.Position.Z);
            }
            base.OnRenderFrame(e);
        }

    }

}