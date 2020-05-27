using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine;


public class EnviromentException:System.Exception
{
    public EnviromentException(string message) : base(message)
    {
        
    }
}

public class Enviroment : MonoBehaviour
{

    public static Enviroment SingletonEviroment;
    [Header("General")]
    public GameObject terrain;
    

    [Header("Plants")]
    [SerializeField]
    protected int plants_amount;
    [SerializeField]
    private float plants_regrowth_factor;
    private float plants_regrowth_remaing_cooldown;
    private float plants_regrowth_cooldown = 30f;
    private Dictionary<string, GameObject> existing_plants;
    private Queue<string> disabled_plants;
    [SerializeField]
    protected GameObject plant_prefab;

    //Rabbits
    [Header("Creatures")]
    [SerializeField]
    private int creatures_inital_amount;
    [SerializeField]
    private GameObject creture_prefab;
    protected Dictionary<string, GameObject> creatures;
    protected Queue<string> disabled_creatures;


    private Mesh terrain_mesh;
    private List<Coords> terrain_tiles;
    private float y_terrain_size, x_terrain_size;

#region <Inicio>

    void Awake()
    {
        if(Enviroment.SingletonEviroment != null)
        {
            throw new EnviromentException("Enviroment instance already defined");
        }
        Enviroment.SingletonEviroment = this;
        this.existing_plants = new Dictionary<string, GameObject>();
        this.disabled_plants = new Queue<string>();
        this.creatures = new Dictionary<string, GameObject>();
        this.disabled_creatures = new Queue<string>();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.terrain_mesh = terrain.GetComponent<MeshFilter>().mesh;
        this.terrain_tiles = this.setTiles();
        this.__init__();
    }

    private void __init__()
    {
        this.y_terrain_size = this.terrain.transform.localScale.z * this.terrain_mesh.bounds.size.y;
        this.x_terrain_size = this.terrain.transform.localScale.x * this.terrain_mesh.bounds.size.x;
        this.createPlants(); 
        this.createCretures();
        this.plants_regrowth_remaing_cooldown = this.plants_regrowth_cooldown;
    }

    private List<Coords> setTiles()
    {
        List<Coords> terrain_coords = new List<Coords>();
        /*float x_mesh_size = this.terrain.transform.localScale.x * this.terrain_mesh.bounds.size.x,
               z_mesh_size = this.terrain.transform.localScale.z * this.terrain_mesh.bounds.size.z,
               x_increment = x_mesh_size * 0.028f,
               z_increment = z_mesh_size * 0.028f;
        */
        Coords new_coords;
        foreach(Vector3 v in this.terrain_mesh.vertices)
        {
            new_coords = new Coords(v.x * this.terrain.transform.localScale.x, v.y * this.terrain.transform.localScale.z);
            if((v.z * this.terrain.transform.localScale.y) != 0)
            {
                //Debug.Log($"Skipping: {new_coords}");
                continue;
            }
            terrain_coords.Add(new_coords);
        }
        return terrain_coords;
    }


#endregion </Inicio>

