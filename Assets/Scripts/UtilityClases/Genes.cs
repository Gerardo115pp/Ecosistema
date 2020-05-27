using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenesException : System.Exception
{
    public GenesException(string message) : base(message)
    {
        
    }
}


// [Serializable]
public class Genes
{
    private static string[] animal_genes = new  string[] {"speed", "appeal", "resistence", "gestation_duration", "sense_capacity"};
    private static float mutation_factor = 0.45f;

    public static Genes mixGenes(Genes male_genes, Genes female_genes, Genes Random_genes)// form a random living member of the population
    {   
        Dictionary<string, float> mutant_genes = Genes.createMutant(female_genes, male_genes, Random_genes);
        return new Genes(mutant_genes, female_genes);
    }

    protected static float[] getGeneticContraints(string gen_name)
    {
        float[] genetic_constraints;
        switch(gen_name)
        {
            case "speed":
                genetic_constraints = new float[] {0.5f, 3f};
                break;
            case "resistence":
                genetic_constraints = new float[] {0.002f, 1.2f};//the lower the better
                break;
            case "gestation_duration":
                genetic_constraints = new float[] {10f, 20f};//the lower the better
                break;
            case "sense_capacity":
                genetic_constraints = new float[] {0.5f,5f};
                break;
            case "appeal":
                genetic_constraints = new float[] {-1000f, 1000f};
                break;
            default:
                genetic_constraints = new float[] {-1f, -1f};
                Debug.Log("That shouldnt have happend");
                break;
        }
        return genetic_constraints;
    }

    protected static float calculateGeneticAppeal(Dictionary<string, float> genoma)
    {
        float appeal = 0f;
        foreach(string gen in genoma.Keys)
        {
            switch(gen)
            {
                case "speed":
                    appeal += genoma[gen];
                    break;
                case "resistence":
                    appeal += Genes.getGeneticContraints(gen)[1] - genoma[gen];//the lower the better
                    break;
                case "gestation_duration":
                    appeal += Genes.getGeneticContraints(gen)[1] - genoma[gen];//the lower the better
                    break;
                case "sense_capacity":
                    appeal += genoma[gen];
                    break;
                case "appeal":
                    break;
                default:
                    Debug.Log("That shouldnt have happend");
                    break;
            }
        }
        return appeal;
    }

    protected static Dictionary<string, float> createMutant(Genes a, Genes b, Genes c)
    {
        Dictionary<string,float> mutant = new Dictionary<string, float>();
        float[] genetic_constraints;
        foreach(string gen in Genes.animal_genes)
        {
            if(gen == "appeal")
            {
                continue;
            }
            mutant[gen] = a.getGenByName(gen) + Genes.mutation_factor * (b.getGenByName(gen) - c.getGenByName(gen));
            genetic_constraints = Genes.getGeneticContraints(gen);
            mutant[gen] = Mathf.Clamp(mutant[gen], genetic_constraints[0], genetic_constraints[1]);
        }
        return mutant;
    }

    private Dictionary<string,float> cromosome;
    private Color32 furcolor;
    public Sex sex;

    public Genes()
    {
        this.sex = (Sex)UnityEngine.Random.Range(0,2);
        this.cromosome = this.generateRandomCromosome();
        this.__init__();
    }

    public Genes(Sex sex)
    {
        this.sex = sex;
        this.cromosome = this.generateRandomCromosome();
        this.__init__();
    }

    public Genes(Dictionary<string, float> genoma, Genes mother)
    {
        this.sex = (Sex)UnityEngine.Random.Range(0,2);
        Debug.Log($"Im a {this.sex}");
        //binomial crossover
        float crossover_point = 0.45f;
        this.cromosome = genoma;// literal
        foreach(string gen in Genes.animal_genes)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            if(gen == "appeal")
            {
                continue;
            }
            this.cromosome[gen] = UnityEngine.Random.value > crossover_point ? this.cromosome[gen] : mother.getGenByName(gen);
        }
        this.cromosome["appeal"] = Genes.calculateGeneticAppeal(this.cromosome);
        this.__init__();
    }

    protected virtual void __init__()
    {
        this.furcolor = this.getFurColor();
    }

    public Color32 getFurColor()
    {
        if(this.sex == Sex.FEMALE)
        {
            return new Color32(255,100,200,255);
            //rgb(200,200,200);
        }
        return new Color32(120,120,120, 255);
    }


    public override string ToString()
    {
        string response = "";
        foreach(string gen in this.cromosome.Keys)
        {
            response += $"{gen}: {this.cromosome[gen]}; ";
        }
        response += $"Sex: {this.sex}";
        return response;
    }

    private Dictionary<string, float> generateRandomCromosome()
    {
        Dictionary<string, float> new_cromosome = new Dictionary<string, float>();
        float gen = 0f,
              appeal = 0f;
        foreach(string gen_name in Genes.animal_genes)
        {
            switch(gen_name)
            {
                case "speed":
                    gen = UnityEngine.Random.Range(1f,2f);
                    break;
                case "resistence":
                    gen = UnityEngine.Random.Range(0.09f, 0.9f)/10f;
                    break;
                case "gestation_duration":
                    gen = UnityEngine.Random.Range(10f, 20f);
                    break;
                case "sense_capacity":
                    gen = UnityEngine.Random.Range(1f,3f)/10f;
                    break;
                case "appeal":
                    break;
                default:
                    Debug.Log("That shouldnt have happend");
                    break;
            }
            appeal += gen;
            new_cromosome[gen_name] = gen;
        }
        new_cromosome["appeal"] = appeal; 
        return new_cromosome;
    }

#region <Getters y Setters>

    public Color32 FurColor{
        get {
            return this.furcolor;
        }
    }

    public float Appeal{
        get {
            return this.cromosome["appeal"];
        }
    }

    public float Speed { 
        get{
            return this.cromosome["speed"];
        }
    }

    public float Resistence
    {
        get {
            return this.cromosome["resistence"];
        }
    }

    public float GestationDuration
    {
        get {
            return this.cromosome["gestation_duration"];
        }
    }

    public float SenseCapacity
    {
        get {
            return this.cromosome["sense_capacity"];
        }
    }

    public float getGenByName(string gen_name)
    {
        if(this.cromosome.ContainsKey(gen_name))
        {
            return this.cromosome[gen_name];
        }
        throw new GenesException($"try to access unkown gene '{gen_name}'");
    }

#endregion </Getters y Setters>


}
