using GDLibrary.Actors;
using GDLibrary.Controllers;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace GDGame.Controllers
{
    public class UIHealthController : Controller
    {
        private int currentValue;
        private int maxValue;
        private int startValue;

        public int CurrentValue
        {
            get
            {
                return currentValue;
            }
            set
            {
                currentValue = ((value >= 0) && (value <= maxValue)) ? value : 0;
            }
        }

        public int MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                maxValue = (value >= 0) ? value : 0;
            }
        }

        public int StartValue
        {
            get
            {
                return startValue;
            }
            set
            {
                startValue = (value >= 0) ? value : 0;
            }
        }

        public UIHealthController(string id, ControllerType controllerType, int startValue, int maxValue)
            : base(id, controllerType)
        {
            StartValue = startValue;
            MaxValue = maxValue;
            CurrentValue = startValue;

            //listen for UI events to change the SourceRectangle
            EventDispatcher.Subscribe(EventCategoryType.UI, HandleEvents);
        }

        private void HandleEvents(EventData eventData)
        {
            if (eventData.EventActionType == EventActionType.OnHealthDelta)
            {
                currentValue += (int)eventData.Parameters[0];
            }
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            UITextObject drawnActor = actor as UITextObject;

            if (drawnActor != null)
            {
                UpdateHealth(drawnActor);
            }

            base.Update(gameTime, actor);
        }

        /// <summary>
        /// Uses the currentValue to set the source rectangle width of the parent UITextureObject
        /// </summary>
        /// <param name="drawnActor">Parent to which this controller is attached</param>
        private void UpdateHealth(UITextObject drawnActor)
        {
            if (currentValue <= 0)
            {
                object[] parameters1 = { "bg1" };
                EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters1));

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

                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPause, null));
                EventDispatcher.Publish(new EventData(EventCategoryType.Game, EventActionType.OnLose, null));
            }

            drawnActor.Text = "SHIELD HEALTH -[" + currentValue + "]- ";
        }

        //to do...Equals, GetHashCode, Clone
    }
}