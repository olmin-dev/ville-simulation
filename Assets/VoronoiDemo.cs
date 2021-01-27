using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using UnityEngine.AI;

public class VoronoiDemo : MonoBehaviour
{
    public const int NPOINTS = 400;
    public const int WIDTH = 200;
    public const int HEIGHT = 200;
	public float freqx = 0.02f, freqy = 0.018f, offsetx = 0.43f, offsety = 0.22f;
    public GameObject road;
    public GameObject highway;
    public GameObject work;
    public GameObject house;
    public int puissance;
    NavMeshSurface[] surfaces;
    public GameObject plane;
    public int planeSize = 150;

    private List<Vector2> m_points;
	private List<LineSegment> m_edges = null;
	private Texture2D tx;
    Habitant[] habitants;
    GameObject[] roads;
    List <Building> works = new List<Building>();
    List<Building> houses = new List<Building>();
    public NavMeshAgent agent;

    private float [,] createMap() 
    {
        float [,] map = new float[WIDTH, HEIGHT];
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
                map[i, j] = Mathf.PerlinNoise(freqx * i + offsetx, freqy * j + offsety);
        return map;
    }

    private int[] Search_density(float[,] map, float t)
    {
        float density = 0;
        int[] res = new int[] {0, 0};
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                density += Mathf.Pow(map[i, j], puissance);
                if (density > t)
                {
                    res[0] = i;
                    res[1] = j;
                    return res;
                }
            }
        }
        return res;
    }

    private int get_nb_etages(float density)
    {
        if (density < 0.14)
        {
            return Random.Range(10, 13);
        }
        if (density < 0.16)
        {
            return Random.Range(7, 10);
        }
        if (density < 0.18)
        {
            return Random.Range(4, 7);
        }
        if (density < 0.2)
        {
            return Random.Range(2, 4);
        }
        return Random.Range(2, 3);
    }

    void Start ()
	{
        float[,] map=createMap();
        float[,] m = new float[WIDTH,HEIGHT];
        Color[] pixels = createPixelMap(map);
		
		m_points = new List<Vector2> ();
        float total_density = 0;
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                total_density += Mathf.Pow(map[i, j],puissance);
                m[i, j] = Mathf.Pow(map[i, j], puissance);
            }
        }
        List<uint> colors = new List<uint> ();
        for (int k = 0; k < NPOINTS; k++) {
            colors.Add((uint)0);
            float t = Random.Range(0, total_density);
            int[] pos = Search_density(map, t);

            Vector2 vec = new Vector2(pos[0], pos[1]); 
			m_points.Add (vec);
        }

        Delaunay.Voronoi v = new Delaunay.Voronoi (m_points, colors, new Rect (0, 0, WIDTH, HEIGHT));
		m_edges = v.VoronoiDiagram ();

        roads = new GameObject[m_edges.Count];
		for (int i = 0; i < m_edges.Count; i++) {
			LineSegment seg = m_edges[i];				
			Vector2 left = (Vector2)seg.p0;
			Vector2 right = (Vector2)seg.p1;
            Vector2 segment = (right - left)/WIDTH*100*planeSize;
            Vector2 middle = Vector2.Lerp(left, right, 0.5f);
            GameObject way;
            int padding = 925;
            float angle = Vector2.SignedAngle(Vector2.right,right-left);
            if (((map[(int)(middle.y + padding) / 10, (int)(middle.x + padding) / 10] > 0.15 && map[(int)(middle.y + padding) / 10, (int)(middle.x + padding) / 10] < 0.16f)
                || (map[(int)(middle.y + padding) / 10, (int)(middle.x + padding) / 10] > 0.17 && map[(int)(middle.y + padding) / 10, (int)(middle.x + padding) / 10] < 0.18f))
                && Vector3.Distance(left, right) < 5)
            {
                way = Instantiate(highway, new Vector3(left.y / WIDTH * 10 * planeSize - (5 * planeSize), 0.15f, left.x / HEIGHT * 10 * planeSize - (5 * planeSize)), Quaternion.Euler(0, angle + 90, 0));
            }
            else
            {
                way = Instantiate(road, new Vector3(left.y / WIDTH * 10 * planeSize - (5 * planeSize), 0.01f, left.x / HEIGHT * 10 * planeSize - (5 * planeSize)), Quaternion.Euler(0, angle + 90, 0));
            }
            way.transform.localScale = new Vector3(segment.magnitude, 1, 1);
            roads[i] = way;                  
        }
        for (int i = 0; i < m_edges.Count; i++)
        {
            LineSegment seg = m_edges[i];
            Vector2 left = (Vector2)seg.p0;
            Vector2 right = (Vector2)seg.p1;
            Vector2 segment = (right - left) / WIDTH * 100 * planeSize;
            Vector2 middle = Vector2.Lerp(left, right, 0.5f);
            float angle = Vector2.SignedAngle(Vector2.right, right - left);
            Building building;
            int nb_etages;
            int padding = 925;
            if (map[(int)(middle.y + padding) / 10, (int)(middle.x + padding) / 10] < 0.18 && Vector3.Distance(left,right) > 1f)
            {
                nb_etages = get_nb_etages(map[(int)(middle.y + padding) / 10, (int)(middle.x + padding) / 10]);
                GameObject objBuilding = Instantiate(work, new Vector3((middle.y / WIDTH) * 10 * planeSize - (5 * planeSize), 0.1f, (middle.x / HEIGHT) * 10 * planeSize - (5 * planeSize)), Quaternion.Euler(0, angle + 90, 0));
                building = objBuilding.AddComponent(typeof(Building)) as Building;
                building.init(1, objBuilding, nb_etages);
                bool intersected_road = false;
                for (int j = 0; j < m_edges.Count; j++)
                {
                    if (building.getGameobject().transform.GetChild(0).GetComponentInChildren<Collider>().bounds.Intersects(roads[j].GetComponentInChildren<Collider>().bounds))
                    {
                        intersected_road = true;
                        building.DisactiveObject();
                    }
                }
                if (!intersected_road)
                {
                    works.Add(building);
                }
            }
            else if (Vector3.Distance(left, right) > 1f)
            {
                nb_etages = get_nb_etages(map[(int)(middle.y + padding) / 10, (int)(middle.x + padding) / 10]) - 1;
                GameObject objBuilding = Instantiate(house, new Vector3((middle.y / WIDTH) * 10 * planeSize - (5 * planeSize), 0.1f, (middle.x / HEIGHT) * 10 * planeSize - (5 * planeSize)), Quaternion.Euler(0, angle + 90, 0));
                building = objBuilding.AddComponent(typeof(Building)) as Building;
                building.init(2, objBuilding, nb_etages);
                bool intersected_road = false;
                for (int j = 0; j < m_edges.Count; j++)
                {
                    if (building.getGameobject().transform.GetChild(0).GetComponentInChildren<Collider>().bounds.Intersects(roads[j].GetComponentInChildren<Collider>().bounds))
                    {
                        intersected_road = true;
                        building.DisactiveObject();
                    }
                }
                if (!intersected_road)
                {
                    houses.Add(building);
                }
            }
            
        }
        plane.GetComponent<NavMeshSurface>().BuildNavMesh();

        /* Apply pixels to texture */
        tx = new Texture2D(WIDTH, HEIGHT);
		tx.SetPixels (pixels);

        //tx.Apply ();
        habitants = new Habitant[100];
        List<Building> emptyWorks = works;
        List<Building> emptyHouses = houses;
        
        emptyHouses.RemoveAll(Building.isFull);
        emptyWorks.RemoveAll(Building.isFull);
        for (int i = 0; i < 100; i++)
        {
            emptyHouses.RemoveAll(Building.isFull);
            emptyWorks.RemoveAll(Building.isFull);

            int NumHouse = Random.Range(0, emptyHouses.Count);
            int NumWork = Random.Range(0, emptyWorks.Count);

            GameObject[] roads = GameObject.FindGameObjectsWithTag("road");
            int closestRoadToHouse = 0;
            int closestRoadToWork = 0;
            float minDistHouse = float.MaxValue;
            float minDistWork = float.MaxValue;
            for (int j = 0; j < roads.Length; j++)
            {
                if (Vector3.Distance(roads[j].transform.position, emptyHouses[NumHouse].getPositions()) < minDistHouse)
                {
                    closestRoadToHouse = j;
                    minDistHouse = Vector3.Distance(roads[j].transform.position, emptyHouses[NumHouse].getPositions());
                }
                if (Vector3.Distance(roads[j].transform.position, emptyWorks[NumWork].getPositions()) < minDistWork)
                {
                    closestRoadToWork = j;
                    minDistWork = Vector3.Distance(roads[j].transform.position, emptyWorks[NumWork].getPositions());
                }
            }
            Vector3 startPos = roads[closestRoadToHouse].transform.position;
            startPos.y = 1.20f;
            NavMeshAgent habitant = NavMeshAgent.Instantiate(agent, startPos, Quaternion.identity);
            Habitant humain = habitant.gameObject.AddComponent(typeof(Habitant)) as Habitant;
            humain.init(emptyHouses[NumHouse], emptyWorks[NumWork], habitant, roads[closestRoadToHouse].transform.position, roads[closestRoadToWork].transform.position);
            habitants[i] = humain;
            emptyHouses[NumHouse].addNewOccupants(humain);
            emptyWorks[NumWork].addNewOccupants(humain);

        }
    }

    public void Update()
    {
    }

    /* Functions to create and draw on a pixel array */
    private Color[] createPixelMap(float[,] map)
    {
        Color[] pixels = new Color[WIDTH * HEIGHT];
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
            {
                pixels[i * HEIGHT + j] = Color.Lerp(Color.white, Color.black, map[i, j]);
            }
        return pixels;
    }
    private void DrawPoint (Color [] pixels, Vector2 p, Color c) {
		if (p.x<WIDTH&&p.x>=0&&p.y<HEIGHT&&p.y>=0) 
		    pixels[(int)p.x*HEIGHT+(int)p.y]=c;
	}
	// Bresenham line algorithm
	private void DrawLine(Color [] pixels, Vector2 p0, Vector2 p1, Color c) {
		int x0 = (int)p0.x;
		int y0 = (int)p0.y;
		int x1 = (int)p1.x;
		int y1 = (int)p1.y;

		int dx = Mathf.Abs(x1-x0);
		int dy = Mathf.Abs(y1-y0);
		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;
		int err = dx-dy;
		while (true) {
            if (x0>=0&&x0<WIDTH&&y0>=0&&y0<HEIGHT)
    			pixels[x0*HEIGHT+y0]=c;

			if (x0 == x1 && y0 == y1) break;
			int e2 = 2*err;
			if (e2 > -dy) {
				err -= dy;
				x0 += sx;
			}
			if (e2 < dx) {
				err += dx;
				y0 += sy;
			}
		}
	}
}