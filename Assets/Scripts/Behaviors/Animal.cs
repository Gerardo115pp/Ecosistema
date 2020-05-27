using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using System;

public class Animal : LivingBeen
{

    public SPECIES diet;
    public bool is_pregnet = false;

    private float best_mate_appeal = 0f; // used by females to accept or reject males
    private Animal last_mate;


    protected NavMeshAgent nav_agent;
    [SerializeField]
    protected float line_of_sight; //how far can the creature set its destination
    [SerializeField]
    protected int memory_capacity;
    [SerializeField]
    protected Color32 mating_color;
    protected Coords move_target;
    protected CREATURE_ACTION current_action;
    private string target_instance;
    
    


    [Header("Stats")]
    [SerializeField]
    public float max_hunger; // if hunger is equals to max hunger the creature dies
    protected float hunger;
    [SerializeField]
    public float max_thirst;
    protected float thirst;
    [SerializeField]
    protected float drinking_lapse = 3f;
    protected float drinking_time;
    [SerializeField]
    protected Color32 drinking_color;
    [SerializeField]
    protected Genes genoma;
    protected float urge_of_reproduction;
    protected float mating_duration = 0f;
    [SerializeField]
    protected float mating_duration_max = 3f;
    private float pregnecy_counter = 0f;


    public LocationsMemory<Coords> creature_memory;


    private void Awake() {
        this.genoma = new Genes();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        this.creature_memory = new LocationsMemory<Coords>(memory_capacity);
        this.move_target= new Coords(this.transform.position);
        this.nav_agent = this.GetComponent<NavMeshAgent>();
        this.nav_agent.speed = this.genoma.Speed;
        this.GetComponent<CapsuleCollider>().radius = this.genoma.SenseCapacity;
        this.fur_renderer = GetComponentInChildren<Renderer>();
        this.hunger = 0f;
        this.thirst = 0f;
        this.drinking_time = this.drinking_lapse;
        this.current_action = CREATURE_ACTION.EXPLORING;
        this.beBorn();

    }

    public Sex Sex
    {
        get {
            return this.genoma.sex;
        }
    }

    public float Hunger
    {
        get {
            return this.hunger;
        }
    }

    public float Thirst
    {
        get {
            return this.thirst;
        }
    }

    public float Pregnacy
    {
        get {
            return this.pregnecy_counter;
        }
    }

    public float Appeal
    {
        get {
            return this.genoma.Appeal;
        }
    }

    public Animal LastMate
    {
        get {
            return this.last_mate;
        }
    }

    public Genes Genoma
    {
        get {
            return this.genoma;
        }
    }

    private void beBorn()
    {
        this.changeFurColor(this.genoma.FurColor);
        Enviroment.SingletonEviroment.registerBirth(this.gameObject, this.gameObject.GetInstanceID().ToString());
        this.explore();
    }