    private void Update() {
        //check if there are more then 1 creatures alive
        if(this.LivingBeensCount <= 1 && !AudioManager.SingeltonAudioManager.is_gameover)
        {
            AudioManager.SingeltonAudioManager.is_gameover = true;
            UIManager.SingletonUIManager.is_gameover = true;
        }
        else if(AudioManager.SingeltonAudioManager.is_gameover)
        {
            if(Input.anyKeyDown)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        //regrowing plants
        if(this.disabled_plants.Count > 0)
        {    
            if(this.plants_regrowth_remaing_cooldown > 0)
            {
                this.plants_regrowth_remaing_cooldown -= Time.deltaTime * this.plants_regrowth_factor;
            }
            else
            {
                this.regrowPlant();
                this.plants_regrowth_remaing_cooldown = this.plants_regrowth_cooldown;   
            }
        }
    }

#region <Utilitys>

    private Coords getRandomSorrouningPoint(Vector3 center)
    {
        Vector3 random_point = (Random.insideUnitSphere * 2f) + center;
        NavMeshHit navMeshHit;
        if(!NavMesh.SamplePosition(random_point, out navMeshHit, 1f, 1))
        {
            return getRandomSorrouningPoint(center);
        }
        else if(navMeshHit.position.y < 0 )
        {
            return getRandomSorrouningPoint(center);
        }
        return new Coords(navMeshHit.position);

    }

    private Vector3 getRandomTile()
    {
        Coords random_position = this.terrain_tiles[(int)Random.Range(0,this.terrain_tiles.Count-1)];
        NavMeshHit safe_random_position;
        NavMesh.SamplePosition(random_position, out safe_random_position, 1f, -1);
        return safe_random_position.position;
    }

    public void registerBirth(GameObject animal, string animal_id)
    {
        this.creatures[animal_id] = animal;
    }

    public void registerDeath(SPECIES species, string instance_id)
    {
        if(species == SPECIES.Plant)
        {
            // Debug.Log($"Coords: {coords}\n{this.existing_plants.ToString()}");
            this.disablePlant(instance_id);
        }
        else if(species == SPECIES.Bunny)
        {
            this.creatures[instance_id].SetActive(false);
            this.disabled_creatures.Enqueue(instance_id);
        }
    }


#endregion </Utilitys>

#region <Plants>

    private void disablePlant(string plant_id)
    {
        this.existing_plants[plant_id].SetActive(false);
        this.disabled_plants.Enqueue(plant_id);
    }

    private void createPlants()
    {
        GameObject plant;
        for(int h = 0; h <= this.plants_amount-1; h++)
        {
            plant = Instantiate(this.plant_prefab, this.getRandomTile(), this.transform.rotation);
            this.existing_plants[plant.GetInstanceID().ToString()] = plant;
            // Debug.Log($"existing plants: {plant.transform.position}");
        }   
    }

    private void regrowPlant()
    {
        //crecer una nueva planta
        string plant_id = this.disabled_plants.Dequeue();
        this.existing_plants[plant_id].transform.position = this.getRandomTile();
        this.existing_plants[plant_id].SetActive(true);
    }


#endregion </Plants>

#region <Bunnys>

    private Genes getOffspringGenes(Animal female)
    {
        //esto es horrible :,v
        Animal random_creature = this.getRandomCreature();
        return Genes.mixGenes(female.LastMate.Genoma, female.Genoma, random_creature.Genoma);
    }

    public void createLife(Animal female)
    {

        int offsprings_cant = Random.Range(1,5);
        GameObject offspring;
        for(int h = 0; h < offsprings_cant; h++)
        {
            if(this.disabled_creatures.Count > 0)
            {
                offspring = this.creatures[this.disabled_creatures.Dequeue()];
                offspring.transform.position = this.getRandomSorrouningPoint(female.Location);
                offspring.SetActive(true);
            }
            else
            {
                offspring = Instantiate(this.creture_prefab, this.getRandomSorrouningPoint(female.Location), female.transform.rotation);
            }
            offspring.GetComponent<Animal>().reset(this.getOffspringGenes(female));
        }
    }

    private void createCretures()
    {
        GameObject been;
        for(int h =0; h <= this.creatures_inital_amount-1; h++)
        {
            been = Instantiate(this.creture_prefab, this.getRandomTile(), this.creture_prefab.transform.rotation);
        }
    }

#endregion </Bunnys>

#region <InfoAPIs>

    public Animal getRandomCreature()
    {
        Animal random_creature = System.Linq.Enumerable.ToList(this.creatures.Values)[Random.Range(0, this.creatures.Count)].GetComponent<Animal>();
        if(!random_creature.gameObject.activeSelf && this.disabled_creatures.Count != this.creatures.Count)
        {
            return this.getRandomCreature();
        }
        return random_creature;
    }

    public int LivingBeensCount
    {
        get {
            return this.creatures.Count - this.disabled_creatures.Count;
        }
    }

    public Vector3 getCreturesCenterPoint()
    {
        Bounds bounds;
        Vector3 first_target = this.getRandomCreature().transform.position;
        //used by the camera in full screen mode
        if(this.LivingBeensCount <= 0)
        {
            return Vector3.zero;
        }
        else if(this.LivingBeensCount == 1)
        {
            return this.getRandomCreature().transform.position;
        }
        bounds = new Bounds(first_target, Vector3.zero);
        foreach(string creature_id in this.creatures.Keys)
        {
            if(!this.creatures[creature_id].activeSelf)
            {
                continue;
            }
            bounds.Encapsulate(this.creatures[creature_id].transform.position);
        }
        return bounds.center;
    }

    public float getCreturesGreatestDistance()
    {
        Bounds bounds = new Bounds();
        foreach(GameObject creature in this.creatures.Values)
        {
            if(!creature.activeSelf)
            {
                continue;
            }
            bounds.Encapsulate(creature.transform.position);
        }
        return bounds.size.x;
    }

#endregion </InfoAPIs>

}
