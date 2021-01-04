using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Factories;
using GDLibrary.GameComponents;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary.Utilities
{
    /// <summary>
    /// Renders the collision skins of any ICollisionPrimitives used in the I-CA project.
    /// </summary>
    public class PrimitiveDebugDrawer : PausableDrawableGameComponent
    {
        private ObjectManager objectManager;
        private CameraManager<Camera3D> cameraManager;
        private bool bShowCDCRSurfaces;
        private bool bShowZones;
        private BasicEffect wireframeEffect;

        //temp vars
        private IVertexData vertexData = null;
        private SphereCollisionPrimitive coll;
        private Matrix world;

        public PrimitiveDebugDrawer(Game game, StatusType statusType,
            ObjectManager objectManager, CameraManager<Camera3D> cameraManager,
            bool bShowCDCRSurfaces, bool bShowZones)
            : base(game, statusType)
        {
            this.objectManager = objectManager;
            this.cameraManager = cameraManager;
            this.bShowCDCRSurfaces = bShowCDCRSurfaces;
            this.bShowZones = bShowZones;
        }

        public override void Initialize()
        {
            //used to draw bounding volumes
            wireframeEffect = new BasicEffect(Game.GraphicsDevice);
            wireframeEffect.VertexColorEnabled = true;
            base.Initialize();
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            //set so we dont see the bounding volume through the object is encloses - disable to see result
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (IActor actor in objectManager.OpaqueList)
            {
                DrawSurfaceOrZonePrimitive(gameTime, actor);
            }
            foreach (IActor actor in objectManager.TransparentList)
            {
                DrawSurfaceOrZonePrimitive(gameTime, actor);
            }
        }

        private void DrawSurfaceOrZonePrimitive(GameTime gameTime, IActor actor)
        {
            if (actor is CollidablePrimitiveObject && bShowCDCRSurfaces)
            {
                DrawBoundingPrimitive(gameTime, (actor as CollidablePrimitiveObject).CollisionPrimitive, Color.White); //collidable object volumes are White
            }
            else if (actor is CollidableZoneObject && bShowZones)
            {
                DrawBoundingPrimitive(gameTime, (actor as CollidableZoneObject).CollisionPrimitive, Color.Red);        //collidable zone volumes are red
            }
        }

        private void DrawBoundingPrimitive(GameTime gameTime, ICollisionPrimitive collisionPrimitive, Color color)
        {
            if (collisionPrimitive is SphereCollisionPrimitive)
            {
                int primitiveCount = 0;
                vertexData = new VertexData<VertexPositionColor>(VertexFactory.GetSphereVertices(1, 10, out primitiveCount),
                                PrimitiveType.LineStrip, primitiveCount);

                coll = collisionPrimitive as SphereCollisionPrimitive;
                world = Matrix.Identity * Matrix.CreateScale(coll.BoundingSphere.Radius) * Matrix.CreateTranslation(coll.BoundingSphere.Center);
                wireframeEffect.World = world;
                wireframeEffect.View = cameraManager.ActiveCamera.View;
                wireframeEffect.Projection = cameraManager.ActiveCamera.Projection;
                wireframeEffect.DiffuseColor = Color.White.ToVector3();
                wireframeEffect.CurrentTechnique.Passes[0].Apply();
                vertexData.Draw(gameTime, wireframeEffect);
            }
            else
            {
                BoxCollisionPrimitive coll = collisionPrimitive as BoxCollisionPrimitive;
                BoundingBoxBuffers buffers = BoundingBoxDrawer.CreateBoundingBoxBuffers(coll.BoundingBox, GraphicsDevice);
                BoundingBoxDrawer.DrawBoundingBox(buffers, wireframeEffect, GraphicsDevice,
                    cameraManager.ActiveCamera);
            }
        }
    }
}