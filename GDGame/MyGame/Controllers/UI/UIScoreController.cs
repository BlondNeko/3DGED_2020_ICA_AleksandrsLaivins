﻿using GDLibrary.Actors;
using GDLibrary.Controllers;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Interfaces;
using Microsoft.Xna.Framework;

namespace GDGame.Controllers
{
    public class UIScoreController : Controller
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

        public UIScoreController(string id, ControllerType controllerType, int startValue, int maxValue)
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
            if (eventData.EventActionType == EventActionType.OnScoreDelta)
            {
                CurrentValue = currentValue + (int)eventData.Parameters[0];
                
            }
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            UITextObject drawnActor = actor as UITextObject;

            if (drawnActor != null)
            {
                UpdateSourceRectangle(drawnActor);
            }

            base.Update(gameTime, actor);
        }

        /// <summary>
        /// Uses the currentValue to set the source rectangle width of the parent UITextureObject
        /// </summary>
        /// <param name="drawnActor">Parent to which this controller is attached</param>
        private void UpdateSourceRectangle(UITextObject drawnActor)
        {
            drawnActor.Text = "SCORE -[" + currentValue + "]- ";
        }

        //to do...Equals, GetHashCode, Clone
    }
}