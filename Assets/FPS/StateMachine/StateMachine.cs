using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HierarchicalStateMachine
{
    public class StateMachine // gestiona el arbol de estados, lo contiene el monobehaviour FSM
    {
        public readonly State Root; // referencia al estado raiz del arbol (H* en el diagrama de Millington)
        public readonly TransitionManager Transitions;
        public FSM Owner;

        private bool started;

        public StateMachine(State root)
        {
            Root = root;
            Transitions = new TransitionManager(this); // crea el manager de transiciones
        }

        public void Start()
        {
            if (started) return;

            Debug.Log("START STATEMACHINE");

            started = true;
            Root.Enter(); // para entrar por primera vez
        }

        // pasar aqui delta time, se llamara a esto en el update de un monobehaviour
        public void Tick(float deltaTime)
        {
            if (!started) Start();
            InternalTick(deltaTime);
        }

        internal void InternalTick(float deltaTime) => Root.Update(deltaTime); // delega

        // ejecuta el cambio triggerado, sale de todos los estados hasta el ancestro comun y entra de vuelta hasta el estado to
        public void ChangeState(State from, State to)
        {
            if (from == to || from == null || to == null) return; // comprueba primero si hay que ejecutar el cambio

            State commonFather = Transitions.CommonFatherState(from, to);

            // <- sale de todos los estados desde from hasta el padre comun
            for (State s = from; s != commonFather; s = s.Parent) s.Exit();

            // -> entra en todos los estados desde el padre comun hasta to
            var stack = new Stack<State>();
            for (State s = to; s != commonFather; s = s.Parent) stack.Push(s);
            while (stack.Count > 0) stack.Pop().Enter();
        }
    }

    public class StateMachineBuilder // construye la maquina de estados
    {
        readonly State root;

        public StateMachineBuilder(State root)
        {
            this.root = root;
        }

        public StateMachine Build()
        {
            //Debug.Log("Building");

            var m = new StateMachine(root);
            Wire(root, m, new HashSet<State>());
            return m;
        }

        void Wire(State s, StateMachine m, HashSet<State> visited)
        {
            if (s == null) return;
            if (!visited.Add(s)) return; // el estado ya esta cableado

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var machineField = typeof(State).GetField("Machine", flags);
            if (machineField != null) machineField.SetValue(s, m);

            foreach (var fld in s.GetType().GetFields(flags))
            {
                if (!typeof(State).IsAssignableFrom(fld.FieldType)) continue; // solo campos de tipo state
                if (fld.Name == "Parent") continue;

                var child = (State)fld.GetValue(s);
                if (child == null) continue;
                if (!ReferenceEquals(child.Parent, s)) continue; // para comprobar que es nuestro hijo directo

                Wire(child, m, visited); // recurre al hijo
            }
        }
    } 
}
