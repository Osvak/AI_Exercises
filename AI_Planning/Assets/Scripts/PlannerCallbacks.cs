using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Generated.Semantic.Traits;

public class PlannerCallbacks : MonoBehaviour
{
    Moves moves;
    UnityEngine.AI.NavMeshAgent agent;
    Robber trt;

    public GameObject treasure;  // Uso GameObjects públicos ya que me petaba el codigo en la linea 66. Al comentarlo con compañeros, Mario me comentó que la estructura era correcta,
    public GameObject cop;       // pero que por algún motivo Unity pasaba el treasure como null. La solución que me propuso fué, pasarle las variables al principio,
                                 // de manera pública. Al entenderlo, he decidido usar este approach, ya que sino, el ejercicio crashearía al momento en el que el robber cambia al Seek.
    void Start()
    {
        moves = this.GetComponent<Moves>();
        agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        trt = this.GetComponent<Robber>();
    }

    public void Steal()
    {
        Debug.Log("Steal");
        treasure.GetComponent<Renderer>().enabled = false;
    }

    public IEnumerator Seek()
    {
        Debug.Log("Approach");
        agent.SetDestination(treasure.transform.position);
        while ((Vector3.Distance(treasure.transform.position, transform.position) > 2f) &&
               (Vector3.Distance(treasure.transform.position, cop.transform.position) > 10f))
            yield return null;
        if (Vector3.Distance(treasure.transform.position, cop.transform.position) < 10f)
        {
            trt.CopAway = false;
        }
        else
        {
            trt.Ready2Steal = true;
        }
    }

    public IEnumerator Wander()
    {
        Debug.Log("Wander");
        while (Vector3.Distance(treasure.transform.position, cop.transform.position) < 10f)
        {
            moves.Wander();
            yield return null;
        }
    }
}
