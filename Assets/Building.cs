using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Building : MonoBehaviour
{
    public int nb_etages;
    public int type;
    public List<Habitant> occupants;
    public List<Habitant> people_inside;
    private GameObject world_obj;
    private Vector3 position;
    private Quaternion angle;

    public void init(int type, GameObject obj, int nb_etages)
    {
        this.type = type;
        this.nb_etages = nb_etages;
        world_obj = obj;
        if (type == 1)
        {
            world_obj.transform.GetChild(0).gameObject.transform.localScale = new Vector3(4, 2 * nb_etages, 4);
            world_obj.transform.GetChild(0).gameObject.transform.localPosition = new Vector3(0, 2 * nb_etages, 3f);
        }
        if(type == 2)
        {
            world_obj.transform.GetChild(0).gameObject.transform.localScale = new Vector3(4, nb_etages, 4);
            world_obj.transform.GetChild(0).gameObject.transform.localPosition = new Vector3(0, 2 * nb_etages, 3f);
        }
        world_obj.transform.GetChild(0).gameObject.transform.localPosition = new Vector3(0, nb_etages, 3f);
        position = world_obj.transform.GetChild(0).gameObject.transform.position;
        occupants = new List<Habitant>();
        people_inside = new List<Habitant>();
    }

    public void DisactiveObject()
    {
        world_obj.SetActive(false);
    }

    public GameObject getGameobject()
    {
        return world_obj;
    }

    public Vector3 getPositions()
    {
        return position;
    }

    public static bool isFull(Building building)
    {
        if(building.type == 2)
        {
            return building.occupants.Count >= 4 * building.nb_etages;
        }
        else
        {
            return building.occupants.Count >= 10 * building.nb_etages;
        }
    }

    public bool addNewOccupants(Habitant habitant)
    {
        if (!isFull(this))
        {
            occupants.Add(habitant);
            return true;
        }
        return false;
    }

    public void enter(Habitant habitant)
    {
        habitant.agent.isStopped = true;
        people_inside.Add(habitant);
        habitant.inside = this;
        Renderer rend = habitant.GetComponent<Renderer>(); 
        rend.enabled = false;
        Collider collider = habitant.GetComponent<Collider>();
        collider.enabled = false;

    }

    public void exit(Habitant habitant)
    {
        if (!people_inside.Remove(habitant))
        {
            return;
        }
        Renderer rend = habitant.GetComponent<Renderer>();
        rend.enabled = true;
        Collider collider = habitant.GetComponent<Collider>();
        collider.enabled = true;
        habitant.inside = null;
        habitant.agent.isStopped = false;
    }
}
