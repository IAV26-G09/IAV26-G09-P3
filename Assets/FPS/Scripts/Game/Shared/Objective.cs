using System;
using UnityEngine;

namespace Unity.FPS.Game
{
    public abstract class Objective : MonoBehaviour
    {
        // Global switch: si se desactiva, no se crean/asignan objetivos ni se muestran en UI.
        // Para este proyecto queremos partidas infinitas, así que lo dejamos apagado por defecto.
        public static bool ObjectivesEnabled = false;

        [Tooltip("Name of the objective that will be shown on screen")]
        public string Title;

        [Tooltip("Short text explaining the objective that will be shown on screen")]
        public string Description;

        [Tooltip("Whether the objective is required to win or not")]
        public bool IsOptional;

        [Tooltip("Delay before the objective becomes visible")]
        public float DelayVisible;

        public bool IsCompleted { get; private set; }
        public bool IsBlocking() => !(IsOptional || IsCompleted);

        public static event Action<Objective> OnObjectiveCreated;
        public static event Action<Objective> OnObjectiveCompleted;

        protected virtual void Start()
        {
            if (!ObjectivesEnabled)
                return;

            OnObjectiveCreated?.Invoke(this);

            DisplayMessageEvent displayMessage = Events.DisplayMessageEvent;
            displayMessage.Message = Title;
            displayMessage.DelayBeforeDisplay = 0.0f;
            EventManager.Broadcast(displayMessage);
        }

        public void UpdateObjective(string descriptionText, string counterText, string notificationText)
        {
            if (!ObjectivesEnabled)
                return;

            ObjectiveUpdateEvent evt = Events.ObjectiveUpdateEvent;
            evt.Objective = this;
            evt.DescriptionText = descriptionText;
            evt.CounterText = counterText;
            evt.NotificationText = notificationText;
            evt.IsComplete = IsCompleted;
            EventManager.Broadcast(evt);
        }

        public void CompleteObjective(string descriptionText, string counterText, string notificationText)
        {
            if (!ObjectivesEnabled)
                return;

            IsCompleted = true;

            ObjectiveUpdateEvent evt = Events.ObjectiveUpdateEvent;
            evt.Objective = this;
            evt.DescriptionText = descriptionText;
            evt.CounterText = counterText;
            evt.NotificationText = notificationText;
            evt.IsComplete = IsCompleted;
            EventManager.Broadcast(evt);

            OnObjectiveCompleted?.Invoke(this);
        }
    }
}