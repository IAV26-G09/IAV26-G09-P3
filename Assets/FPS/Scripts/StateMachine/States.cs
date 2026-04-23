using UnityEngine;

[CreateAssetMenu(fileName = "States", menuName = "Scriptable Objects/States")]
public class States : ScriptableObject
{
    public enum STATE
    {
        /// <summary>Inactivo hasta que la red y el NavMesh estén listos.</summary>
        Idle,

        /// <summary>Comportamiento de ejemplo: deambular por el mapa eligiendo puntos aleatorios.</summary>
        Wandering,

        /// <summary>Podéis usar este estado cuando CurrentHealth = 0 o cuando queráis bloquear la IA por otra razón.</summary>
        Dead
    };

    public enum EVENT
    {
        Enter,  // al entrar a un estado
        Update, // estando en un estado
        Exit    // al salir de un estado
    };


}
