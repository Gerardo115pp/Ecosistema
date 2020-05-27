using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UIManagerException : System.Exception
{
    public UIManagerException(string message) : base(message)
    {

    }
}

public class UIManager : MonoBehaviour
{

    public static UIManager SingletonUIManager;

    [Header("General")]
    [SerializeField]
    private float update_cooldown = 1f;
    private float update_timer;

    private Animal selected_animal;
    public bool is_gameover = false;

    [Header("UI elements")]
    [SerializeField]
    private Text bunny_counter;
    [SerializeField]
    private Text bunny_resistence;
    [SerializeField]
    private Text bunny_speed;
    [SerializeField]
    private Text bunny_appeal;
    [SerializeField]
    private Text bunny_senses;
    [SerializeField]
    private Text bunny_pregnet;
    [SerializeField]
    private Text bunny_gender;
    [SerializeField]
    private Text end_screen;
    [SerializeField]
    private Slider slider_hunger;
    [SerializeField]
    private Slider slider_thirst;
    [SerializeField]
    private Slider slider_pregnecy;


    void Awake() {
        if(UIManager.SingletonUIManager == null)
        {
            UIManager.SingletonUIManager = this;
            return;
        }
        throw new UIManagerException("Attempt to re define UIManager singleton");

    }

    // Start is called before the first frame update
    void Start()
    {
        this.update_timer = this.update_cooldown;
    }

    void Update()
    {
        if(is_gameover && !this.end_screen.gameObject.activeSelf)
        {   
            this.end_screen.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void LateUpdate() {
        this.update_timer += Time.deltaTime;
        if(this.update_timer > this.update_cooldown && !is_gameover)
        {
            this.update_timer = 0f;
            this.updateUI();
        }   
    }

    private void updateUI()
    {
            this.bunny_counter.text = Enviroment.SingletonEviroment.LivingBeensCount.ToString();
            if(this.selected_animal != null)
            {
                this.bunny_resistence.text = this.selected_animal.Genoma.Resistence.ToString();
                this.bunny_speed.text = this.selected_animal.Genoma.Speed.ToString();
                this.bunny_appeal.text = this.selected_animal.Genoma.Appeal.ToString();
                this.bunny_senses.text = this.selected_animal.Genoma.SenseCapacity.ToString();
                this.bunny_pregnet.text = this.selected_animal.is_pregnet ? "si" : "no";
                this.bunny_gender.text = this.selected_animal.Sex == Sex.FEMALE ? "Hembra" : "Macho";
                this.UpdateSliders();
            }
    }

    private void UpdateSliders()
    {
        this.slider_hunger.value = this.selected_animal.Hunger;
        this.slider_thirst.value = this.selected_animal.Thirst;
        this.slider_pregnecy.value = this.selected_animal.Pregnacy;
    }

    public Animal SelectedAnimal
    {
        set{
            this.slider_hunger.maxValue = value.max_hunger;
            this.slider_thirst.maxValue = value.max_thirst;
            this.slider_pregnecy.maxValue = value.Genoma.GestationDuration;
            this.selected_animal = value;
            this.update_timer = this.update_cooldown;
        }
    }
}
