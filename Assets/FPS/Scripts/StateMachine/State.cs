using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/*
 * Se trata como un arbol:
 *
 *            STATEMACHINE
 *             /        \
 *         paseo        emergencia
 *         / 
 *     idle
 *
 * (ejemplo de arbol como el que esta ahora mismo hardcodeado en fsm)
 */

namespace HSM
{
    public abstract class State : ScriptableObject // nodo en la maquina de estados
    {
        [SerializeField]
        public State Parent; // referencia al nodo padre para poder volver hacia atras sobre el arbol

        private State ActiveChild; // nodo hijo activo debajo de este nodo (si lo hubiese)

        [SerializeField]
        protected string stateName; 

        [SerializeField]
        private State _initialState = null;

        [SerializeField] 
        private List<State> Transitions;

        protected virtual State GetInitialState() => _initialState; // con que estado hijo empezar cuando se entre a este estado (si es nulo soy hoja)
        protected virtual State GetTransition(BotGameplayActions a) => null; // si quiero transicionar devuelve el estado al que quiero ir (si es nulo me quedo)
        // Para buscar en la lista de transiciones, ej:
        // State e = Transitions.Find(x => x.stateName.Contains("LootState"));

        // Metodos basicos de un estado
        protected virtual void OnEnter(BotGameplayActions a) {}
        protected virtual void OnExit(BotGameplayActions a) {}
        protected virtual void OnUpdate(StateMachine m, float deltaTime) {}

        // metodos internos
        internal void Enter(BotGameplayActions a)
        {
            if (Parent != null)
            {
                Parent.ActiveChild = this; // si tengo un padre yo soy su hijo
            }

            OnEnter(a); // entras a este estado
            // despues de llamar al metodo basico de entrada entro recursivamente en mi primer hijo (si tengo)
            // el OnEnter del padre SIEMPRE se llama antes de el del hijo
            State init = GetInitialState();
            if (init != null) init.Enter(a);
        }

        internal void Exit(BotGameplayActions a)
        {
            // se sale en orden inverso al que se entra
            // el OnExit del hijo SIEMPRE se llama antes de el del padre
            if (ActiveChild != null) ActiveChild.Exit(a);
            ActiveChild = null;
            OnExit(a);
        }

        internal void Logic(StateMachine m, float deltaTime)
        {
            State to = GetTransition(m.Owner.Actions); // ver si quiero ir a otro estados

            if (to != null)
            {
                m.Transitions.RequestTransition(this, to); // triggerea la transicion
            }

            // si no hemos transicionado y tenemos un hijo recurre en el update
            else if (ActiveChild != null)
            {
                ActiveChild.Logic(m, deltaTime);
            }

            OnUpdate(m, deltaTime); // llama al metodo basico de este estado
        }

        public State Leaf() // busca el nodo activo mas profundo en un arbol, la hoja del camino en el arbol que estamos siguiendo
        {
            State state = this;

            while (state.ActiveChild != null) state = state.ActiveChild;
            return state;
        }

        public IEnumerable<State> PathToRoot() // devuelve el camino desde este estado a la raiz del arbol
        {
            for (State s = this; s != null; s = s.Parent) yield return s;
        }
    }
}
