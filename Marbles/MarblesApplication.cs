﻿// This file is part of Aximo Marbles, a Game written in C# with the Aximo Game Engine. Web: https://github.com/AximoGames
// Licensed under the GPL3 license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Aximo.Engine;
using Aximo.Marbles.PathFinding;
using Aximo.Render;
using OpenToolkit;
using OpenToolkit.Input;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Aximo.Marbles
{
    public class MarblesApplication : RenderApplication
    {
        public MarblesApplication(RenderApplicationConfig startup) : base(startup)
        {
        }

        protected override void SetupScene()
        {
            Board = new MarbleBoard
            {
                OnMatch = OnMatch,
                OnNewMarbles = OnNewMarbles,
            };

            //RenderContext.WorldPositionMatrix = Matrix4.CreateScale(1, -1, 1);
            //RenderContext.PrimaryRenderPipeline = RenderContext.GetPipeline<ForwardRenderPipeline>();
            DefaultKeyBindings = false;

            GameContext.AddActor(BoardActor = new Actor(BoardComponent = new SceneComponent()
            {
                //RelativeRotation = new Vector3(0, 0, 0.02f).ToQuaternion(),
                RelativeTranslation = new Vector3(0, 0, 0.05f),
            }));

            BoardComponent.AddComponent(NextMarbleComponent = new SceneComponent()
            {
                RelativeTranslation = new Vector3(10, -3, 0),
                RelativeScale = new Vector3(0.5f),
            });

            BoardComponent.AddComponent(NextMarbleBoardComponent = new SceneComponent()
            {
                RelativeTranslation = new Vector3(0, 0, -0.2f),
                // RelativeScale = new Vector3(0.5f),
                // TranslationMatrix = Matrix4.CreateScale(2),
                // IsAbsoluteRotation = true,
                // IsAbsoluteScale = true,
                // IsAbsoluteTranslation = true,
            });

            var camSize = new Vector2(9 * RenderContext.ScreenAspectRatio, 9);

            RenderContext.Camera = new OrthographicCamera(new Vector3(4.5f + ((camSize.X - camSize.Y) / 2f) - 0.5f, -4.5f + 0.5f, 25))
            {
                Size = camSize,
                NearPlane = 1f,
                FarPlane = 100.0f,
                Facing = (float)Math.PI / 2,
                Pitch = -((float)(Math.PI / 2) - 0.00001f),
            };

            GameContext.AddActor(new Actor(new CubeComponent()
            {
                Name = "Ground",
                Material = new GameMaterial
                {
                    Color = new Vector3(0.4f, 0.6f, 0.6f),
                    Shininess = 1.0f,
                    PipelineType = PipelineType.Forward,
                },
                RelativeScale = new Vector3(50, 50, 1),
                RelativeTranslation = new Vector3(0f, 0f, -0.5f),
                //RenderShadow = false,
            }));

            BoardComponent.AddComponent(new CubeComponent()
            {
                Name = "Board",
                Material = new GameMaterial
                {
                    Color = new Vector3(0.4f, 0.6f, 0.6f) * 1.1f,
                    Shininess = 1.0f,
                    PipelineType = PipelineType.Forward,
                },
                RelativeScale = new Vector3(Board.Width, Board.Height, 1),
                RelativeTranslation = new Vector3((Board.Width / 2f) - 0.5f, (Board.Height / 2f) - 0.5f, -0.5f),
                TranslationMatrix = BoardTranslationMatrix,
                //RenderShadow = false,
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

            BoardComponent.AddComponent(new GridPlaneComponent(9, false)
            {
                Name = "Grid",
                RelativeTranslation = new Vector3(-0.5f, -0.5f + 9, 0.01f),
                TranslationMatrix = BoardTranslationMatrix,
            });
            GameContext.AddActor(new Actor(new CrossLineComponent(10, true)
            {
                Name = "CenterCross",
                RelativeTranslation = new Vector3(0f, 0f, 0.02f),
                RelativeScale = new Vector3(1.0f),
            }));

            BoardComponent.AddComponent(new CubeComponent()
            {
                Name = "GroundCursor",
                Material = new GameMaterial()
                {
                    DiffuseTexture = GameTexture.GetFromFile("Textures/woodenbox.png"),
                    SpecularTexture = GameTexture.GetFromFile("Textures/woodenbox_specular.png"),
                    Ambient = 0.3f,
                    Shininess = 32.0f,
                    SpecularStrength = 0.5f,
                    CastShadow = true,
                },
                RelativeTranslation = new Vector3(0, 1, 0.05f),
                RelativeScale = new Vector3(1.0f, 1.0f, 0.1f),
                // Enabled = false,
            });

            BoardComponent.AddComponent(new CubeComponent()
            {
                Name = "MarbleSelector",
                Material = new GameMaterial()
                {
                    DiffuseTexture = GameTexture.GetFromFile("Textures/woodenbox.png"),
                    SpecularTexture = GameTexture.GetFromFile("Textures/woodenbox_specular.png"),
                    Ambient = 0.3f,
                    Shininess = 32.0f,
                    SpecularStrength = 0.5f,
                    CastShadow = true,
                },
                TranslationMatrix = BoardTranslationMatrix,
                //Material = material,
                RelativeTranslation = new Vector3(0, 1, 0.05f),
                RelativeScale = new Vector3(1.3f, 1.3f, 0.1f),
                Visible = false,
            });

            GameContext.AddActor(new Actor(new PointLightComponent()
            {
                RelativeTranslation = new Vector3(0, 2, 3.5f),
                Name = "MovingLight",
                //Enabled = false,
            }));

            GameContext.AddActor(new Actor(new PointLightComponent()
            {
                RelativeTranslation = new Vector3(2f, 0.5f, 4.25f),
                Name = "StaticLight",
            }));

            var flowContainer = new UIFlowContainer()
            {
                Name = "UI",
                DefaultChildSizes = new Vector2(0, 50),
                ExtraChildMargin = new UIAnchors(10, 10, 10, 0),
                Location = new Vector2(600, 0),
                Size = new Vector2(200, 0),
            };
            GameContext.AddActor(new Actor(flowContainer));

            // flowContainer.AddComponent(new UIMarbles()
            // {
            //     Name = "UI",
            // });

            flowContainer.AddComponent(new UILabelComponent()
            {
                Name = "LastScore",
                Color = Color.White,
            });
            flowContainer.AddComponent(new UILabelComponent()
            {
                Name = "TotalScore",
                Color = Color.White,
            });

            GameContext.AddActor(new Actor(new StatsComponent()
            {
                Name = "Stats",
            }));

            UIButton bt;
            flowContainer.AddComponent(bt = new UIButton()
            {
                Name = "Exit",
                Text = "Exit",
                Color = Color.White,
                BackColor = new Color(new Rgba32(0, 0, 0, 0.5f)),
                BackColorHover = new Color(new Rgba32(0, 0, 0, 0.8f)),
                BorderColor = new Color(new Rgba32(0, 0, 0, 0.8f)),
                // Location = new Vector2(620, 200),
                // Size = new Vector2(100, 100),
            });
            bt.Click += (e) =>
            {
                Stop();
            };

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

        public Actor BoardActor;
        public SceneComponent BoardComponent;
        public SceneComponent NextMarbleComponent;
        public SceneComponent NextMarbleBoardComponent;

        public Matrix4 BoardTranslationMatrix = Matrix4.CreateScale(1, -1, 1);

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            var kbState = KeyboardState;
            if (kbState[Key.AltRight] && kbState[Key.K])
                DefaultKeyBindings = !DefaultKeyBindings;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Board.Marbles.Count == 0)
            {
                Board.NewGame();
            }

            var kbState = KeyboardState;

            if (kbState[Key.Escape])
            {
                Stop();
                return;
            }

            foreach (var marble in Board.Marbles)
            {
                if (marble.RenderObject == null)
                {
                    if (marble.Color == MarbleColor.BombJoker)
                    {
                        marble.RenderObject = new CubeComponent()
                        {
                            Material = new GameMaterial()
                            {
                                DiffuseTexture = GameTexture.GetFromFile("Textures/woodenbox.png"),
                                SpecularTexture = GameTexture.GetFromFile("Textures/woodenbox_specular.png"),
                                Ambient = 0.3f,
                                Shininess = 32.0f,
                                SpecularStrength = 0.5f,
                                CastShadow = true,
                            },
                            TranslationMatrix = BoardTranslationMatrix,
                            //TranslationTransform = Transform.CreateScale(1, -1, 1),
                            RelativeScale = new Vector3(MarbleScale),
                        };
                        marble.RenderObject.Material.AddParameter("joker", marble.Color == MarbleColor.ColorJoker ? 1 : 0);
                        marble.RenderObject.Material.AddParameter("color2", GetMaterialColorShader(marble.Color2));
                    }
                    else
                    {
                        marble.RenderObject = new SphereComponent()
                        {
                            TranslationMatrix = BoardTranslationMatrix,
                            //TranslationTransform = Transform.CreateScale(1, -1, 1),
                            Material = GetMaterial(marble),
                            RelativeScale = new Vector3(MarbleScale),
                        };
                        marble.RenderObject.Material.AddParameter("joker", marble.Color == MarbleColor.ColorJoker ? 1 : 0);
                        marble.RenderObject.Material.AddParameter("color2", GetMaterialColorShader(marble.Color2));
                    }
                }

                SceneComponent parent = null;
                if (marble.OnBoard)
                {
                    parent = BoardComponent;
                }
                else
                {
                    if (Board.PreviewMode == MarblePreview.Board)
                        parent = NextMarbleBoardComponent;
                    else if (Board.PreviewMode == MarblePreview.Side)
                        parent = NextMarbleComponent;
                }

                if (parent != null && marble.RenderObject.Parent != parent)
                {
                    marble.RenderObject.Detach();
                    parent.AddComponent(marble.RenderObject);
                }

                var ro = marble.RenderObject;
                if (marble.State == MarbleState.Adding || marble.State == MarbleState.PreAdding)
                {
                    if (marble.OnBoard)
                        ro.RelativeScale = new Vector3(CreateAnim.Value);
                    else if (Board.PreviewMode == MarblePreview.Board)
                        ro.RelativeScale = new Vector3(CreateAnim.Value * 0.35f);
                }
                if (marble.State == MarbleState.Removing || marble.State == MarbleState.Exploding)
                {
                    ro.RelativeScale = new Vector3(RemoveAnim.Value);
                }
                if (marble == SelectedMarble && CurrentPath != null && MoveAnim.Enabled)
                {
                    var result = GetPathPosition(marble);
                    ro.RelativeTranslation = result.Position;
                    ro.RelativeRotation = result.Rotate;
                }
                else
                {
                    ro.RelativeTranslation = GetMarblePos(marble);
                    ro.RelativeRotation = Quaternion.Identity;
                }
            }

            GameContext.GetActor("UI").GetComponent<UILabelComponent>("TotalScore").Text = Board.TotalScore.ToString();
            GameContext.GetActor("UI").GetComponent<UILabelComponent>("LastScore").Text = Board.LastMoveScore.ToString();

            // Test Rotation:
            //Board.Marbles.Last().RenderObject.RelativeRotation = Quaternion.FromEulerAngles(0.2f, 0.5f, 0.7f);
        }

        private (Vector3 Position, Quaternion Rotate) GetPathPosition(Marble marble)
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
            var subDirection = new Vector3(direction.X, direction.Y, 0.0f) * subPos;
            var resultPos = GetMarblePos(fromPos) + subDirection;
            var resultRotate = new Vector3(direction.Y, direction.X, 0) * (subPos * 0.5f);

            return (resultPos, resultRotate.ToQuaternion());
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

        private Vector3 GetMarblePos(Marble marble)
        {
            if (marble.OnBoard || Board.PreviewMode == MarblePreview.Board)
                return GetMarblePos(marble.OnBoard ? marble.Position : marble.Position);

            if (Board.PreviewMode == MarblePreview.Side)
            {
                var idx = Board.NextMarbles.IndexOf(marble);
                return new Vector3(0, idx, MarbleZ);
            }
            else
            {
                return Vector3.Zero;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (CurrentMouseWorldPositionIsValid)
            {
                var cursor = BoardActor.GetComponent<SceneComponent>("GroundCursor");
                cursor.RelativeTranslation = new Vector3(CurrentMouseWorldPosition.X, CurrentMouseWorldPosition.Y, cursor.RelativeTranslation.Z);
            }
        }

        private Marble SelectedMarble;

        private GameMaterial GetMaterial(Marble marble)
        {
            var material = new GameMaterial()
            {
                Color = GetMaterialColorShader(marble.Color1),
                Ambient = 0.5f,
                Shininess = 32.0f,
                SpecularStrength = 0.5f,
                CastShadow = true,
            };
            material.SetDefine("OVERRIDE_GET_MATERIAL_DIFFUSE_FILE", "marble.material.glsl");
            material.SetDefine("FRAG_HEADER_FILE", "marble.params.glsl");
            return material;
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
        protected override void OnMouseDown(MouseButtonArgs e)
        {
            if (CurrentMouseWorldPositionIsValid)
            {
                var translatedPos = (new Vector4(CurrentMouseWorldPosition, 1) * BoardTranslationMatrix).Xyz;
                Console.WriteLine(translatedPos);
                var pos = translatedPos.Round().Xy.ToVector2i();

                if (!Board.PositionInMap(pos))
                    return;

                //ScaleAnim.Start();
                var selector = BoardActor.GetComponent<SceneComponent>("MarbleSelector");

                if (MoveAnim.Enabled || RemoveAnim.Enabled || CreateAnim.Enabled)
                    return;

                if (Board[pos]?.Color == MarbleColor.BombJoker)
                    return;

                var marble = Board[pos];
                Console.WriteLine($"Clicked: {pos}. Marble: {marble}");
                if (marble != null)
                {
                    SelectedMarble = marble;
                    selector.RelativeTranslation = new Vector3(pos.X, pos.Y, 0);
                    selector.Visible = true;
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
                            selector.Visible = false;
                        }
                    }
                }
            }
        }

    }

}
