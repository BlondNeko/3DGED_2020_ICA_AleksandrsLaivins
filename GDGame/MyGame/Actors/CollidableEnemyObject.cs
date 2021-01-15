using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary.MyGame
{
    /// <summary>
    /// Moveable, collidable player using keyboard and checks for collisions
    /// </summary>
    public class CollidableEnemyObject : CollidablePrimitiveObject
    {
        #region Fields
        //private bool damageDone = false;
        private PickupParameters pickupParameters;
        #endregion Fields

        public CollidableEnemyObject(string id, ActorType actorType,
            StatusType statusType, Transform3D transform,
            EffectParameters effectParameters,
            IVertexData vertexData,
             ICollisionPrimitive collisionPrimitive,
             ObjectManager objectManager, PickupParameters pickupParameters)
            : base(id, actorType, statusType, transform, effectParameters, vertexData, collisionPrimitive, objectManager)
        {
            this.pickupParameters = pickupParameters;
        }

        public override void Update(GameTime gameTime)
        {
            //have we collided with something?
            Collidee = CheckAllCollisions(gameTime);

            //how do we respond to this collidee e.g. pickup?
            HandleCollisionResponse(Collidee);

            //if no collision then move - see how we set this.Collidee to null in HandleCollisionResponse()
            //below when we hit against a zone
            //if (Collidee == null)
            //{
            //    ApplyInput(gameTime);
            //}

            //reset translate and rotate and update primitive
            base.Update(gameTime);
        }

        /********************************************************************************************/

        //this is where you write the application specific CDCR response for your game
        protected override void HandleCollisionResponse(Actor collidee)
        {
            if (collidee is CollidableZoneObject)
            {
                CollidableZoneObject simpleZoneObject = collidee as CollidableZoneObject;

                //do something based on the zone type - see Main::InitializeCollidableZones() for ID
                if (simpleZoneObject.ID.Equals("kill"))
                {
                    //publish an event e.g sound, health progress
                    //damageDone = true;
                    AudioListener listener = new AudioListener();
                    listener.Position = new Vector3(0, 5, 50);
                    listener.Forward = -Vector3.UnitZ;
                    listener.Up = Vector3.UnitY;

                    AudioEmitter emitter = new AudioEmitter();
                    emitter.DopplerScale = 1;
                    emitter.Position = new Vector3(0, 5, 0);
                    emitter.Forward = Vector3.UnitZ;
                    emitter.Up = Vector3.UnitY;

                    object[] parametersS = { "hitbad", listener, emitter };
                    EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                        EventActionType.OnPlay3D, parametersS));

                    object[] parameters = { -1 };
                    EventDispatcher.Publish(new EventData(EventCategoryType.UI, EventActionType.OnHealthDelta, parameters));
                }

                //IMPORTANT - setting this to null means that the ApplyInput() method will get called and the player can move through the zone.
                Collidee = null;
            }
            //if (collidee is CollidablePrimitiveObject)
            //{
            //    //the boxes on the left that we loaded from level loader
            //    if (collidee.ActorType == ActorType.CollidablePlayer)
            //    {
            //        //remove the object
            //        //damageDone = true;

            //    }
            //    ////else if(collidee.ActorType == ActorType.CollidablePickupDamage)
            //    ////{

            //    ////}
            //    //the boxes on the right that move up and down
            //    //else if (collidee.ActorType == ActorType.CollidableDecorator)
            //    //{
            //    //    (collidee as DrawnActor3D).EffectParameters.DiffuseColor = Color.Blue;
            //    //}
            //}
        }

        public new object Clone()
        {
            return new CollidablePickupObject("clone - " + ID, //deep
             ActorType, //deep
             StatusType, //deep
             Transform3D.Clone() as Transform3D, //deep
             EffectParameters.Clone() as EffectParameters, //deep
             this.IVertexData, //shallow - its ok if objects refer to the same vertices
             CollisionPrimitive.Clone() as ICollisionPrimitive, //deep
             ObjectManager, //shallow - reference
             pickupParameters.Clone() as PickupParameters); //deep
        }

    }
}