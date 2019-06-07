using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavTester : MonoBehaviour
{

    private NavMeshAgent _Agent;
    private Camera _Cam;

    private void Awake()
    {
        _Agent = GetComponent<NavMeshAgent>();
        _Cam = Camera.main;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            var ray = _Cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out var hit))
            {
                _Agent.SetDestination(hit.point);
            }
        }
    }

}
