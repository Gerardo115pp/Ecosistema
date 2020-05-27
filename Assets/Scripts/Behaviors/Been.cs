using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingBeen : MonoBehaviour
{
    public SPECIES spicies;
    public Renderer fur_renderer;

    protected bool im_i_dead;


    // Start is called before the first frame update
    void Start()
    {
        this.im_i_dead = false;
    }

    public Vector3 Location
    {
        get{
            return this.transform.position;
        }
    }
}
