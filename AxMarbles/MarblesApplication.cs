﻿// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using Aximo.Engine;
using Aximo.Render;
using OpenTK;
using OpenTK.Input;

namespace Aximo.Marbles
{
    public class MarblesApplication : RenderApplication
    {
        public MarblesApplication(RenderApplicationStartup startup) : base(startup)
        {
        }

        protected override void SetupScene()
        {
            Board = new MarbleBoard
            {
                OnMatch = OnMatch,
                OnNewMarbles = OnNewMarbles,
            };

            RenderContext.WorldPositionMatrix = Matrix4.CreateScale(1, -1, 1);
            RenderContext.PrimaryRenderPipeline = RenderContext.GetPipeline<ForwardRenderPipeline>();
            DefaultKeyBindings = false;

            var camSize = new Vector2(9 * RenderContext.ScreenAspectRatio, 9);

            RenderContext.Camera = new OrthographicCamera(new Vector3(4.5f + ((camSize.X - camSize.Y) / 2f) - 0.5f, -4.5f + 0.5f, 25))
            {
                Size = camSize,
                NearPlane = 1f,
                FarPlane = 100.0f,
                Facing = (float)Math.PI / 2,
                Pitch = -((float)(Math.PI / 2) - 0.00001f),
            };

            var material = new Material()
            {
                DiffuseImagePath = "Textures/woodenbox.png",
                SpecularImagePath = "Textures/woodenbox_specular.png",
                Color = new Vector3(1.0f, 1.0f, 0.0f),
                Ambient = 0.5f,
                Shininess = 32.0f,
                SpecularStrength = 0.5f,
            };

            MarbleShader = new Shader("Shaders/shader.vert", "Shaders/lighting.frag", null, false);
            MarbleShader.SetDefine("OVERRIDE_GET_MATERIAL_DIFFUSE_FILE", "marble.material.glsl");
            MarbleShader.SetDefine("FRAG_HEADER_FILE", "marble.params.glsl");
            MarbleShader.Compile();

            RenderContext.AddObject(new CubeObject()
            {
                Name = "Ground",
                Material = new Material
                {
                    Color = new Vector3(0.4f, 0.6f, 0.6f),
                    ColorBlendMode = MaterialColorBlendMode.Set,
                },
                Scale = new Vector3(50, 50, 1),
                Position = new Vector3(0f, 0f, -0.5f),
                // RenderShadow = false,
                PrimaryRenderPipeline = RenderContext.GetPipeline<ForwardRenderPipeline>(),
            });

            // ctx.AddObject(new CubeObject()
            // {
            //     Name = "Board",
            //     Material = new Material
            //     {
            //         Color = new Vector3(0.4f, 0.6f, 0.6f) * 1.1f,
            //         ColorBlendMode = MaterialColorBlendMode.Set,
            //     },
            //     Scale = new Vector3(Board.Width, Board.Height, 1),
            //     Position = new Vector3(Board.Width / 2f - 0.5f, Board.Height / 2f - 0.5f, -0.45f),
            //     // RenderShadow = false,
            //     PrimaryRenderPipeline = ctx.GetPipeline<ForwardRenderPipeline>(),
            // });

            RenderContext.AddObject(new GridObject()
            {
                Name = "Grid",
                Size = 9,
                Center = false,
                ModelMatrix = Matrix4.CreateScale(1, -1, 1) * Matrix4.CreateTranslation(-0.5f, 0.5f, 0.01f),
                //Debug = true,
            });
            RenderContext.AddObject(new CrossLinesObject()
            {
                Name = "CenterCross",
                ModelMatrix = Matrix4.CreateScale(2.0f) * Matrix4.CreateTranslation(0f, 0f, 0.02f),
                //Debug = true,
            });

            RenderContext.AddObject(new CubeObject()
            {
                Name = "GroundCursor",
                Material = material,
                Position = new Vector3(0, 1, 0.05f),
                Scale = new Vector3(1.0f, 1.0f, 0.1f),
                // Enabled = false,
            });

            RenderContext.AddObject(new CubeObject()
            {
                Name = "MarbleSelector",
                Material = material,
                Position = new Vector3(0, 1, 0.05f),
                Scale = new Vector3(1.3f, 1.3f, 0.1f),
                Enabled = false,
            });

            RenderContext.AddObject(new LightObject()
            {
                Position = new Vector3(0, 2, 3.5f),
                Name = "MovingLight",
                LightType = LightType.Point,
                ShadowTextureIndex = 0,
                //Enabled = false,
            });

            RenderContext.AddObject(new LightObject()
            {
                Position = new Vector3(2f, 0.5f, 4.25f),
                Name = "StaticLight",
                LightType = LightType.Point,
                ShadowTextureIndex = 1,
            });

            // ctx.AddObject(new StatsObject()
            // {
            //     Name = "Stats",
            //     RectanglePixels = new RectangleF(40, 40, 100f, 100f),
            // });

            RenderContext.AddObject(new UIObject()
            {
                Name = "UI",
                RectanglePixels = new RectangleF(0, 0, RenderContext.ScreenSize.X, RenderContext.ScreenSize.Y),
            });

            GameContext.AddAnimation(RemoveAnim = new Animation()
            {
                Duration = TimeSpan.FromSeconds(0.75),
                AnimationFunc = AnimationFuncs.LinearReverse(MarbleScale),
            });
            RemoveAnim.AnimationFinished += OnAnimFinshed_MarbleScaled;

            GameContext.AddAnimation(CreateAnim = new Animation()
            {
                Duration = TimeSpan.FromSeconds(0.75),
                AnimationFunc = AnimationFuncs.Linear(MarbleScale),
            });
            CreateAnim.AnimationFinished += OnAnimationFinished_MarbleCreated;

            GameContext.AddAnimation(MoveAnim = new Animation()
            {
            });
            MoveAnim.AnimationFinished += OnAnimFinished_MarbleMoved;
        }

