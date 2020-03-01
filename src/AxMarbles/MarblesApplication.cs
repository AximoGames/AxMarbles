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

            var camSize = new Vector2(9 * ctx.ScreenAspectRatio, 9);

            ctx.Camera = new OrthographicCamera(new Vector3(4.5f + (camSize.X - camSize.Y) / 2f - 0.5f, -4.5f + 0.5f, 25))
            {
                Size = camSize,
                NearPlane = 1f,
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
                Size = 3,
                Center = false,
                ModelMatrix = Matrix4.CreateTranslation(0f, 0f, 0.01f),
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
                        PositionMatrix = Matrix4.CreateScale(1, -1, 1),
                        Material = new Material()
                        {
                            DiffuseImagePath = "Ressources/woodenbox.png",
                            SpecularImagePath = "Ressources/woodenbox_specular.png",
                            Color = new Vector3(0.0f, 0.5f, 0.0f),
                            Ambient = 1f,
                            Shininess = 32.0f,
                            SpecularStrength = 0.5f,
                            ColorBlendMode = MaterialColorBlendMode.Add,
                        },

                    };
                    ctx.AddObject(marble.RenderObject);
                }
            }
        }

        private Vector3 GetMarblePos(Vector2i marblePos)
        {
            return new Vector3(marblePos.X, marblePos.Y, 0.5f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (CurrentMouseWorldPositionIsValid)
            {
                var cursor = ctx.GetObjectByName<IPosition>("GroundCursor");
                cursor.Position = new Vector3(CurrentMouseWorldPosition.X, CurrentMouseWorldPosition.Y, cursor.Position.Z);
            }
        }

    }

}