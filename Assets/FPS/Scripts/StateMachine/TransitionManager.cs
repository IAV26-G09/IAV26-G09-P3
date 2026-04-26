using System.Collections.Generic;
using UnityEngine;

namespace HSM
{
    public class TransitionManager // controla las transiciones entre nodos
    {
        public readonly StateMachine Machine; // referencia a la maquina de estados

        public TransitionManager(StateMachine machine)
        {
            Machine = machine;
        }

        // triggerea transicion desde estado from hasta estado to
        public void RequestTransition(State from, State to)
        {
            Machine.ChangeState(from, to);
        }

        // saca el estado padre comun mas cercano a dos nodos
        public State CommonFatherState(State a, State b)
        {
            var aFathers = new HashSet<State>(); // set de padres del estado a
            for (var s = a; s != null; s = s.Parent) aFathers.Add(s); // sacas todos los antecesores de a

            for (var s = b; s != null; s = s.Parent) // buscas en los antecesores de b el primer antecesor comun con a
            {
                if (aFathers.Contains(s)) return s;
            }

            return null; // si no hay ancestro comun
        }
    }
}