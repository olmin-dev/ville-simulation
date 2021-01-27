using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Habitant : MonoBehaviour
{
    public Building house;
    public Building work;
    public Vector3 HouseSpawn;
    public Vector3 WorkSpawn;
    public NavMeshAgent agent;
    public Building inside;
    public int index;
    public float dist;
    public int type;

    public void init(Building house,Building work,NavMeshAgent agent, Vector3 HouseSpawn, Vector3 WorkSpawn)
    {
        this.type = 0;
        this.WorkSpawn = WorkSpawn;
        this.house = house;
        this.agent = agent;
        this.work = work;
        this.HouseSpawn = HouseSpawn;
        agent.SetDestination(WorkSpawn);
        agent.isStopped = false;
    }

    // Update is called once per frame
    void Update()
        {
        NavMeshHit hit;
        NavMesh.SamplePosition(agent.transform.position, out hit, 1.0f, NavMesh.AllAreas);

        DayNightCycle day = GameObject.Find("Plane").GetComponent(typeof(DayNightCycle)) as DayNightCycle;
        if (day.currentTimeOfDay > 0.7f || day.currentTimeOfDay < 0.2f && !inside == house)
        {
            if (inside == work)
            {
                work.exit(this);
            }
            agent.destination = HouseSpawn;
        }
        if (day.currentTimeOfDay > 0.2f && day.currentTimeOfDay < 0.75f && !inside == work)
        {
            if (inside == house)
            {
                house.exit(this);
            }
            agent.destination = WorkSpawn;
        }
        dist = Vector3.Distance(agent.destination, agent.gameObject.transform.position);
        if (Vector3.Distance(agent.destination, agent.gameObject.transform.position) < 0.4 && day.currentTimeOfDay > 0.75f || day.currentTimeOfDay < 0.2f && inside == null)
        { 
            house.enter(this);
        }
        if (Vector3.Distance(agent.destination, agent.gameObject.transform.position) < 0.4 && day.currentTimeOfDay > 0.2f && day.currentTimeOfDay < 0.75f && inside == null)
        {
            work.enter(this);
        }
    }
}
