using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{

    [Header("Camera movement")]
    [SerializeField]
    private float smooth_speed;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private float current_zoom;
    [SerializeField]
    private float zoom_speed; 
    private CameraModes camera_mode;

    [SerializeField]
    private float min_zoom;
    [SerializeField]
    private float max_zoom;

    private Camera main_camera;
    private Transform camera_target;


    // Start is called before the first frame update
    void Start()
    {
        this.main_camera = this.gameObject.GetComponent<Camera>();
        this.camera_mode = CameraModes.TARGETING;
    }

    // Update is called once per frame
    void Update()
    {
        current_zoom -= Input.GetAxis("Mouse ScrollWheel") * zoom_speed; 
        current_zoom = Mathf.Clamp(current_zoom, 0.5f, 5f);
        this.handelMouseInput();
        this.handelKeyStokes();
    }

    private void zoom()
    {
        float new_zoom = Mathf.Lerp(this.max_zoom, this.min_zoom, Enviroment.SingletonEviroment.getCreturesGreatestDistance()/18f);
        // Debug.Log(Enviroment.SingletonEviroment.getCreturesGreatestDistance());
        this.main_camera.fieldOfView = new_zoom;
    }

    private void LateUpdate()
    {
        if(this.camera_target != null)
        {
            Vector3 target_position = this.camera_mode == CameraModes.TARGETING ? this.camera_target.transform.position : Enviroment.SingletonEviroment.getCreturesCenterPoint();
            Vector3 desaierd_position = target_position + this.offset * this.current_zoom,
                    smooth_position =Vector3.Lerp(transform.position,desaierd_position, smooth_speed); 
            this.transform.position = smooth_position;
            if(this.camera_mode == CameraModes.FULL_SCENE)
            {
                this.zoom();
            }
            this.transform.LookAt(target_position);
        }
        else if(this.camera_mode == CameraModes.TARGETING)
        {
            Animal creature = Enviroment.SingletonEviroment.getRandomCreature();
            this.camera_target = creature.transform;
            UIManager.SingletonUIManager.SelectedAnimal = creature;
        }
    }

    private void handelKeyStokes()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            this.camera_mode = this.camera_mode == CameraModes.TARGETING ? CameraModes.FULL_SCENE : CameraModes.TARGETING;
            // this.camera_target = null;
        }
    }

    private void handelMouseInput()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = this.main_camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit ray_hit;
            if(Physics.Raycast(ray, out ray_hit, 100))
            {
                Debug.Log(ray_hit.collider.gameObject.tag);
                if(ray_hit.collider.gameObject.tag == "bunny")
                {
                    this.camera_target = ray_hit.transform;
                    UIManager.SingletonUIManager.SelectedAnimal = ray_hit.collider.gameObject.GetComponent<Animal>();
                }
            }   
        }
        if(Input.GetMouseButtonDown(2))
        {
            Animal creature = Enviroment.SingletonEviroment.getRandomCreature();
            this.camera_target = creature.transform;
            UIManager.SingletonUIManager.SelectedAnimal = creature;
        }
    }
}