    protected void changeFurColor(Color new_color)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", new_color);
        this.fur_renderer.SetPropertyBlock(block);
    }

    protected bool validateCoord(Coords coord, float y_axis)
    {
        
        bool is_validate = true;
        if(y_axis < 0f)
        {
            is_validate = false;
        }
        else if(this.creature_memory.remember(coord))
        {
            is_validate = false;
        }
        else if(((Vector3)coord - this.transform.position).magnitude < 6f)
        {
            is_validate = false;
        }
        return is_validate;
    }

    protected Coords getRandomNearPoint()
    {
        Vector3 randomPoint = (UnityEngine.Random.insideUnitSphere * this.line_of_sight) + transform.position;
        Coords randomPointCoords;
        NavMeshHit meshHit;
        NavMesh.SamplePosition(randomPoint, out meshHit, this.line_of_sight, -1);
        randomPointCoords = new Coords(meshHit.position.x,  meshHit.position.z);
        if(!this.validateCoord(randomPointCoords, meshHit.position.y))
        {
            return getRandomNearPoint();
        }
        return randomPointCoords;
    }

    public void reset(Genes genes)
    {
        this.is_pregnet = false;
        this.best_mate_appeal = 0f;
        this.current_action = CREATURE_ACTION.EXPLORING;
        this.hunger = 0f;
        this.thirst = 0f;
        this.drinking_time = 0f;
        this.genoma = genes;
        this.nav_agent.speed = genes.Speed;
        this.urge_of_reproduction = 0f;
        this.mating_duration = 0f;
        this.pregnecy_counter =0f;
        this.creature_memory = new LocationsMemory<Coords>(this.memory_capacity);

    }

    // Update is called once per frame
    void Update()
    {
        if(this.move_target == this.transform.position)
        {
            switch(this.current_action)
            {
                case CREATURE_ACTION.DRINKING:
                    break;
                case CREATURE_ACTION.EXPLORING:
                    this.explore();
                    break;
                case CREATURE_ACTION.EATING:
                    this.eat();
                    break;
                case CREATURE_ACTION.MATING:
                    break;
                case CREATURE_ACTION.RESTING:
                    break;
                default:
                    this.current_action = CREATURE_ACTION.EXPLORING;
                    break;
            }
        }

        if(this.current_action == CREATURE_ACTION.MATING)
        {
            this.progressMatingRitual();
        }
        if(this.is_pregnet)
        {
            this.pregnecy_counter += Time.deltaTime;
            if(this.pregnecy_counter >= this.genoma.GestationDuration)
            {
                this.pregnecy_counter = 0f;
                this.is_pregnet = false;
                Enviroment.SingletonEviroment.createLife(this);
            }
        }
        if(this.current_action == CREATURE_ACTION.DRINKING)
        {
            if(this.drinking_time <= 0)
            {
                this.current_action = CREATURE_ACTION.EXPLORING;
                this.thirst = 0f;
                this.drinking_time = this.drinking_lapse;
                this.changeFurColor(this.genoma.FurColor);
                this.explore();
            }
            else
            {
                this.drinking_time -= Time.deltaTime;
            }
        }
        else
        {
            this.thirst += (this.max_thirst * this.genoma.Resistence) * Time.deltaTime/2;
        }
        this.hunger += (this.max_hunger * this.genoma.Resistence) * Time.deltaTime;
        this.urge_of_reproduction += Time.deltaTime;
        this.checkCretureStatus();
        Debug.DrawRay(this.transform.position, (move_target - this.transform.position ), Color.red);
    }

    protected virtual void eat()
    {
        if (this.target_instance != null)
        {
            this.hunger = 0;
            this.current_action = CREATURE_ACTION.EXPLORING;
            Enviroment.SingletonEviroment.registerDeath(this.diet,this.target_instance);
        }
            
    }

    private void checkCretureStatus()
    {
        if(hunger >= this.max_hunger || this.thirst >= this.max_thirst)
        {
            if(this.current_action == CREATURE_ACTION.MATING)
            {
                this.hunger /= 2f;
                this.thirst /= 2f;
                return;
            }
            // Debug.Log("Mori");
            this.die();
        }
    }

    private void die()
    {
        Enviroment.SingletonEviroment.registerDeath(this.spicies, this.gameObject.GetInstanceID().ToString());
    }

    private void explore()
    {
        this.move_target = this.getRandomNearPoint();
        this.creature_memory.store(move_target);
        this.nav_agent.SetDestination(this.move_target);        
    }

    void OnTriggerEnter(Collider other) {
        if(other.tag == "LivingBeen" && this.current_action == CREATURE_ACTION.EXPLORING)
        {
            if(this.diet == SPECIES.Plant)
            {   
                Plant plant = other.gameObject.GetComponent<Plant>();
                if (plant == null)
                {
                    return;
                } 
                if(this.hunger > (this.max_hunger * 0.25f))
                {
                    this.current_action = CREATURE_ACTION.EATING;
                    this.move_target = new Coords(plant.Location);
                    this.target_instance = other.gameObject.GetInstanceID().ToString();
                    this.nav_agent.SetDestination(this.move_target);
                }
            }
        }
        else if(other.tag == "water" && this.current_action == CREATURE_ACTION.EXPLORING)
        {
            if(this.thirst > (this.max_thirst * 0.2f))
            {                        
                this.target_instance = other.gameObject.GetInstanceID().ToString();
                this.current_action = CREATURE_ACTION.DRINKING;
                this.changeFurColor(this.drinking_color);
                this.move_target = new Coords(other.transform.position);
                this.nav_agent.SetDestination(this.move_target);
            }
        }
        else if(other.tag == this.gameObject.tag && this.Sex != Sex.FEMALE && this.urge_of_reproduction > 3f)
        {
            Animal mate = other.GetComponent<Animal>();
            this.attemptReproduction(mate);
        }
    }

    private void progressMatingRitual()
    {
        this.mating_duration += Time.deltaTime;
        if(mating_duration >= this.mating_duration_max)
        {
            this.urge_of_reproduction = 0f;
            this.changeFurColor(this.genoma.FurColor);
            this.nav_agent.isStopped = false;
            this.nav_agent.stoppingDistance = 0f;
            this.current_action = CREATURE_ACTION.EXPLORING;
            this.is_pregnet = this.Sex == Sex.FEMALE;
            this.mating_duration = 0;
            this.explore();
        }
    }

    public void lookAtPoint(Vector3 location)
    {
        Vector3 direction = (location - this.transform.position).normalized;
        Quaternion look_rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        this.transform.rotation = look_rotation;
    }

    public bool propose(Animal male)
    {
        if(this.Sex == Sex.FEMALE && this.current_action == CREATURE_ACTION.EXPLORING && !this.is_pregnet)
        {
            if(male.Appeal >= (this.best_mate_appeal * 0.9f))
            {
                this.last_mate = male;
                this.current_action = CREATURE_ACTION.MATING;
                this.nav_agent.isStopped = true;
                this.lookAtPoint(male.Location);
                this.best_mate_appeal = male.Appeal > this.best_mate_appeal ? male.Appeal : best_mate_appeal;
                this.changeFurColor(this.mating_color);
                return true;
            }
        }
        return false;
    }



    protected virtual void attemptReproduction(Animal mate)
    {
        if(mate.spicies == this.spicies && mate.Sex == Sex.FEMALE)
        {
            this.current_action = CREATURE_ACTION.RESTING;
            this.nav_agent.isStopped = true;
            this.lookAtPoint(mate.Location);
            if(mate.propose(this))
            {
                this.current_action = CREATURE_ACTION.MATING;
                this.nav_agent.isStopped = false;
                this.move_target = new Coords(mate.Location);
                this.nav_agent.stoppingDistance = 0.5f;
                this.changeFurColor(this.mating_color);
                this.nav_agent.SetDestination(this.move_target);
            }
            else
            {
                this.nav_agent.isStopped = false;
                this.current_action =CREATURE_ACTION.EXPLORING;// y llorando bien cabron ademas
                this.explore();
            }
        }
    } 
}
