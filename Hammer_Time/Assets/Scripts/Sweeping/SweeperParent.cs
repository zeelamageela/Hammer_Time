using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweeperParent : MonoBehaviour
{
    public Sweeper[] sweeperLayers;

    [Header("Broom (this Sweeper should also be included in sweeperLayers above, so it gets the same Sweep/Hard/Whoa triggers as the other layers)")]
    [Tooltip("The layer whose sprite/animation is the broom itself - the one that swaps per equipped handle tier.")]
    public Sweeper broomLayer;

    [Tooltip("Index-matched to handle tier: 0=Wooden, 1=Fibreglass, 2=Composite, 3=Carbon Fibre, 4=Exotic Carbon Fibre.")]
    public AnimatorOverrideController[] broomOverrides;

    [Tooltip("Tints broomLayer's sprite renderer(s) - assign broomLayer's SpriteRenderer(s) to its colour1GO list.")]
    public CharColourChanger broomColour;

    // Called from SweeperManager.SetupSweepers() once per turn. Wooden Handle (tier 0)
    // stays white to match EquipmentManager's color rule for the shop; every other tier
    // takes whichever side (red/yellow) is currently sweeping.
    public void SetBroom(int handleTierIndex, Color sideColor)
    {
        if (broomLayer != null && broomLayer.anim != null && broomOverrides != null
            && handleTierIndex >= 0 && handleTierIndex < broomOverrides.Length && broomOverrides[handleTierIndex] != null)
        {
            broomLayer.anim.runtimeAnimatorController = broomOverrides[handleTierIndex];
        }

        if (broomColour != null)
        {
            broomColour.TeamColour(handleTierIndex == 0 ? Color.white : sideColor);
        }
    }

    public bool sweep;
    public bool hard;
    public bool whoa;

    public float xOffset;
    public float yOffset;

    Vector3 currentEulerAngles;
    Quaternion currentRotation;

    float x;
    float y;
    float z;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //sweeperParent = transform.parent;

        //Vector3 followSpot = new Vector3((xOffset + transform.parent.position.x), (transform.parent.position.y + yOffset), 0f);
        //transform.position = followSpot;
        //float angle;
        //angle = -2 * (transform.parent.rotation.z * Mathf.Rad2Deg);

        //if (angle > 90f)
        //    angle = (-2 * (transform.parent.rotation.z * Mathf.Rad2Deg)) - 90f;
        //else if (angle < -90f)
        //    angle = (-2 * (transform.parent.rotation.z * Mathf.Rad2Deg)) + 90f;
        //else
        //    angle = -2 * (transform.parent.rotation.z * Mathf.Rad2Deg);

        //Debug.Log("Angle is " + angle);
        ////transform.localRotation = Quaternion.AngleAxis(-angle, Vector3.forward);
        //transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.identity;
        float angle = transform.localRotation.z;
        float absAngle = Mathf.Abs(angle) * Mathf.Rad2Deg;
        //Debug.Log("Abs Angle " + absAngle);

        transform.localPosition = new Vector3(0f, ((yOffset - 0.3f) * ((absAngle - 90f) / -90f)) + 0.3f, 0f);
        //if (absAngle > 90f)
        //    transform.localPosition = new Vector3(0f, ((yOffset - 0.3f) * ((absAngle - 180f) / -90f)) + 0.3f, 0f);
        //else if (absAngle > 180f)
        //    transform.localPosition = new Vector3(0f, ((yOffset - 0.3f) * ((absAngle - 270f) / -90f)) + 0.3f, 0f);
        //else
        //    transform.localPosition = new Vector3(0f, yOffset, 0f);

    }

    public void Sweep()
    {
        sweep = true;
        hard = false;
        whoa = false;

        for (int i = 0; i < sweeperLayers.Length; i++)
        {
            sweeperLayers[i].Sweep();
        }
    }

    public void Hard()
    {
        sweep = false;
        hard = true;
        whoa = false;

        for (int i = 0; i < sweeperLayers.Length; i++)
        {
            sweeperLayers[i].Hard();
        }
    }

    public void Whoa()
    {
        sweep = false;
        hard = false;
        whoa = true;

        for (int i = 0; i < sweeperLayers.Length; i++)
        {
            sweeperLayers[i].Whoa();
        }
    }
}
