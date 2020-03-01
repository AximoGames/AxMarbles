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
            WorldPositionMatrix = Matrix4.CreateScale(1, -1, 1);
            DefaultKeyBindings = false;

            var camSize = new Vector2(9 * ctx.ScreenAspectRatio, 9);

            ctx.Camera = new OrthographicCamera(new Vector3(4.5f + (camSize.X - camSize.Y) / 2f - 0.5f, -4.5f + 0.5f, 25))
            {
                Size = camSize,
                NearPlane = 1f,
                FarPlane = 100.0f,
                Facing = (float)Math.PI / 2,
                Pitch = -((float)(Math.PI / 2) - 0.00001f),
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
                Size = 9,
                Center = false,
                ModelMatrix = Matrix4.CreateScale(1, -1, 1) * Matrix4.CreateTranslation(-0.5f, 0.5f, 0.01f),
                //Debug = true,
            });
            ctx.AddObject(new CrossLinesObject()
            {
                Name = "CenterCross",
                ModelMatrix = Matrix4.CreateScale(2.0f) * Matrix4.CreateTranslation(0f, 0f, 0.02f),
                //Debug = true,
            });

            ctx.AddObject(new CubeObject()
            {
                Name = "GroundCursor",
                Material = material,
                Position = new Vector3(0, 1, 0.05f),
                Scale = new Vector3(1.0f, 1.0f, 0.1f),
                // Enabled = false,
            });

            ctx.AddObject(new CubeObject()
            {
                Name = "MarbleSelector",
                Material = material,
                Position = new Vector3(0, 1, 0.05f),
                Scale = new Vector3(1.3f, 1.3f, 0.1f),
                Enabled = false,
            });

            ctx.AddObject(new CubeObject()
            {
                Name = "Box1",
                Material = material,
                Scale = new Vector3(1),
                Position = new Vector3(0, 0, 0.5f),
                // Enabled = false,
            });

            ctx.AddAnimation(ScaleAnim = new Animation()
            {
                Duration = TimeSpan.FromSeconds(0.75),
            });
            ScaleAnim.AnimationFinished += OnMarbleScaled;
        }

        public MarbleBoard Board;

        private Animation ScaleAnim;

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
                        PositionMatrix = Matrix4.CreateScale(1, -1, 1),
                        Material = GetMaterial(marble),

                    };
                    ctx.AddObject(marble.RenderObject);
                }
                var ro = marble.RenderObject;
                if (marble.State == MarbleState.Removing)
                {
                    ro.Scale = new Vector3(ScaleAnim.Value);
                }
                ro.Position = GetMarblePos(marble.Position);
            }
        }

        private void OnMarbleScaled()
        {
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

        private Marble SelectedMarble;

        private Material GetMaterial(Marble marble)
        {
            return new Material()
            {
                DiffuseImagePath = "Ressources/woodenbox.png",
                SpecularImagePath = "Ressources/woodenbox_specular.png",
                Color = GetMaterialColor(marble),
                Ambient = 1f,
                Shininess = 32.0f,
                SpecularStrength = 0.5f,
                ColorBlendMode = MaterialColorBlendMode.Add,
            };
        }

        private Vector3 GetMaterialColor(Marble marble)
        {
            switch (marble.Color)
            {
                case MarbleColor.Red:
                    return new Vector3(1, 0, 0);
                case MarbleColor.Green:
                    return new Vector3(0, 1, 0);
                case MarbleColor.Blue:
                    return new Vector3(0, 0, 1);
                case MarbleColor.Yellow:
                    return new Vector3(1, 1, 0);
                case MarbleColor.Orange:
                    return new Vector3(1, 0.65f, 0);
                case MarbleColor.White:
                    return new Vector3(1, 1, 1);
                case MarbleColor.Black:
                    return new Vector3(0, 0, 0);
                default:
                    return new Vector3(0, 0, 0);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (CurrentMouseWorldPositionIsValid)
            {
                var pos = CurrentMouseWorldPosition.Round().Xy.ToVector3i();
                Console.WriteLine($"Clicked: {pos}");
                ScaleAnim.Start();
                var selector = ctx.GetObjectByName<CubeObject>("MarbleSelector");

                var marble = Board[pos];
                if (marble != null)
                {
                    SelectedMarble = marble;
                    selector.Position = new Vector3(pos.X, pos.Y, 0);
                    selector.Enabled = true;
                }
                else
                {
                    if (SelectedMarble != null)
                    {
                        Board.MoveMarble(SelectedMarble, pos);
                        SelectedMarble = null;
                        selector.Enabled = false;
                    }
                }
            }
        }

    }

}