        public MarbleBoard Board;

        private Animation RemoveAnim;
        private Animation CreateAnim;
        private Animation MoveAnim;

        private const float MarbleScale = MathF.PI / 2f / 2f;
        private const float MarbleZ = MarbleScale / 2f;

        public Shader MarbleShader;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Board.Marbles.Count == 0)
            {
                Board.NewGame();
            }

            var kbState = Keyboard.GetState();
            if (kbState[Key.AltRight] && kbState[Key.K])
                DefaultKeyBindings = !DefaultKeyBindings;

            foreach (var marble in Board.Marbles)
            {
                if (marble.RenderObject == null)
                {
                    if (marble.Color == MarbleColor.BombJoker)
                    {
                        marble.RenderObject = new CubeObject()
                        {
                            PositionMatrix = Matrix4.CreateScale(1, -1, 1),
                            Scale = new Vector3(MarbleScale),
                            Shader = MarbleShader,
                        };
                        RenderContext.AddObject(marble.RenderObject);
                        marble.RenderObject.AddShaderParam("joker", marble.Color == MarbleColor.ColorJoker ? 1 : 0);
                        marble.RenderObject.AddShaderParam("color2", GetMaterialColorShader(marble.Color2));
                    }
                    else
                    {
                        marble.RenderObject = new SphereObject()
                        {
                            PositionMatrix = Matrix4.CreateScale(1, -1, 1),
                            Material = GetMaterial(marble),
                            Scale = new Vector3(MarbleScale),
                            Shader = MarbleShader,
                        };
                        RenderContext.AddObject(marble.RenderObject);
                        marble.RenderObject.AddShaderParam("joker", marble.Color == MarbleColor.ColorJoker ? 1 : 0);
                        marble.RenderObject.AddShaderParam("color2", GetMaterialColorShader(marble.Color2));
                    }
                }
                var ro = marble.RenderObject;
                if (marble.State == MarbleState.Adding)
                {
                    ro.Scale = new Vector3(CreateAnim.Value);
                }
                if (marble.State == MarbleState.Removing || marble.State == MarbleState.Exploding)
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
                    ro.Rotate = new Vector3();
                }
            }
        }

        private (Vector3 Position, Vector3 Rotate) GetPathPosition(Marble marble)
        {
            var steps = CurrentPath.Count - 1;
            var scaledPos = MoveAnim.Position * steps;
            var step = (int)MathF.Floor(scaledPos);

            // Prevent rare exception
            step = Math.Min(step, CurrentPath.Count - 2);

            float subPos = scaledPos - step;
            var fromPos = CurrentPath[step];
            var toPos = CurrentPath[step + 1];
            var direction = toPos - fromPos;
            var subDirection = new Vector3(direction.X, direction.Y, 0.5f) * subPos;
            var resultPos = GetMarblePos(fromPos) + subDirection;
            var resultRotate = new Vector3(direction.Y, direction.X, 0) * (subPos * 0.5f);
            return (resultPos, resultRotate);
        }

        private void OnMatch()
        {
            RemoveAnim.Start();
            if (!Board.MatchHasBomb)
                AudioManager.Default.PlayAsync("Sounds/marble-removing.wav");
            else
                AudioManager.Default.PlayAsync("Sounds/marble-explode.wav");
        }

        private void OnNewMarbles()
        {
            CreateAnim.Start();
            AudioManager.Default.PlayAsync("Sounds/marble-adding.wav");
        }

        private void OnAnimFinshed_MarbleScaled()
        {
            Board.ScoreMatches();
        }

        private void OnAnimationFinished_MarbleCreated()
        {
            Board.CreatedAnimationFinished();
        }

        private void OnAnimFinished_MarbleMoved()
        {
            var target = CurrentPath[CurrentPath.Count - 1];
            CurrentPath = null;
            Board.MoveMarble(SelectedMarble, target);
            if (!Board.CheckMatch(target))
            {
                Board.CreateRandomMarbles();
            }
            SelectedMarble = null;
        }

        private Vector3 GetMarblePos(Vector2i marblePos)
        {
            return new Vector3(marblePos.X, marblePos.Y, MarbleZ);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (CurrentMouseWorldPositionIsValid)
            {
                var cursor = RenderContext.GetObjectByName<IPosition>("GroundCursor");
                cursor.Position = new Vector3(CurrentMouseWorldPosition.X, CurrentMouseWorldPosition.Y, cursor.Position.Z);
            }
        }

        private Marble SelectedMarble;

        private Material GetMaterial(Marble marble)
        {
            return new Material()
            {
                DiffuseImagePath = "Textures/woodenbox.png",
                SpecularImagePath = "Textures/woodenbox_specular.png",
                Color = GetMaterialColorShader(marble.Color1),
                Ambient = 0.5f,
                Shininess = 32.0f,
                SpecularStrength = 0.5f,
                ColorBlendMode = MaterialColorBlendMode.Set,
            };
        }

        private Vector3 GetMaterialColorShader(MarbleColor marbleColor)
        {
            var color = GetMaterialColor(marbleColor);
            var addColor = new Vector3(0);
            if (color == Vector3.Zero)
                addColor = new Vector3(0.3f);
            color += addColor;
            return color * 0.5f;
        }

        private Vector3 GetMaterialColor(MarbleColor marbleColor)
        {
            switch (marbleColor)
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
                var pos = CurrentMouseWorldPosition.Round().Xy.ToVector2i();

                if (!Board.PositionInMap(pos))
                    return;

                //ScaleAnim.Start();
                var selector = RenderContext.GetObjectByName<CubeObject>("MarbleSelector");

                if (MoveAnim.Enabled || RemoveAnim.Enabled || CreateAnim.Enabled)
                    return;

                if (Board[pos]?.Color == MarbleColor.BombJoker)
                    return;

                var marble = Board[pos];
                Console.WriteLine($"Clicked: {pos}. Marble: {marble}");
                if (marble != null)
                {
                    SelectedMarble = marble;
                    selector.Position = new Vector3(pos.X, pos.Y, 0);
                    selector.Enabled = true;
                    AudioManager.Default.PlayAsync("Sounds/marble-select.wav");
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
                            //var moveStepDuration = TimeSpan.FromSeconds(2);
                            MoveAnim.Duration = moveStepDuration.Multiply(path.Count);
                            MoveAnim.Start();
                            AudioManager.Default.PlayAsync("Sounds/marble-moving.wav");
                            selector.Enabled = false;
                        }
                    }
                }
            }
        }

    }

}
