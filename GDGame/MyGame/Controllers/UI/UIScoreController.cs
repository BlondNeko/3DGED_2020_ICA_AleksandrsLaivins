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
                currentValue = value;
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
                startValue = currentValue;
            }
        }

        public UIScoreController(string id, ControllerType controllerType, int startValue)
            : base(id, controllerType)
        {
            StartValue = startValue;
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
            else if(eventData.EventActionType == EventActionType.OnWin)
            {

            }
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            UITextObject drawnActor = actor as UITextObject;

            if (drawnActor != null)
            {
                UpdateScore(drawnActor);
            }

            base.Update(gameTime, actor);
        }

        /// <summary>
        /// Uses the currentValue to set the source rectangle width of the parent UITextureObject
        /// </summary>
        /// <param name="drawnActor">Parent to which this controller is attached</param>
        private void UpdateScore(UITextObject drawnActor)
        {
            drawnActor.Text = "SCORE -[" + currentValue + "]- ";
        }

        private void CenterText(UITextObject drawnActor)
        {
            //drawnActor.
        }

        //to do...Equals, GetHashCode, Clone
    }
}