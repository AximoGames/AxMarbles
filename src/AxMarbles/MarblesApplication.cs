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
                Ambient = 0.5f,
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

            ctx.AddObject(new LightObject()
            {
                Position = new Vector3(0, 2, 2.5f),
                Name = "MovingLight",
                LightType = LightType.Directional,
                ShadowTextureIndex = 0,
                //Enabled = false,
            });

            ctx.AddObject(new LightObject()
            {
                Position = new Vector3(2f, 0.5f, 3.25f),
                Name = "StaticLight",
                LightType = LightType.Directional,
                ShadowTextureIndex = 1,
            });

            ctx.AddObject(new StatsObject()
            {
                Name = "Stats",
                RectanglePixels = new RectangleF(40, 40, 100f, 100f),
            });

            ctx.AddAnimation(RemoveAnim = new Animation()
            {
                Duration = TimeSpan.FromSeconds(0.75),
                AnimationFunc = AnimationFuncs.LinearReverse(MarbleScale),
            });
            RemoveAnim.AnimationFinished += OnAnimFinshed_MarbleScaled;

            ctx.AddAnimation(CreateAnim = new Animation()
            {
                Duration = TimeSpan.FromSeconds(0.75),
                AnimationFunc = AnimationFuncs.Linear(MarbleScale),
            });
            CreateAnim.AnimationFinished += OnAnimationFinished_MarbleCreated;

            ctx.AddAnimation(MoveAnim = new Animation()
            {
            });
            MoveAnim.AnimationFinished += OnAnimFinished_MarbleMoved;
        }

        public MarbleBoard Board;

        private Animation RemoveAnim;
        private Animation CreateAnim;
        private Animation MoveAnim;

        private float MarbleScale = 0.8f;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Board == null)
            {
                Board = new MarbleBoard();
                Board.OnMatch = OnMatch;
                Board.OnNewMarbles = OnNewMarbles;
                Board.NewGame();
            }

            foreach (var marble in Board.Marbles)
            {
                if (marble.RenderObject == null)
                {
                    marble.RenderObject = new SphereObject()
                    {
                        PositionMatrix = Matrix4.CreateScale(1, -1, 1),
                        Material = GetMaterial(marble),
                        Scale = new Vector3(MarbleScale),
                    };
                    ctx.AddObject(marble.RenderObject);
                }
                var ro = marble.RenderObject;
                if (marble.State == MarbleState.Adding)
                {
                    ro.Scale = new Vector3(CreateAnim.Value);
                }
                if (marble.State == MarbleState.Removing)
                {
                    ro.Scale = new Vector3(RemoveAnim.Value);
                }
                if (marble == SelectedMarble && CurrentPath != null && MoveAnim.Enabled)
                {
                    var result = GetPathPosition(marble);
                    ro.Position = result.Position;
                    ro.Rotate = result.Rotate;
                }
                else
                {
                    ro.Position = GetMarblePos(marble.Position);
                    ro.Rotate = new Vector3(); ;
                }
            }
        }

        private (Vector3 Position, Vector3 Rotate) GetPathPosition(Marble marble)
        {
            var steps = CurrentPath.Count - 1;
            var scaledPos = MoveAnim.Position * steps;
            var step = (int)MathF.Floor(scaledPos);
            float subPos = scaledPos - step;
            var fromPos = CurrentPath[step];
            var toPos = CurrentPath[step + 1];
            var direction = toPos - fromPos;
            var subDirection = new Vector3(direction.X, direction.Y, 0.5f) * subPos;
            var resultPos = GetMarblePos(fromPos) + subDirection;
            var resultScale = new Vector3(direction.Y, direction.X, 0) * subPos;
            return (resultPos, resultScale);
        }

        private void OnMatch()
        {
            RemoveAnim.Start();
        }

        private void OnNewMarbles()
        {
            CreateAnim.Start();
        }

        private void OnAnimFinshed_MarbleScaled()
        {
            Board.ScoreMatches();
        }

        private void OnAnimationFinished_MarbleCreated()
        {
            foreach (var marble in Board.Marbles)
                if (marble.State == MarbleState.Adding)
                    marble.State = MarbleState.Default;
        }

        private void OnAnimFinished_MarbleMoved()
        {
            var target = CurrentPath[CurrentPath.Count - 1];
            CurrentPath = null;
            Board.MoveMarble(SelectedMarble, target);
            Board.CheckMatch(target);
            SelectedMarble = null;
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
                Color = GetMaterialColor(marble) + new Vector3(0.1f),
                Ambient = 0.5f,
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

        private Vector2iList CurrentPath;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (CurrentMouseWorldPositionIsValid)
            {
                var pos = CurrentMouseWorldPosition.Round().Xy.ToVector3i();
                //ScaleAnim.Start();
                var selector = ctx.GetObjectByName<CubeObject>("MarbleSelector");

                if (MoveAnim.Enabled || RemoveAnim.Enabled || CreateAnim.Enabled)
                    return;

                var marble = Board[pos];
                Console.WriteLine($"Clicked: {pos}. Marble: {marble}");
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
                        var path = Board.FindPath(SelectedMarble, pos);
                        if (path != null && path.Count > 0)
                        {
                            CurrentPath = path;
                            var moveStepDuration = TimeSpan.FromSeconds(0.1);
                            //var moveStepDuration = TimeSpan.FromSeconds(1);
                            MoveAnim.Duration = moveStepDuration * path.Count;
                            MoveAnim.Start();
                            selector.Enabled = false;
                        }
                    }
                }
            }
        }

    }

}