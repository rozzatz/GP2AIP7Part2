using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - this.transform.position;
        agent.SetDestination(this.transform.position - fleeVector);
    }
    void pursue()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;

        float relativeHeading = Vector3.Angle(this.transform.forward, this.transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDir));

        if(toTarget > 90 && relativeHeading < 20 && target.GetComponent<Drive>().currentSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        float lookAhead = targetDir.magnitude/(agent.speed + target.GetComponent<Drive>().currentSpeed);

        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    void Evade()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;
        float lookAhead = targetDir.magnitude / (agent.speed + target.GetComponent<Drive>().currentSpeed);

        Flee(target.transform.position + target.transform.forward * lookAhead);

    }
    Vector3 wanderTarget = Vector3.zero;
    void Wander()
    {
       float WanderRadius = 10f;
        float WanderDistance = 20f;
        float WanderJitter = 1f;

        wanderTarget += new Vector3(Random.Range(-1.0f,1.0f) * WanderJitter,
                                                               0,
                                                               Random.Range(-1.0f,1.0f) * WanderJitter);
        wanderTarget.Normalize();
        wanderTarget *= WanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, WanderDistance);
        Vector3 TargetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);

        Seek(TargetWorld);
    }

    void Hide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        for (int i = 0; i <  World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 HidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 5;

            if(Vector3.Distance(this.transform.position, HidePos) < dist)
            {
                chosenSpot = HidePos;
                dist = Vector3.Distance(this.transform.position, HidePos);
            }
        }

        Seek(chosenSpot);
    }

    void CleverHide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 ChosenDir = Vector3.zero;
        GameObject chosenGO = World.Instance.GetHidingSpots()[0];

        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 HidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 5;

            if (Vector3.Distance(this.transform.position, HidePos) < dist)
            {
                chosenSpot = HidePos;
                ChosenDir = hideDir;
                chosenGO = World.Instance.GetHidingSpots()[i];
                dist = Vector3.Distance(this.transform.position, HidePos);
            }
        }
        Collider hideCol = chosenGO.GetComponent<Collider>();
        Ray backRay = new Ray(chosenSpot, -ChosenDir.normalized);
        RaycastHit info;
        float distance = 100.0f;
        hideCol.Raycast(backRay, out info, distance );

        Seek(info.point + ChosenDir.normalized * 5);
    }
    
    bool CanSeeTarget()
    {
        RaycastHit raycastInfo;
        Vector3 rayToTarget = target.transform.position - this.transform.position;
        if (Physics.Raycast(this.transform.position, rayToTarget, out raycastInfo))
        {
            if (raycastInfo.transform.gameObject.tag == "cop")
                return true;
        }
        return false;
    }
    // Update is called once per frame
    void Update()
    {
       if(CanSeeTarget())
            CleverHide();
        
    }
}
