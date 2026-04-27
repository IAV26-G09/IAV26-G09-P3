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
        public StateMachine Machine; // referencia a la maquina de estados, para que un estado pueda "pedir" transicionar

        [SerializeField]
        public State Parent; // referencia al nodo padre para poder volver hacia atras sobre el arbol

        private State ActiveChild; // nodo hijo activo debajo de este nodo (si lo hubiese)

        [SerializeField]
        private State _initialState = null;

        public State(StateMachine machine, State parent = null) // constructora de esta clase
        {
            Machine = machine;
            Parent = parent;
        }

        protected virtual State GetInitialState() => _initialState; // con que estado hijo empezar cuando se entre a este estado (si es nulo soy hoja)
        protected virtual State GetTransition() => null; // si quiero transicionar devuelve el estado al que quiero ir (si es nulo me quedo)

        // Metodos basicos de un estado
        protected virtual void OnEnter() {}
        protected virtual void OnExit() {}
        protected virtual void OnUpdate(float deltaTime) {}

        protected BotGameplayActions Actions => Machine.Owner.Actions;

        // metodos internos
        internal void Enter()
        {
            if (Parent != null)
            {
                Parent.ActiveChild = this; // si tengo un padre yo soy su hijo
            }

            OnEnter(); // entras a este estado
            // despues de llamar al metodo basico de entrada entro recursivamente en mi primer hijo (si tengo)
            // el OnEnter del padre SIEMPRE se llama antes de el del hijo
            State init = GetInitialState();
            if (init != null) init.Enter();
        }

        internal void Exit()
        {
            // se sale en orden inverso al que se entra
            // el OnExit del hijo SIEMPRE se llama antes de el del padre
            if (ActiveChild != null) ActiveChild.Exit();
            ActiveChild = null;
            OnExit();
        }

        internal void Logic(float deltaTime)
        {
            State to = GetTransition(); // ver si quiero ir a otro estados

            if (to != null)
            {
                Machine.Transitions.RequestTransition(this, to); // triggerea la transicion
            }

            // si no hemos transicionado y tenemos un hijo recurre en el update
            else if (ActiveChild != null)
            {
                ActiveChild.Logic(deltaTime);
            }

            OnUpdate(deltaTime); // llama al metodo basico de este estado
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
