﻿#define DEMO

using GDGame.Controllers;
using GDGame.MyGame.Managers;
using GDLibrary.Actors;
using GDLibrary.Containers;
using GDLibrary.Controllers;
using GDLibrary.Core.Controllers;
using GDLibrary.Debug;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Factories;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.MyGame;
using GDLibrary.Parameters;
using GDLibrary.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace GDGame
{
    public class Main : Game
    {
        #region Fields

        private int resolutionX = 1280;
        private int resolutionY = 720;

        private const float SPAWNTIMER = 2f;
        private float spawnTimer = SPAWNTIMER;

        private const int MAXHEALTH = 100;
        private int currentHealth = MAXHEALTH;

        private const int BASESPEED = 5000;

        private int score;

        private float currentSpeed = BASESPEED;
        private int currentLevel = 1;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private CameraManager<Camera3D> cameraManager;
        private ObjectManager objectManager;
        private KeyboardManager keyboardManager;
        private MouseManager mouseManager;
        private RenderManager renderManager;
        private UIManager uiManager;
        private MyMenuManager menuManager;
        private SoundManager soundManager;

        //used to process and deliver events received from publishers
        private EventDispatcher eventDispatcher;

        //store useful game resources (e.g. effects, models, rails and curves)
        private Dictionary<string, BasicEffect> effectDictionary;

        //use ContentDictionary to store assets (i.e. file content) that need the Content.Load() method to be called
        private ContentDictionary<Texture2D> textureDictionary;

        private ContentDictionary<SpriteFont> fontDictionary;
        private ContentDictionary<Model> modelDictionary;

        //use normal Dictionary to store objects that do NOT need the Content.Load() method to be called (i.e. the object is not based on an asset file)
        private Dictionary<string, Transform3DCurve> transform3DCurveDictionary;

        //stores the rails used by the camera
        private Dictionary<string, RailParameters> railDictionary;

        //stores the archetypal primitive objects (used in Main and LevelLoader)
        private Dictionary<string, PrimitiveObject> archetypeDictionary;

        //defines centre point for the mouse i.e. (w/2, h/2)
        private Vector2 screenCentre;

        private CollidablePlayerObject collidablePlayerObject;

        #endregion Fields

        #region Constructors

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        #endregion Constructors

        #region Debug
#if DEBUG

        private void InitDebug()
        {
            InitDebugInfo(true);
            bool bShowCDCRSurfaces = true;
            bool bShowZones = true;
            InitializeDebugCollisionSkinInfo(bShowCDCRSurfaces, bShowZones, Color.White);
        }

        private void InitializeDebugCollisionSkinInfo(bool bShowCDCRSurfaces, bool bShowZones, Color boundingBoxColor)
        {
            //draws CDCR surfaces for boxes and spheres
            PrimitiveDebugDrawer primitiveDebugDrawer =
                new PrimitiveDebugDrawer(this, StatusType.Drawn | StatusType.Update,
                objectManager, cameraManager,
                bShowCDCRSurfaces, bShowZones);

            primitiveDebugDrawer.DrawOrder = 5;
            BoundingBoxDrawer.BoundingBoxColor = boundingBoxColor;

            Components.Add(primitiveDebugDrawer);
        }

        private void InitDebugInfo(bool bEnable)
        {
            if (bEnable)
            {
                //create the debug drawer to draw debug info
                DebugDrawer debugInfoDrawer = new DebugDrawer(this, _spriteBatch,
                    Content.Load<SpriteFont>("Assets/Fonts/debug"),
                    cameraManager, objectManager);

                //set the debug drawer to be drawn AFTER the object manager to the screen
                debugInfoDrawer.DrawOrder = 2;

                //add the debug drawer to the component list so that it will have its Update/Draw methods called each cycle.
                Components.Add(debugInfoDrawer);
            }
        }

#endif
        #endregion Debug

        #region Load - Assets

        private void LoadSounds()
        {
            soundManager.Add(new GDLibrary.Managers.Cue("smokealarm",
                Content.Load<SoundEffect>("Assets/Audio/Effects/smokealarm1"), SoundCategoryType.Alarm, new Vector3(1, 0, 0), false));

            //soundManager.Add(new GDLibrary.Managers.Cue("thing", Content.Load<>))


            soundManager.Add(new GDLibrary.Managers.Cue("hitgood",
                Content.Load<SoundEffect>("Assets/Audio/Effects/hitgood"), SoundCategoryType.Explosion, new Vector3(1, 0, 0), false));

            soundManager.Add(new GDLibrary.Managers.Cue("hitbad",
                Content.Load<SoundEffect>("Assets/Audio/Effects/hitbad"), SoundCategoryType.Explosion, new Vector3(1, 0, 0), false));

            soundManager.Add(new GDLibrary.Managers.Cue("end",
                Content.Load<SoundEffect>("Assets/Audio/Effects/end"), SoundCategoryType.Explosion, new Vector3(1, 0, 0), false));

            soundManager.Add(new GDLibrary.Managers.Cue("bg1",
                Content.Load<SoundEffect>("Assets/Audio/bg/bg1"), SoundCategoryType.BackgroundMusic, new Vector3(1, 0, 0), false));

            soundManager.Add(new GDLibrary.Managers.Cue("bg2",
                Content.Load<SoundEffect>("Assets/Audio/bg/bg2"), SoundCategoryType.BackgroundMusic, new Vector3(1, 0, 0), true));

            soundManager.Add(new GDLibrary.Managers.Cue("bg3",
                Content.Load<SoundEffect>("Assets/Audio/bg/bg3"), SoundCategoryType.BackgroundMusic, new Vector3(1, 0, 0), true));
        }

        private void LoadEffects()
        {
            //to do...
            BasicEffect effect = null;

            //used for unlit primitives with a texture (e.g. textured quad of skybox)
            effect = new BasicEffect(_graphics.GraphicsDevice);
            effect.VertexColorEnabled = true; //otherwise we wont see RGB
            effect.TextureEnabled = true;
            effectDictionary.Add(GameConstants.Effect_UnlitTextured, effect);

            //used for wireframe primitives with no lighting and no texture (e.g. origin helper)
            effect = new BasicEffect(_graphics.GraphicsDevice);
            effect.VertexColorEnabled = true;
            effectDictionary.Add(GameConstants.Effect_UnlitWireframe, effect);

            //to do...add a new effect to draw a lit textured surface (e.g. a lit pyramid)
            effect = new BasicEffect(_graphics.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.LightingEnabled = true; //redundant?
            effect.PreferPerPixelLighting = true; //cost GPU cycles
            effect.EnableDefaultLighting();
            
            //change lighting position, direction and color

            effectDictionary.Add(GameConstants.Effect_LitTextured, effect);
        }

        private void LoadTextures()
        {
            //level 1 where each image 1_1, 1_2 is a different Y-axis height specificied when we use the level loader
            textureDictionary.Load("Assets/Textures/Level/level1_1");
            textureDictionary.Load("Assets/Textures/Level/level1_2");
            //add more levels here...

            //sky
            textureDictionary.Load("Assets/Textures/Skybox/back");
            textureDictionary.Load("Assets/Textures/Skybox/left");
            textureDictionary.Load("Assets/Textures/Skybox/right");
            textureDictionary.Load("Assets/Textures/Skybox/front");
            textureDictionary.Load("Assets/Textures/Skybox/sky");
            textureDictionary.Load("Assets/Textures/Foliage/Ground/grass1");

            //demo
            textureDictionary.Load("Assets/Demo/Textures/checkerboard");

            //ui
            textureDictionary.Load("Assets/Textures/UI/Controls/progress_white");

            //props
            textureDictionary.Load("Assets/Textures/Props/Crates/crate1");

            //menu
            textureDictionary.Load("Assets/Textures/UI/Controls/genericbtn");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/mainmenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/audiomenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/controlsmenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/exitmenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/exitmenuwithtrans");

            //ui
            textureDictionary.Load("Assets/Textures/UI/Controls/reticuleSpace");
            textureDictionary.Load("Assets/Textures/UI/uiMain");

            //add more...
            textureDictionary.Load("Assets/Textures/Base/ship");

            textureDictionary.Load("Assets/Textures/Base/texturespace");
            textureDictionary.Load("Assets/Textures/Base/spaceback");

            textureDictionary.Load("Assets/Textures/Base/spaceempty");
        }

        private void LoadFonts()
        {
            fontDictionary.Load("Assets/Fonts/debug");
            fontDictionary.Load("Assets/Fonts/menu");
            fontDictionary.Load("Assets/Fonts/ui");
        }

        #endregion Load - Assets

        #region Initialization - Graphics, Managers, Dictionaries, Cameras, Menu, UI

        protected override void Initialize()
        {
            float worldScale = 2000;
            //set game title
            Window.Title = "Void Schism";

            //graphic settings - see https://en.wikipedia.org/wiki/Display_resolution#/media/File:Vector_Video_Standards8.svg
            InitGraphics(resolutionX, resolutionY);

            //note that we moved this from LoadContent to allow InitDebug to be called in Initialize
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //create event dispatcher
            InitEventDispatcher();

            //managers
            InitManagers();

            //dictionaries
            InitDictionaries();

            //load from file or initialize assets, effects and vertices
            LoadEffects();
            LoadTextures();
            LoadFonts();
            LoadSounds();

            //ui
            InitUI();
            InitMenu();

            //add archetypes that can be cloned
            InitArchetypes();

            //drawn content (collidable and noncollidable together - its simpler)
            InitLevel(worldScale);

            //curves and rails used by cameras
            InitCurves();
            InitRails();

            //cameras - notice we moved the camera creation BELOW where we created the drawn content - see DriveController
            InitCameras3D();

            #region Debug
#if DEBUG
            //debug info
            //InitDebug();
#endif
            #endregion Debug

            base.Initialize();
        }

        private void InitGraphics(int width, int height)
        {
            //set resolution
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;

            //dont forget to apply resolution changes otherwise we wont see the new WxH
            _graphics.ApplyChanges();

            //set screen centre based on resolution
            screenCentre = new Vector2(width / 2, height / 2);

            //set cull mode to show front and back faces - inefficient but we will change later
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RasterizerState = rs;

            //we use a sampler state to set the texture address mode to solve the aliasing problem between skybox planes
            SamplerState samplerState = new SamplerState();
            samplerState.AddressU = TextureAddressMode.Clamp;
            samplerState.AddressV = TextureAddressMode.Clamp;
            _graphics.GraphicsDevice.SamplerStates[0] = samplerState;

            //set blending
            _graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //set screen centre for use when centering mouse
            screenCentre = new Vector2(width / 2, height / 2);
        }

        private void InitUI()
        {
            Transform2D transform2D = null;
            Texture2D texture = null;
            SpriteFont spriteFont = null;

            #region Mouse Reticule & Text
            texture = textureDictionary["reticuleSpace"];

            transform2D = new Transform2D(
                new Vector2(512, 384), //this value doesnt matter since we will recentre in UIMouseObject::Update()
                0,
                 Vector2.One,
                new Vector2(texture.Width / 2, texture.Height / 2),
                new Integer2(100, 100)); //read directly from the PNG file dimensions

            UIMouseObject uiMouseObject = new UIMouseObject("reticule", ActorType.UIMouse,
                StatusType.Update | StatusType.Drawn, transform2D, Color.White,
                SpriteEffects.None, fontDictionary["menu"],
                "",
                new Vector2(0, -55),
                Color.LightCyan,
                0.75f * Vector2.One,
                0,
                texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height), //how much of source image do we want to draw?
                mouseManager);

            //uiManager.Add(uiMouseObject);
            #endregion Mouse Reticule & Text


            #region Progress Control Left
            texture = textureDictionary["progress_white"];

            transform2D = new Transform2D(new Vector2(512, 20),
                0,
                 Vector2.One,
                new Vector2(texture.Width / 2, texture.Height / 2),
                new Integer2(100, 100));

            UITextureObject uiTextureObject = new UITextureObject("progress 1", ActorType.UITextureObject,
                StatusType.Drawn | StatusType.Update, transform2D, Color.Yellow, 0, SpriteEffects.None,
                texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));

            //uiTextureObject.ControllerList.Add(new UIRotationController("rc1", ControllerType.RotationOverTime));

            //uiTextureObject.ControllerList.Add(new UIColorLerpController("clc1", ControllerType.ColorLerpOverTime,
            //    Color.White, Color.Black));

            //uiTextureObject.ControllerList.Add(new UIMouseController("moc1", ControllerType.MouseOver,
            //    this.mouseManager));

            //uiTextureObject.ControllerList.Add(new UIProgressController("pc1", ControllerType.Progress, 0, 10));

            //uiManager.Add(uiTextureObject);
            #endregion Progress Control Left

            texture = textureDictionary["uiMain"];

            transform2D = new Transform2D(new Vector2(resolutionX / 2, resolutionY / 2), 0, Vector2.One, new Vector2(texture.Width / 2, texture.Height / 2), new Integer2(100, 100));

            uiTextureObject = new UITextureObject("overlay", ActorType.UITextureObject,
                StatusType.Drawn, transform2D, Color.White, 0, SpriteEffects.None,
                texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));

            uiManager.Add(uiTextureObject);


            #region Text Object
            spriteFont = Content.Load<SpriteFont>("Assets/Fonts/ui");

            //calculate how big the text is in (w,h)
            string text = "SCORE -[0]- ";
            Vector2 originalDimensions = spriteFont.MeasureString(text);

            transform2D = new Transform2D(new Vector2(resolutionX / 2 + (originalDimensions.X / 2), 45), 0, new Vector2(2, 2), new Vector2(originalDimensions.X, originalDimensions.Y), new Integer2(originalDimensions));

            UITextObject uiTextObject = new UITextObject("score", ActorType.UIText,
                StatusType.Update | StatusType.Drawn, transform2D, new Color(255, 0, 255, 1),
                0, SpriteEffects.None, text, spriteFont);

            uiTextObject.ControllerList.Add(new UIScoreController("scoreUpdate", ControllerType.Score, score));

            //uiTextObject.ControllerList.Add(new UIMouseOverController("moc1", ControllerType.MouseOver,
            //     mouseManager, Color.Red, Color.White));


            uiManager.Add(uiTextObject);

            //uiTextObject.Transform2D.Translation = new Vector2(resolutionX / 2 + (originalDimensions.X / 2), 200);
            //uiTextObject.Transform2D.Scale = new Vector2(10, 10);

            menuManager.Add("exit", uiTextObject);
            
            uiTextObject = null;
            //spriteFont = Content.Load<SpriteFont>("Assets/Fonts/ui");

            //calculate how big the text is in (w,h)
            text = "SHIELD HEALTH -[0]- ";
            originalDimensions = spriteFont.MeasureString(text);

            transform2D = new Transform2D(new Vector2(resolutionX / 2 + (originalDimensions.X / 2) + (500), 45), 0, new Vector2(2, 2), new Vector2(originalDimensions.X, originalDimensions.Y), new Integer2(originalDimensions));

            uiTextObject = new UITextObject("health", ActorType.UIText,
                StatusType.Update | StatusType.Drawn, transform2D, new Color(255, 0, 255, 1),
                0, SpriteEffects.None, text, spriteFont);

            uiTextObject.ControllerList.Add(new UIHealthController("healtheUpdate", ControllerType.Health, currentHealth, MAXHEALTH));

            uiManager.Add(uiTextObject);

            #endregion Text Object

        }

        private void InitMenu()
        {
            Texture2D texture = null;
            Transform2D transform2D = null;
            DrawnActor2D uiObject = null;
            Vector2 fullScreenScaleFactor = Vector2.One;

            #region All Menu Background Images
            //background main
            texture = textureDictionary["exitmenuwithtrans"];
            fullScreenScaleFactor = new Vector2((float)_graphics.PreferredBackBufferWidth / texture.Width, (float)_graphics.PreferredBackBufferHeight / texture.Height);

            transform2D = new Transform2D(fullScreenScaleFactor);
            uiObject = new UITextureObject("main_bckgnd", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.LightGreen, 1, SpriteEffects.None, texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));
            menuManager.Add("main", uiObject);

            //background audio
            texture = textureDictionary["audiomenu"];
            fullScreenScaleFactor = new Vector2((float)_graphics.PreferredBackBufferWidth / texture.Width, (float)_graphics.PreferredBackBufferHeight / texture.Height);
            transform2D = new Transform2D(fullScreenScaleFactor);
            uiObject = new UITextureObject("audio_bckgnd", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.White, 1, SpriteEffects.None, texture, new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));
            menuManager.Add("audio", uiObject);

            //background controls
            texture = textureDictionary["controlsmenu"];
            fullScreenScaleFactor = new Vector2((float)_graphics.PreferredBackBufferWidth / texture.Width, (float)_graphics.PreferredBackBufferHeight / texture.Height);
            transform2D = new Transform2D(fullScreenScaleFactor);
            uiObject = new UITextureObject("controls_bckgnd", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.White, 1, SpriteEffects.None, texture, new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));
            menuManager.Add("controls", uiObject);

            //background exit
            texture = textureDictionary["exitmenuwithtrans"];
            fullScreenScaleFactor = new Vector2((float)_graphics.PreferredBackBufferWidth / texture.Width, (float)_graphics.PreferredBackBufferHeight / texture.Height);
            transform2D = new Transform2D(fullScreenScaleFactor);
            uiObject = new UITextureObject("exit_bckgnd", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.White, 1, SpriteEffects.None, texture, new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));
            menuManager.Add("exit", uiObject);
            #endregion All Menu Background Images

            //main menu buttons
            texture = textureDictionary["genericbtn"];

            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            Integer2 imageDimensions = new Integer2(texture.Width, texture.Height);

            //play
            transform2D = new Transform2D(screenCentre - new Vector2(0, 50), 0, Vector2.One, origin, imageDimensions);
            uiObject = new UIButtonObject("play", ActorType.UITextureObject, StatusType.Drawn | StatusType.Update,
                transform2D, Color.White, 1, SpriteEffects.None, texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height),
                "Play",
                fontDictionary["menu"],
                new Vector2(1, 1),
                Color.Black,
                new Vector2(0, 0));

            uiObject.ControllerList.Add(new UIMouseOverController("moc2", ControllerType.MouseOver,
                 mouseManager, Color.BlueViolet, Color.White));

            uiObject.ControllerList.Add(new UIScaleLerpController("slc2", ControllerType.ScaleLerpOverTime,
              mouseManager, new TrigonometricParameters(0.02f, 1, 0)));


            menuManager.Add("main", uiObject);
            //exit
            transform2D = new Transform2D(screenCentre + new Vector2(0, 50), 0, Vector2.One, origin, imageDimensions);
            uiObject = new UIButtonObject("exit", ActorType.UITextureObject,
                StatusType.Update | StatusType.Drawn,
             transform2D, Color.White, 1, SpriteEffects.None, texture,
             new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height),
             "Exit",
             fontDictionary["menu"],
             new Vector2(1, 1),
             Color.Black,
             new Vector2(0, 0));

            uiObject.ControllerList.Add(new UIMouseOverController("moc1", ControllerType.MouseOver,
                 mouseManager, Color.BlueViolet, Color.White));

            uiObject.ControllerList.Add(new UIScaleLerpController("slc1", ControllerType.ScaleLerpOverTime,
              mouseManager, new TrigonometricParameters(0.02f, 1, 0)));

            menuManager.Add("main", uiObject);

            menuManager.Add("exit", uiObject);

            //finally dont forget to SetScene to say which menu should be drawn/updated!
            menuManager.SetScene("main");

            SpriteFont spriteFont = Content.Load<SpriteFont>("Assets/Fonts/ui");

            string text = " -[  VOID SCHIZM  ]-  ";
            Vector2 originalDimensions = spriteFont.MeasureString(text);

            transform2D = new Transform2D(new Vector2(965, 200), 0, new Vector2(5, 5), new Vector2(originalDimensions.X, originalDimensions.Y), new Integer2(originalDimensions));

            UITextObject uiTextObject = new UITextObject("health", ActorType.UIText,
                StatusType.Update | StatusType.Drawn, transform2D, new Color(255, 0, 255, 1),
                0, SpriteEffects.None, text, spriteFont);

            menuManager.Add("main", uiTextObject);
        }

        private void InitEventDispatcher()
        {
            eventDispatcher = new EventDispatcher(this);
            Components.Add(eventDispatcher);
        }

        private void InitCurves()
        {
            //create the camera curve to be applied to the track controller
            Transform3DCurve curveA = new Transform3DCurve(CurveLoopType.Oscillate); //experiment with other CurveLoopTypes
            curveA.Add(new Vector3(0, 5, 100), -Vector3.UnitZ, Vector3.UnitY, 0); //start
            curveA.Add(new Vector3(0, 5, 80), new Vector3(1, 0, -1), Vector3.UnitY, 1000); //start position
            curveA.Add(new Vector3(0, 5, 50), -Vector3.UnitZ, Vector3.UnitY, 3000); //start position
            curveA.Add(new Vector3(0, 5, 20), new Vector3(-1, 0, -1), Vector3.UnitY, 4000); //start position
            curveA.Add(new Vector3(0, 5, 10), -Vector3.UnitZ, Vector3.UnitY, 6000); //start position

            //add to the dictionary
            transform3DCurveDictionary.Add("headshake1", curveA);

            curveA = new Transform3DCurve(CurveLoopType.Oscillate); //experiment with other CurveLoopTypes
            curveA.Add(new Vector3(0, 5, 100), -Vector3.UnitZ, Vector3.UnitY, 0); //start
            curveA.Add(new Vector3(0, 5, 80), new Vector3(1, 0, -1), Vector3.UnitY, 1000); //start position
            curveA.Add(new Vector3(0, 5, 50), -Vector3.UnitZ, Vector3.UnitY, 3000); //start position
            curveA.Add(new Vector3(0, 5, 20), new Vector3(-1, 0, -1), Vector3.UnitY, 4000); //start position
            curveA.Add(new Vector3(0, 5, 10), -Vector3.UnitZ, Vector3.UnitY, 6000); //start position

            //add to the dictionary
            transform3DCurveDictionary.Add("enemypath", curveA);


            curveA = new Transform3DCurve(CurveLoopType.Linear); //experiment with other CurveLoopTypes
            curveA.Add(new Vector3(0, 5, 100), -Vector3.UnitZ, Vector3.UnitY, 0); //start
            curveA.Add(new Vector3(0, 5, 80), new Vector3(1, 0, -1), Vector3.UnitY, 1000); //start position
            curveA.Add(new Vector3(0, 5, 50), -Vector3.UnitZ, Vector3.UnitY, 3000); //start position
            curveA.Add(new Vector3(0, 5, 20), new Vector3(-1, 0, -1), Vector3.UnitY, 4000); //start position
            curveA.Add(new Vector3(0, 5, 10), -Vector3.UnitZ, Vector3.UnitY, 6000); //start position

            //add to the dictionary
            transform3DCurveDictionary.Add("introCam", curveA);



        }

        private void InitRails()
        {
            //create the track to be applied to the non-collidable track camera 1
            railDictionary.Add("rail1", new RailParameters("rail1 - parallel to z-axis", new Vector3(20, 10, 50), new Vector3(20, 10, -50)));

            railDictionary.Add("enemyrail", new RailParameters("rail1 - parallel to z-axis", new Vector3(20, 10, 50), new Vector3(20, 10, -50)));
        }

        private void InitDictionaries()
        {
            //stores effects
            effectDictionary = new Dictionary<string, BasicEffect>();

            //stores textures, fonts & models
            modelDictionary = new ContentDictionary<Model>("models", Content);
            textureDictionary = new ContentDictionary<Texture2D>("textures", Content);
            fontDictionary = new ContentDictionary<SpriteFont>("fonts", Content);

            //curves - notice we use a basic Dictionary and not a ContentDictionary since curves and rails are NOT media content
            transform3DCurveDictionary = new Dictionary<string, Transform3DCurve>();

            //rails - store rails used by cameras
            railDictionary = new Dictionary<string, RailParameters>();

            //used to store archetypes for primitives in the game
            archetypeDictionary = new Dictionary<string, PrimitiveObject>();
        }

        private void InitManagers()
        {
            //physics and CD-CR (moved to top because MouseManager is dependent)
            //to do - replace with simplified CDCR

            //camera
            cameraManager = new CameraManager<Camera3D>(this, StatusType.Off);
            Components.Add(cameraManager);

            //keyboard
            keyboardManager = new KeyboardManager(this);
            Components.Add(keyboardManager);

            //mouse
            mouseManager = new MouseManager(this, true, screenCentre);
            Components.Add(mouseManager);

            //object
            objectManager = new ObjectManager(this, StatusType.Off, 6, 10);
            Components.Add(objectManager);

            //render
            renderManager = new RenderManager(this, StatusType.Drawn, ScreenLayoutType.Single,
                objectManager, cameraManager);
            Components.Add(renderManager);

            //add in-game ui
            uiManager = new UIManager(this, StatusType.Off, _spriteBatch, 10);
            uiManager.DrawOrder = 4;
            Components.Add(uiManager);

            //add menu
            menuManager = new MyMenuManager(this, StatusType.Update | StatusType.Drawn, _spriteBatch,
                mouseManager, keyboardManager);
            menuManager.DrawOrder = 5; //highest number of all drawable managers since we want it drawn on top!
            Components.Add(menuManager);

            //sound
            soundManager = new SoundManager(this, StatusType.Update);
            Components.Add(soundManager);
        }

        private void InitCameras3D()
        {
            Transform3D transform3D = null;
            Camera3D camera3D = null;
            Viewport viewPort = new Viewport(0, 0, resolutionX, resolutionY);

            #region Fixed

            Vector3 translation = new Vector3(0, 300, 200);

            Vector3 rotationInDegrees = new Vector3(0, 0, 0);

            transform3D = new Transform3D(translation, rotationInDegrees, Vector3.One, new Vector3(0, -0.32f, -1), Vector3.UnitY);

            camera3D = new Camera3D(" Fixed Camera - Main 2",
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen,
                viewPort);

            Transform3DCurve curveA = new Transform3DCurve(CurveLoopType.Constant); //experiment with other CurveLoopTypes
            curveA.Add(new Vector3(0, 5, 100), -Vector3.UnitZ, Vector3.UnitY, 0); //start
            curveA.Add(new Vector3(0, 5, 80), new Vector3(1, 0, -1), Vector3.UnitY, 1000); //start position
            curveA.Add(new Vector3(0, 5, 50), -Vector3.UnitZ, Vector3.UnitY, 3000); //start position
            curveA.Add(new Vector3(0, 5, 20), new Vector3(-1, 0, -1), Vector3.UnitY, 4000); //start position
            curveA.Add(new Vector3(0, 300, 200), new Vector3(0, -0.32f, -1), Vector3.UnitY, 6000); //start position

            transform3DCurveDictionary.Add("cameraCurve", curveA);

            curveA = new Transform3DCurve(CurveLoopType.Constant); //experiment with other CurveLoopTypes
            curveA.Add(new Vector3(0, 5, 100), -Vector3.UnitZ, Vector3.UnitY, 0); //start
            curveA.Add(new Vector3(0, 5, 80), new Vector3(1, 0, -1), Vector3.UnitY, 500); //start position
            curveA.Add(new Vector3(0, 5, 50), -Vector3.UnitZ, Vector3.UnitY, 1500); //start position
            curveA.Add(new Vector3(0, 5, 20), new Vector3(-1, 0, -1), Vector3.UnitY, 3000); //start position
            curveA.Add(translation, transform3D.Look, Vector3.UnitY, 4000); //start position

            transform3DCurveDictionary.Add("cameraCurve2", curveA);

            camera3D.ControllerList.Add(new Curve3DController("intro curve", ControllerType.Curve, transform3DCurveDictionary["cameraCurve"]));

            camera3D.ControllerList.Add(new Curve3DController(GameConstants.Camera_NonCollidableCurveMainArena, ControllerType.Curve, transform3DCurveDictionary["cameraCurve2"]));

            cameraManager.Add(camera3D);

            #endregion Fixed


            #region Fixed

            translation = new Vector3(0, 300, 200);

            rotationInDegrees = new Vector3(0, 0, 0);

            transform3D = new Transform3D(translation, rotationInDegrees, Vector3.One, new Vector3(0, -0.32f, -1), Vector3.UnitY);

            camera3D = new Camera3D(" Fixed Camera - Main ",
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen,
                viewPort);

            cameraManager.Add(camera3D);

            #endregion Fixed


            #region Collidable Camera - 3rd Person

            transform3D = new Transform3D(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);

            camera3D = new Camera3D(GameConstants.Camera_CollidableThirdPerson,
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen,
                viewPort);

            //attach a controller
            camera3D.ControllerList.Add(new ThirdPersonController(
                GameConstants.Controllers_CollidableThirdPerson,
                ControllerType.ThirdPerson,
                collidablePlayerObject,
                165,
                150,
                1,
                mouseManager));

            cameraManager.Add(camera3D);

            #endregion Collidable Camera - 3rd Person

            #region Noncollidable Camera - First Person

            transform3D = new Transform3D(new Vector3(10, 10, 20),
                new Vector3(0, 0, -1), Vector3.UnitY);

            camera3D = new Camera3D(GameConstants.Camera_NonCollidableFirstPerson,
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen,
                viewPort);

            //attach a controller
            camera3D.ControllerList.Add(new FirstPersonController(
                GameConstants.Controllers_NonCollidableFirstPerson,
                ControllerType.FirstPerson,
                keyboardManager, mouseManager,
                GameConstants.moveSpeed, GameConstants.strafeSpeed, GameConstants.rotateSpeed));
            cameraManager.Add(camera3D);

            #endregion Noncollidable Camera - First Person

            #region Noncollidable Camera - Flight

            transform3D = new Transform3D(new Vector3(10, 10, 20),
                new Vector3(0, 0, -1), Vector3.UnitY);

            camera3D = new Camera3D(GameConstants.Camera_NonCollidableFlight,
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen, viewPort);

            //attach a controller
            camera3D.ControllerList.Add(new FlightCameraController(
                GameConstants.Controllers_NonCollidableFlight, ControllerType.FlightCamera,
                keyboardManager, mouseManager, null,
                GameConstants.CameraMoveKeys,
                10 * GameConstants.moveSpeed,
                10 * GameConstants.strafeSpeed,
                GameConstants.rotateSpeed));
            cameraManager.Add(camera3D);

            #endregion Noncollidable Camera - Flight

            #region Noncollidable Camera - Security

            transform3D = new Transform3D(new Vector3(10, 10, 50),
                        new Vector3(0, 0, -1),
                        Vector3.UnitY);

            camera3D = new Camera3D(GameConstants.Camera_NonCollidableSecurity,
                ActorType.Camera3D, StatusType.Update, transform3D,
            ProjectionParameters.StandardDeepSixteenTen, viewPort);

            camera3D.ControllerList.Add(new PanController(
                GameConstants.Controllers_NonCollidableSecurity, ControllerType.Pan,
                new Vector3(1, 1, 0), new TrigonometricParameters(30, GameConstants.mediumAngularSpeed, 0)));
            cameraManager.Add(camera3D);

            #endregion Noncollidable Camera - Security

            #region Noncollidable Camera - Curve3D

            //notice that it doesnt matter what translation, look, and up are since curve will set these
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero, Vector3.Zero);

            camera3D = new Camera3D(GameConstants.Camera_NonCollidableCurveMainArena,
              ActorType.Camera3D, StatusType.Update, transform3D,
                        ProjectionParameters.StandardDeepSixteenTen, viewPort);

            camera3D.ControllerList.Add(
                new Curve3DController(GameConstants.Controllers_NonCollidableCurveMainArena,
                ControllerType.Curve,
                        transform3DCurveDictionary["headshake1"])); //use the curve dictionary to retrieve a transform3DCurve by id

            cameraManager.Add(camera3D);

            #endregion Noncollidable Camera - Curve3D

            #region Collidable Camera - 3rd Person backup

            transform3D = new Transform3D(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);

            camera3D = new Camera3D(GameConstants.Camera_CollidableThirdPerson,
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen,
                viewPort);

            //attach a controller
            camera3D.ControllerList.Add(new ThirdPersonController(
                GameConstants.Controllers_CollidableThirdPerson,
                ControllerType.ThirdPerson,
                collidablePlayerObject,
                135,
                40,
                1,
                mouseManager));
            cameraManager.Add(camera3D);

            #endregion Collidable Camera - 3rd Person backup

            #region Collidable Camera - Ship Cam

            transform3D = new Transform3D(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);

            camera3D = new Camera3D(GameConstants.Camera_CollidableThirdPerson,
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen,
                viewPort);

            //attach a controller
            //camera3D.ControllerList.Add(new ThirdPersonController(
            //    GameConstants.Controllers_CollidableThirdPerson,
            //    ControllerType.ThirdPerson,
            //    collidablePlayerObject,
            //    135,
            //    40,
            //    1,
            //    mouseManager));


            cameraManager.Add(camera3D);

            #endregion Collidable Camera - Ship Cam



            cameraManager.ActiveCameraIndex = 0; //0, 1, 2, 3
        }

        #endregion Initialization - Graphics, Managers, Dictionaries, Cameras, Menu, UI

        #region Initialization - Vertices, Archetypes, Helpers, Drawn Content(e.g. Skybox)

        /// <summary>
        /// Creates archetypes used in the game.
        ///
        /// What are the steps required to add a new primitive?
        ///    1. In the VertexFactory add a function to return Vertices[]
        ///    2. Add a new BasicEffect IFF this primitive cannot use existing effects(e.g.wireframe, unlit textured)
        ///    3. Add the new effect to effectDictionary
        ///    4. Create archetypal PrimitiveObject.
        ///    5. Add archetypal object to archetypeDictionary
        ///    6. Clone archetype, change its properties (transform, texture, color, alpha, ID) and add manually to the objectmanager or you can use LevelLoader.
        /// </summary>
        private void InitArchetypes() //formerly InitTexturedQuad
        {
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            PrimitiveType primitiveType;
            int primitiveCount;

            /************************* Non-Collidable  *************************/

            #region Lit Textured Pyramid

            /*********** Transform, Vertices and VertexData ***********/
            //lit pyramid
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero,
                 Vector3.One, Vector3.UnitZ, Vector3.UnitY);
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["checkerboard"], Color.White, 1);

            VertexPositionNormalTexture[] vertices
                = VertexFactory.GetVerticesPositionNormalTexturedPyramid(out primitiveType,
                out primitiveCount);

            //analog of the Model class in G-CA (i.e. it holdes vertices and type, count)
            vertexData = new VertexData<VertexPositionNormalTexture>(vertices,
                primitiveType, primitiveCount);

            /*********** PrimitiveObject ***********/
            //now we use the "FBX" file (our vertexdata) and make a PrimitiveObject
            PrimitiveObject primitiveObject = new PrimitiveObject(
                GameConstants.Primitive_LitTexturedPyramid,
                ActorType.Decorator, //we could specify any time e.g. Pickup
                StatusType.Drawn,
                transform3D, effectParameters,
                vertexData);

            /*********** Controllers (optional) ***********/
            //we could add controllers to the archetype and then all clones would have cloned controllers
            //  drawnActor3D.ControllerList.Add(
            //new RotationController("rot controller1", ControllerType.RotationOverTime,
            //1, new Vector3(0, 1, 0)));

            //to do...add demos of controllers on archetypes
            //ensure that the Clone() method of PrimitiveObject will Clone() all controllers

            archetypeDictionary.Add(primitiveObject.ID, primitiveObject);
            #endregion Lit Textured Pyramid

            #region Unlit Textured Quad
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero,
                  Vector3.One, Vector3.UnitZ, Vector3.UnitY);

            effectParameters = new EffectParameters(
                effectDictionary[GameConstants.Effect_UnlitTextured],
                textureDictionary["grass1"], Color.White, 1);

            vertexData = new VertexData<VertexPositionColorTexture>(
                VertexFactory.GetTextureQuadVertices(out primitiveType, out primitiveCount),
                primitiveType, primitiveCount);

            archetypeDictionary.Add(GameConstants.Primitive_UnlitTexturedQuad,
                new PrimitiveObject(GameConstants.Primitive_UnlitTexturedQuad,
                ActorType.Decorator,
                StatusType.Update | StatusType.Drawn,
                transform3D, effectParameters, vertexData));
            #endregion Unlit Textured Quad

            #region Unlit Origin Helper
            transform3D = new Transform3D(new Vector3(0, 20, 0),
                     Vector3.Zero, new Vector3(10, 10, 10),
                     Vector3.UnitZ, Vector3.UnitY);

            effectParameters = new EffectParameters(
                effectDictionary[GameConstants.Effect_UnlitWireframe],
                null, Color.White, 1);

            vertexData = new VertexData<VertexPositionColor>(VertexFactory.GetVerticesPositionColorOriginHelper(
                                    out primitiveType, out primitiveCount),
                                    primitiveType, primitiveCount);

            archetypeDictionary.Add(GameConstants.Primitive_WireframeOriginHelper,
                new PrimitiveObject(GameConstants.Primitive_WireframeOriginHelper,
                ActorType.Helper,
                StatusType.Update | StatusType.Drawn,
                transform3D, effectParameters, vertexData));

            #endregion Unlit Origin Helper

            #region Lit Textured Star

            /*********** Transform, Vertices and VertexData ***********/
            //lit star
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero,
                 Vector3.One, Vector3.UnitZ, Vector3.UnitY);
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["checkerboard"], Color.White, 1);

            vertices = VertexFactory.GetVerticesPositionNormalTexturedStar(out primitiveType,
                out primitiveCount);

            //analog of the Model class in G-CA (i.e. it holdes vertices and type, count)
            vertexData = new VertexData<VertexPositionNormalTexture>(vertices,
                primitiveType, primitiveCount);

            /*********** PrimitiveObject ***********/
            //now we use the "FBX" file (our vertexdata) and make a PrimitiveObject
            primitiveObject = new PrimitiveObject(
                GameConstants.Primitive_LitTexturedStar,
                ActorType.Decorator, //we could specify any time e.g. Pickup
                StatusType.Drawn,
                transform3D, effectParameters,
                vertexData);

            /*********** Controllers (optional) ***********/
            //we could add controllers to the archetype and then all clones would have cloned controllers
            //  drawnActor3D.ControllerList.Add(
            //new RotationController("rot controller1", ControllerType.RotationOverTime,
            //1, new Vector3(0, 1, 0)));

            //to do...add demos of controllers on archetypes
            //ensure that the Clone() method of PrimitiveObject will Clone() all controllers

            archetypeDictionary.Add(primitiveObject.ID, primitiveObject);
            #endregion Lit Textured Star

            #region Lit Textured Cylinder

            /*********** Transform, Vertices and VertexData ***********/
            //lit cylinder
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero,
                 Vector3.One, Vector3.UnitZ, Vector3.UnitY);
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["checkerboard"], Color.Red, 1);

            vertices = VertexFactory.GetVerticesPositionNormalTexturedCylinder(out primitiveType,
                out primitiveCount);

            //analog of the Model class in G-CA (i.e. it holdes vertices and type, count)
            vertexData = new VertexData<VertexPositionNormalTexture>(vertices,
                primitiveType, primitiveCount);

            /*********** PrimitiveObject ***********/
            //now we use the "FBX" file (our vertexdata) and make a PrimitiveObject
            primitiveObject = new PrimitiveObject(
                GameConstants.Primitive_LitTexturedCylinder,
                ActorType.Decorator, //we could specify any time e.g. Pickup
                StatusType.Drawn,
                transform3D, effectParameters,
                vertexData);

            /*********** Controllers (optional) ***********/
            //we could add controllers to the archetype and then all clones would have cloned controllers
            //  drawnActor3D.ControllerList.Add(
            //new RotationController("rot controller1", ControllerType.RotationOverTime,
            //1, new Vector3(0, 1, 0)));

            //to do...add demos of controllers on archetypes
            //ensure that the Clone() method of PrimitiveObject will Clone() all controllers

            archetypeDictionary.Add(primitiveObject.ID, primitiveObject);
            #endregion Lit Textured Cylinder

            #region Lit Textured Diamond

            /*********** Transform, Vertices and VertexData ***********/
            //lit star
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero,
                 Vector3.One, Vector3.UnitZ, Vector3.UnitY);
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["checkerboard"], Color.White, 1);

            vertices = VertexFactory.GetVerticesPositionNormalTexturedDiamond(out primitiveType,
                out primitiveCount);

            //analog of the Model class in G-CA (i.e. it holdes vertices and type, count)
            vertexData = new VertexData<VertexPositionNormalTexture>(vertices,
                primitiveType, primitiveCount);

            /*********** PrimitiveObject ***********/
            //now we use the "FBX" file (our vertexdata) and make a PrimitiveObject
            primitiveObject = new PrimitiveObject(
                GameConstants.Primitive_LitTexturedDiamond,
                ActorType.Decorator, //we could specify any time e.g. Pickup
                StatusType.Drawn,
                transform3D, effectParameters,
                vertexData);

            /*********** Controllers (optional) ***********/
            //we could add controllers to the archetype and then all clones would have cloned controllers
            //  drawnActor3D.ControllerList.Add(
            //new RotationController("rot controller1", ControllerType.RotationOverTime,
            //1, new Vector3(0, 1, 0)));

            //to do...add demos of controllers on archetypes
            //ensure that the Clone() method of PrimitiveObject will Clone() all controllers

            archetypeDictionary.Add(primitiveObject.ID, primitiveObject);
            #endregion Lit Textured Diamond

            //add more archetypes here...
        }

        private void InitLevel(float worldScale)//, List<string> levelNames)
        {
            //remove any old content (e.g. on restart or next level)

            objectManager.Clear();

            /************ Non-collidable ************/
            //adds origin helper etc
            InitHelpers();

            //add skybox
            InitSkybox(worldScale);

            //add grass plane
            InitGround(worldScale);

            //pyramids
            InitDecorators();

            /************ Collidable ************/

            InitCollidableProps();

            InitCollidablePickups();

            InitCollidableZones();

            InitializeCollidablePlayer();

            /************ Level-loader (can be collidable or non-collidable) ************/

            LevelLoader<PrimitiveObject> levelLoader = new LevelLoader<PrimitiveObject>(archetypeDictionary, textureDictionary);
            List<DrawnActor3D> actorList = null;

            //add level1_1 contents
            actorList = levelLoader.Load(
                textureDictionary["level1_1"],
                                10,     //number of in-world x-units represented by 1 pixel in image
                                10,     //number of in-world z-units represented by 1 pixel in image
                                20,     //y-axis height offset
                                new Vector3(0, 0, 0) //offset to move all new objects by
                                );
            //objectManager.Add(actorList);

            //clear the list otherwise when we add level1_2 we would re-add level1_1 objects to object manager
            actorList.Clear();

            //add level1_2 contents
            actorList = levelLoader.Load(
             textureDictionary["level1_2"],
                             10,     //number of in-world x-units represented by 1 pixel in image
                             10,     //number of in-world z-units represented by 1 pixel in image
                             40,     //y-axis height offset
                             new Vector3(-50, 0, -150) //offset to move all new objects by
                             );
            //objectManager.Add(actorList);
            actorList.Clear();

        }

        #region NEW - 26.12.20

        //adds a drivable player that can collide against collidable objects and zones

        private void Shoot()
        {

        }

        private void InitializeCollidablePlayer()
        {
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            ICollisionPrimitive collisionPrimitive = null;
            PrimitiveType primitiveType;
            int primitiveCount;

            //set the position
            transform3D = new Transform3D(new Vector3(0, 0, -250), Vector3.Zero, new Vector3(40, 20, 40),
                -Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["ship"], Color.White, 1);

            //get the vertex data object
            //vertexData = new VertexData<VertexPositionNormalTexture>(VertexFactory.GetVerticesPositionNormalTexturedCube(1, out primitiveType, out primitiveCount), primitiveType, primitiveCount);

            vertexData = new VertexData<VertexPositionNormalTexture>(VertexFactory.GetVerticesPositionNormalTexturedDiamond(out primitiveType, out primitiveCount), primitiveType, primitiveCount);

            //make a CDCR surface - sphere or box, its up to you - you dont need to pass transform to either primitive anymore
            collisionPrimitive = new SphereCollisionPrimitive(transform3D, 30);

            //if we make this a field then we can pass to the 3rd person camera controller
            collidablePlayerObject
                = new CollidablePlayerObject(
                    "player",
                    //this is important as it will determine how we filter collisions in our collidable player CDCR code
                    ActorType.CollidablePlayer,
                    StatusType.Drawn | StatusType.Update,
                    transform3D,
                    effectParameters,
                    vertexData,
                    collisionPrimitive,
                    objectManager,
                    GameConstants.KeysArrows,
                    GameConstants.playerMoveSpeed,
                    GameConstants.playerRotateSpeed,
                    keyboardManager);


            collidablePlayerObject.ControllerList.Add( new RotationController("rot controller1", ControllerType.RotationOverTime, 6f, new Vector3(0, 1, 0)));

            objectManager.Add(collidablePlayerObject);
        }

        private void InitCollidableZones()
        {
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            CollidablePrimitiveObject collidablePrimitiveObject = null;
            PrimitiveType primitiveType;
            int primitiveCount;

            Transform3D transform3D = null;
            ICollisionPrimitive collisionPrimitive = null;
            CollidableZoneObject collidableZoneObject = null;

            //transform3D = new Transform3D(new Vector3(0, -50, 0), Vector3.Zero, new Vector3(1000, 1, 1000), Vector3.UnitZ, Vector3.UnitY);

            ////make the collision primitive - changed slightly to no longer need transform
            //collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //collidableZoneObject = new CollidableZoneObject("game end", ActorType.CollidableZone,
            //    StatusType.Drawn | StatusType.Update,
            //    transform3D,
            //    collisionPrimitive);

            //objectManager.Add(collidableZoneObject);







            transform3D = new Transform3D(new Vector3(0, 0, -210), Vector3.Zero, new Vector3(1000, 1000, 0), Vector3.UnitZ, Vector3.UnitY);

            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            collidableZoneObject = new CollidableZoneObject("kill", ActorType.CollidableZone,
                StatusType.Drawn | StatusType.Update,
                transform3D,
                collisionPrimitive);

            objectManager.Add(collidableZoneObject);






            transform3D = new Transform3D(new Vector3(-1000, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1000, 1000), Vector3.UnitZ, Vector3.UnitY);

            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["ship"], Color.Black, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(VertexFactory.GetVerticesPositionNormalTexturedCube(1,out primitiveType, out primitiveCount),primitiveType, primitiveCount);

            //vertexData = new VertexData<VertexPositionNormalTexture>(VertexFactory.GetVerticesPositionColorTextureQuad(1, out primitiveType, out primitiveCount));



            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidablePrimitiveObject(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.CollidableDecorator,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager);

            collidablePrimitiveObject.Transform3D.RotateBy(new Vector3(0,0,0));

            //objectManager.Add(collidablePrimitiveObject);
            //objectManager.Add(drawnActor3D);


        }

        private void InitCollidableProps()
        {
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            ICollisionPrimitive collisionPrimitive = null;
            CollidablePrimitiveObject collidablePrimitiveObject = null;
            PrimitiveType primitiveType;
            int primitiveCount;

            /************************* Box Collision Primitive  *************************/

            transform3D = new Transform3D(new Vector3(10, 0, 0), Vector3.Zero, new Vector3(6, 8, 6), Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["crate1"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidablePrimitiveObject(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.CollidableDecorator,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager);


            transform3D = new Transform3D(new Vector3(250, 0, 0), Vector3.Zero, new Vector3(1 ,1000, 1000), Vector3.UnitZ, Vector3.UnitY);

            //add to the archetype dictionary
            //objectManager.Add(collidablePrimitiveObject);

            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured], textureDictionary["ship"], Color.White, 0);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidablePrimitiveObject(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.CollidableDecorator,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager);

            objectManager.Add(collidablePrimitiveObject);

            transform3D = new Transform3D(new Vector3(-250, 0, 0), Vector3.Zero, new Vector3(1, 1000, 1000), Vector3.UnitZ, Vector3.UnitY);

            //add to the archetype dictionary
            //objectManager.Add(collidablePrimitiveObject);

            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured], textureDictionary["ship"], Color.White, 0);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new BoxCollisionPrimitive(transform3D);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidablePrimitiveObject(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.CollidableDecorator,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager);

            objectManager.Add(collidablePrimitiveObject);

        }

        private void NewPickup()
        {
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            ICollisionPrimitive collisionPrimitive = null;
            CollidablePrimitiveObject collidablePrimitiveObject = null;
            PrimitiveType primitiveType;
            int primitiveCount;
            int rand = MathUtility.RandomInRange(-200, 200);

            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["spaceback"], Color.White, 1);

            transform3D = new Transform3D(new Vector3(rand, 0, -1100), Vector3.Zero, new Vector3(20, 20, 20), Vector3.UnitZ, Vector3.UnitY);

            vertexData = new VertexData<VertexPositionNormalTexture>(VertexFactory.GetVerticesPositionNormalTexturedDiamond(out primitiveType, out primitiveCount), primitiveType, primitiveCount);

            collisionPrimitive = new SphereCollisionPrimitive(transform3D, 20);

            collidablePrimitiveObject = new CollidableEnemyObject(
                GameConstants.Primitive_LitTexturedDiamond,
                ActorType.CollidablePickup,
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive,
                objectManager,
                new PickupParameters("health", -1)
                );

            collidablePrimitiveObject.ControllerList.Add(new RotationController("rot controller1", ControllerType.RotationOverTime, 0.7f, new Vector3(0, 1, 0)));

            Transform3DCurve curveA = new Transform3DCurve(CurveLoopType.Linear); //experiment with other CurveLoopTypes
            curveA.Add(transform3D.Translation, -Vector3.UnitZ, Vector3.UnitY, 0); //start

            Vector3 by = new Vector3(rand, 0, transform3D.Translation.Z + 1200);

            curveA.Add(by, new Vector3(1, 0, -1), Vector3.UnitY, (int)currentSpeed); //start position

            collidablePrimitiveObject.ControllerList.Add(new Curve3DController("path", ControllerType.Curve, curveA));

            collidablePrimitiveObject.ControllerList.Add(
                new RotationController("rot controller1", ControllerType.RotationOverTime,
                0.2f, new Vector3(0, 1, 0)));

            objectManager.Add(collidablePrimitiveObject);
        }

        private void InitCollidablePickups()
        {
            Transform3D transform3D = null;
            //Transform3D change = null;
            EffectParameters effectParameters = null;
            IVertexData vertexData = null;
            ICollisionPrimitive collisionPrimitive = null;
            CollidablePrimitiveObject collidablePrimitiveObject = null;
            PrimitiveType primitiveType;
            int primitiveCount;
            /************************* Sphere Collision Primitive  *************************/

            int rand = MathUtility.RandomInRange(-100, 100);

            transform3D = new Transform3D(new Vector3(-20, 4, 0), Vector3.Zero, new Vector3(4, 12, 4), Vector3.UnitZ, Vector3.UnitY);

            //a unique effectparameters instance for each box in case we want different color, texture, alpha
            effectParameters = new EffectParameters(effectDictionary[GameConstants.Effect_LitTextured],
                textureDictionary["spaceback"], Color.White, 1);

            //get the vertex data object
            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedCube(1,
                                  out primitiveType, out primitiveCount),
                                  primitiveType, primitiveCount);

            //make the collision primitive - changed slightly to no longer need transform
            collisionPrimitive = new SphereCollisionPrimitive(transform3D, 20);

            //make a collidable object and pass in the primitive
            collidablePrimitiveObject = new CollidablePrimitiveObject(
                GameConstants.Primitive_LitTexturedCube,
                ActorType.CollidablePickup,  //this is important as it will determine how we filter collisions in our collidable player CDCR code
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager);

            //add to the archetype dictionary
            //objectManager.Add(collidablePrimitiveObject);

            transform3D = new Transform3D(new Vector3(0, 0, -1100), Vector3.Zero, new Vector3(10, 10, 10), Vector3.UnitZ, Vector3.UnitY);

            vertexData = new VertexData<VertexPositionNormalTexture>(VertexFactory.GetVerticesPositionNormalTexturedDiamond(out primitiveType, out primitiveCount), primitiveType, primitiveCount);

            collisionPrimitive = new SphereCollisionPrimitive(transform3D, 10);

            collidablePrimitiveObject = new CollidablePickupObject(
                GameConstants.Primitive_LitTexturedDiamond,
                ActorType.CollidablePickup,
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, 
                objectManager,
                new PickupParameters("health", 10)
                );

            collidablePrimitiveObject.ControllerList.Add( new RotationController("rot controller1", ControllerType.RotationOverTime, 0.7f, new Vector3(0, 1, 0)));

            Transform3DCurve curveA = new Transform3DCurve(CurveLoopType.Linear); //experiment with other CurveLoopTypes
            curveA.Add(transform3D.Translation, -Vector3.UnitZ, Vector3.UnitY, 0); //start
            Vector3 by = new Vector3(rand,0,transform3D.Translation.Z+1200);

            curveA.Add(by, new Vector3(1, 0, -1), Vector3.UnitY, 10000); //start position

            //curveA.Add(new Vector3(0, 5, 50), -Vector3.UnitZ, Vector3.UnitY, 3000); //start position
            //curveA.Add(new Vector3(0, 5, 20), new Vector3(-1, 0, -1), Vector3.UnitY, 4000); //start position
            //curveA.Add(new Vector3(0, 5, 10), -Vector3.UnitZ, Vector3.UnitY, 6000); //start position



            collidablePrimitiveObject.ControllerList.Add(new Curve3DController("enemyPath", ControllerType.Curve, curveA));



            //objectManager.Add(collidablePrimitiveObject);





            //objectManager.Add(collidablePrimitiveObject);

            transform3D = new Transform3D(new Vector3(-20, 4, 0), Vector3.Zero, new Vector3(4, 12, 4), Vector3.UnitZ, Vector3.UnitY);

            vertexData = new VertexData<VertexPositionNormalTexture>(
                VertexFactory.GetVerticesPositionNormalTexturedDiamond(out primitiveType, out primitiveCount), primitiveType, primitiveCount);

            collisionPrimitive = new SphereCollisionPrimitive(transform3D, 10);

            collidablePrimitiveObject = new CollidablePrimitiveObject(
                GameConstants.Primitive_LitTexturedDiamond,
                ActorType.CollidableDecorator,
                StatusType.Drawn | StatusType.Update,
                transform3D,
                effectParameters,
                vertexData,
                collisionPrimitive, objectManager
                );

            collidablePrimitiveObject.ControllerList.Add(
                new RotationController("rot controller1", ControllerType.RotationOverTime,
               0.5f, new Vector3(0, 1, 0)));


            curveA = new Transform3DCurve(CurveLoopType.Oscillate); //experiment with other CurveLoopTypes
            curveA.Add(transform3D.Translation, -Vector3.UnitZ, Vector3.UnitY, 0); //start
            curveA.Add(new Vector3(0, 5, 80), new Vector3(1, 0, -1), Vector3.UnitY, 1000); //start position
            curveA.Add(new Vector3(0, 5, 50), -Vector3.UnitZ, Vector3.UnitY, 3000); //start position
            curveA.Add(new Vector3(0, 5, 20), new Vector3(-1, 0, -1), Vector3.UnitY, 4000); //start position
            curveA.Add(new Vector3(0, 5, 10), -Vector3.UnitZ, Vector3.UnitY, 6000); //start position



            collidablePrimitiveObject.ControllerList.Add(new Curve3DController("enemyPath", ControllerType.Curve, curveA));



            //objectManager.Add(collidablePrimitiveObject);




        }


        #endregion NEW - 26.12.20

        /// <summary>
        /// Demos how we can clone an archetype and manually add to the object manager.
        /// </summary>
        private void InitDecorators()
        {
            //main backdrop
            PrimitiveObject drawnActor3D
                = archetypeDictionary[GameConstants.Primitive_LitTexturedCylinder].Clone() as PrimitiveObject;

            drawnActor3D.ActorType = ActorType.Decorator;
            drawnActor3D.StatusType = StatusType.Drawn | StatusType.Update;
            drawnActor3D.EffectParameters.Texture = textureDictionary["texturespace"];
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(0, 0, 90);
            drawnActor3D.Transform3D.Scale = 100 * new Vector3(2, 10, 2);
            drawnActor3D.Transform3D.Translation = new Vector3(500, -225, -450);
            drawnActor3D.EffectParameters.Alpha = 1f;

            drawnActor3D.ControllerList.Add(
                new RotationController("rot controller1", ControllerType.RotationOverTime,
               1, new Vector3(0, -1, 0)));

            objectManager.Add(drawnActor3D);

            drawnActor3D
                = archetypeDictionary[GameConstants.Primitive_LitTexturedDiamond].Clone() as PrimitiveObject;

            drawnActor3D.ActorType = ActorType.Decorator;
            drawnActor3D.StatusType = StatusType.Drawn | StatusType.Update;
            drawnActor3D.EffectParameters.Texture = textureDictionary["checkerboard"];
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(0, 0, 0);
            drawnActor3D.Transform3D.Scale = 20 * new Vector3(2, 2, 2);
            drawnActor3D.Transform3D.Translation = new Vector3(0, 0, 0);
            drawnActor3D.EffectParameters.Alpha = 1f;

            drawnActor3D.ControllerList.Add(
                new RotationController("rot controller1", ControllerType.RotationOverTime,
               0f, new Vector3(0, 0, 1)));

            //objectManager.Add(drawnActor3D);

        }

        private void InitHelpers()
        {
            //clone the archetype
            PrimitiveObject originHelper = archetypeDictionary[GameConstants.Primitive_WireframeOriginHelper].Clone() as PrimitiveObject;
            //add to the dictionary
            objectManager.Add(originHelper);
        }

        private void InitGround(float worldScale)
        {
            PrimitiveObject drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Ground;
            //drawnActor3D.EffectParameters.Texture = textureDictionary["grass1"];
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(-90, 0, 0);
            drawnActor3D.Transform3D.Scale = worldScale * Vector3.One;
            drawnActor3D.EffectParameters.Alpha = 0.1f;
            //objectManager.Add(drawnActor3D);
        }

        private void InitSkybox(float worldScale)
        {
            PrimitiveObject drawnActor3D = null;

            //back
            drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Sky;

            //  primitiveObject.StatusType = StatusType.Off; //Experiment of the effect of StatusType
            drawnActor3D.ID = "sky back";
            drawnActor3D.EffectParameters.Texture = textureDictionary["spaceempty"];
            drawnActor3D.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            drawnActor3D.Transform3D.Translation = new Vector3(0, 0, -worldScale / 2.0f);

            drawnActor3D.ControllerList.Add(
                new RotationController("rot controller1", ControllerType.RotationOverTime,
               0.04f, new Vector3(0, 0, 1)));

            

            objectManager.Add(drawnActor3D);


            //left
            drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Sky;
            drawnActor3D.ID = "left back";
            drawnActor3D.EffectParameters.Texture = textureDictionary["spaceempty"]; ;
            drawnActor3D.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(0, 45, 0);
            drawnActor3D.Transform3D.Translation = new Vector3(-worldScale / 2.0f, 0, 0);
            objectManager.Add(drawnActor3D);

            //right
            drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Sky;
            drawnActor3D.ID = "sky right";
            drawnActor3D.EffectParameters.Texture = textureDictionary["spaceempty"];
            drawnActor3D.Transform3D.Scale = new Vector3(worldScale, worldScale, 20);
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(0, -45, 0);
            drawnActor3D.Transform3D.Translation = new Vector3(worldScale / 2.0f, 0, 0);
            objectManager.Add(drawnActor3D);

            //top
            drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Sky;
            drawnActor3D.ID = "sky top";
            drawnActor3D.EffectParameters.Texture = textureDictionary["sky"];
            drawnActor3D.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(90, -90, 0);
            drawnActor3D.Transform3D.Translation = new Vector3(0, worldScale / 2.0f, 0);
            //objectManager.Add(drawnActor3D);

            //front
            drawnActor3D = archetypeDictionary[GameConstants.Primitive_UnlitTexturedQuad].Clone() as PrimitiveObject;
            drawnActor3D.ActorType = ActorType.Sky;
            drawnActor3D.ID = "sky front";
            drawnActor3D.EffectParameters.Texture = textureDictionary["front"];
            drawnActor3D.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            drawnActor3D.Transform3D.RotationInDegrees = new Vector3(0, 180, 0);
            drawnActor3D.Transform3D.Translation = new Vector3(0, 0, worldScale / 2.0f);
            //objectManager.Add(drawnActor3D);
        }

        private void GameOver(int score)
        {

            //gameover screen, reset or quit

        }

        #endregion Initialization - Vertices, Archetypes, Helpers, Drawn Content(e.g. Skybox)

        #region Load & Unload Game Assets

        protected override void LoadContent()
        {
        }

        protected override void UnloadContent()
        {
            //housekeeping - unload content
            textureDictionary.Dispose();
            modelDictionary.Dispose();
            fontDictionary.Dispose();
            modelDictionary.Dispose();
            soundManager.Dispose();

            base.UnloadContent();
        }

        #endregion Load & Unload Game Assets

        #region Update & Draw
        protected override void Update(GameTime gameTime)
        {
            if(currentSpeed == 1250)
            {
                if(currentSpeed == 750)
                {
                    score += currentLevel;
                    currentSpeed = BASESPEED - (score * 0.25f);
                }
                else
                {

                }
            }
            else
            {
                score += currentLevel;
                currentSpeed = BASESPEED - (score * currentLevel);
            }
            

            if(currentHealth <= 0)
            {
                GameOver(score);
            }

            if(mouseManager.IsLeftButtonClicked())
            {
                Shoot();
            }

            spawnTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(spawnTimer < 0)
            {
                NewPickup();

                spawnTimer = SPAWNTIMER;
            }

            if (keyboardManager.IsFirstKeyPress(Keys.N))
            {
                LevelLoader<PrimitiveObject> levelLoader = new LevelLoader<PrimitiveObject>(
                archetypeDictionary, textureDictionary);
                List<DrawnActor3D> actorList = null;
                actorList = levelLoader.Load(
                textureDictionary["level1_1"],
                                10,     //number of in-world x-units represented by 1 pixel in image
                                10,     //number of in-world z-units represented by 1 pixel in image
                                20,     //y-axis height offset
                                new Vector3(0, -200, -1500) //offset to move all new objects by
                                );
                int count = 1;

                //Transform3DCurve curveA = new Transform3DCurve(CurveLoopType.Oscillate); //experiment with other CurveLoopTypes

                foreach (DrawnActor3D actor3D in actorList)
                {
                    if (actor3D.ID.StartsWith('r'))
                    {
                        //Vector3 current = actor3D.Transform3D.Translation;

                        //curveA.Add(new Vector3(0, 0, 0), -Vector3.UnitZ, Vector3.UnitY, 0); //start
                        //curveA.Add(new Vector3(0, 10, 200), new Vector3(1, 0, -1), Vector3.UnitY, 10000); //start position
                        //curveA.Add(new Vector3(0, 20, 400), -Vector3.UnitZ, Vector3.UnitY, 30000); //start position
                        //curveA.Add(new Vector3(0, 50, 600), new Vector3(-1, 0, -1), Vector3.UnitY, 40000); //start position
                        //curveA.Add(new Vector3(0, 1000, 800), -Vector3.UnitZ, Vector3.UnitY, 60000); //start position

                        //RailParameters rail = new RailParameters("rail " + count, current + new Vector3(0, 200, 0), current + new Vector3(0, 200, 0));
                        //Curve3DController curve = new Curve3DController("c" + count, ControllerType.Rail, );

                        //actor3D.ControllerList.Add(new Curve3DController(GameConstants.Controllers_NonCollidableCurveMainArena, ControllerType.Curve, curveA));

                        actor3D.ControllerList.Add(
                                new Curve3DController(GameConstants.Controllers_NonCollidableCurveMainArena,
                                ControllerType.Curve,
                                transform3DCurveDictionary["headshake1"]));

                        //curveA.Clear();

                        count++;

                        actor3D.EffectParameters.DiffuseColor = Color.Green;
                    }
                    
                }

                objectManager.Add(actorList);
            }

            if (keyboardManager.IsFirstKeyPress(Keys.Escape))
            {
                Exit();
            }
            #region Demo
#if DEMO

            #region Object Manager

            #endregion Object Manager

            #region Sound Demos
            if (keyboardManager.IsFirstKeyPress(Keys.F1))
            {
                // soundManager.Play2D("smokealarm");
                AudioListener listener = new AudioListener();
                listener.Position = new Vector3(0, 5, 50);
                listener.Forward = -Vector3.UnitZ;
                listener.Up = Vector3.UnitY;

                AudioEmitter emitter = new AudioEmitter();
                emitter.DopplerScale = 1;
                emitter.Position = new Vector3(0, 5, 0);
                emitter.Forward = Vector3.UnitZ;
                emitter.Up = Vector3.UnitY;

                object[] parameters = { "hitbad", listener, emitter };
                EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay3D, parameters));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F2))
            {
                soundManager.Pause("smokealarm");

                object[] parameters = { "smokealarm" };
                EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPause, parameters));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F3))
            {
                soundManager.Stop("bg1");

                //or stop with an event
                //object[] parameters = { "smokealarm" };
                //EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                //    EventActionType.OnStop, parameters));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F4))
            {
                //soundManager.SetMasterVolume(0);
                soundManager.ChangeVolume("bg1",1f);
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F5))
            {
                soundManager.SetMasterVolume(0.5f);
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F6))
            {
                AudioListener listener = new AudioListener();
                listener.Position = new Vector3(0, 5, 50);
                listener.Forward = -Vector3.UnitZ;
                listener.Up = Vector3.UnitY;

                AudioEmitter emitter = new AudioEmitter();
                emitter.DopplerScale = 1;
                emitter.Position = new Vector3(0, 5, 0);
                emitter.Forward = Vector3.UnitZ;
                emitter.Up = Vector3.UnitY;

                object[] parameters = { "bg1", listener, emitter };
                EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay3D, parameters));
            }
            #endregion Sound Demos

            #region Menu & UI Demos
            if (keyboardManager.IsFirstKeyPress(Keys.F9))
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPause, null));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F10))
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPlay, null));
            }

            //if (keyboardManager.IsFirstKeyPress(Keys.Up))
            //{
            //    object[] parameters = { 1 }; //will increase the progress by 1 to its max of 10 (see InitUI)
            //    EventDispatcher.Publish(new EventData(EventCategoryType.UI, EventActionType.OnHealthDelta, parameters));
            //}
            //else if (keyboardManager.IsFirstKeyPress(Keys.Down))
            //{
            //    object[] parameters = { -1 }; //will decrease the progress by 1 to its min of 0 (see InitUI)
            //    EventDispatcher.Publish(new EventData(EventCategoryType.UI, EventActionType.OnHealthDelta, parameters));
            //}

            if (keyboardManager.IsFirstKeyPress(Keys.F5)) //game -> menu
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPlay, null));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F6)) //menu -> game
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPause, null));
            }
            #endregion Menu & UI Demos

            #region Camera
            if (keyboardManager.IsFirstKeyPress(Keys.C))
            {
                cameraManager.CycleActiveCamera();
                EventDispatcher.Publish(new EventData(EventCategoryType.Camera,
                    EventActionType.OnCameraCycle, null));
            }

            if (keyboardManager.IsFirstKeyPress(Keys.Space))
            {
                cameraManager.CycleActiveCamera();
                EventDispatcher.Publish(new EventData(EventCategoryType.Camera,
                    EventActionType.OnCameraSetActive, new object[] { 0 }));
            }
            #endregion Camera

#endif
            #endregion Demo

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }

        #endregion Update & Draw
    }
}