using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterAnim : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private RelativeJoint2D rj;

    GameManager gm;
    GameObject gmGO;

    public bool isPressed = false;
    public bool springReleased = false;
    public bool isReleased = false;
    public float pullback;
    GameObject rock;
    Rock_Flick rockFlick;
    Rock_Info rockInfo;

    public float releasePoint;
    public float backSwingPoint;
    public float throwDistance;

    Vector2 startPoint;
    Vector2 endPoint;
    public float springDistance;
    public Vector2 springDirection;
    float angle;
    public float throwSpeed;
    public bool extend;

    public bool amIRock;

    int slowdownTimer;
    
    // Flick shot swipe control
    private bool isSwipeControlled = false;
    private float swipeProgress = 0f;
    private float lastSwipeProgress = 0f;
    public float releaseThreshold = 0.6f; // Minimum progress to allow release


    // Start is called before the first frame update
    void Start()
    {
        slowdownTimer = 0;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rj = GetComponent<RelativeJoint2D>();

        gmGO = GameObject.Find("GameManager");
        gm = gmGO.GetComponent<GameManager>();

        rock = gm.rockList[gm.rockCurrent].rock;
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();

        rj.connectedBody = rock.GetComponent<Rigidbody2D>();
        rj.enabled = false;
    }


    void Update()
    {

        isPressed = rockFlick.isPressed;
        springReleased = rockFlick.springReleased;
        isReleased = rockInfo.released;
        springDistance = rockFlick.springDistance;
        springDirection = rockFlick.springDirection;

        // Handle swipe-controlled animation during flick shot power phase
        if (isSwipeControlled)
        {
            UpdateSwipeControlledAnimation();
            return; // Skip normal animation logic
        }

        if (isPressed)
        {

            anim.SetBool("mouseDown", true);
            if (springDistance > 1f && rock.transform.position.y < -25.5f)
            {
                pullback = (springDistance) / 5f;
                pullback = Mathf.Clamp(pullback, 0f, 1f);
                anim.SetBool("mouseDown", false);
                anim.Play("Shooter_2_Backswing", 0, pullback);
            }
            else
            {
                pullback = 0f / 4f;
                pullback = Mathf.Clamp(pullback, 0f, 1f);
                anim.SetBool("mouseDown", false);
                anim.Play("Shooter_2_Backswing", 0, pullback);
            }

            if (pullback >= 1f)
            {
                angle = Mathf.Atan2(springDirection.y, springDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
            }

            
        }

        if (isPressed == false && springReleased == true)
        {
            throwDistance = rock.transform.position.y;
            throwDistance = Mathf.Clamp(throwDistance, backSwingPoint, releasePoint);

            throwSpeed = (throwDistance - backSwingPoint) / (releasePoint - backSwingPoint);
            //throwSpeed = (1f - pullback) + throwSpeed;
            anim.Play("Shooter_2_Kick", 0, throwSpeed);

            if (rock.transform.position.y >= releasePoint)
            {
                float slidePos = rock.transform.position.y;
                float slideSpeed = (slidePos - releasePoint) / (-16.03f - releasePoint);
                rj.enabled = true;

                //rb.transform.position = rock.transform.position;
                anim.SetBool("extend", true);
                anim.Play("Shooter_2_Slide", 0, slideSpeed);
            }

            if (transform.position.y >= -19.75f)
            {
                extend = false;
                anim.SetBool("isReleased", true);
                if (amIRock)
                    gameObject.SetActive(false);
                else
                    anim.Play("Shooter_2_Release", 0, 0f);
                rock.GetComponent<SpriteRenderer>().enabled = true;
                rj.enabled = false;

                if (gameObject.activeSelf == true && slowdownTimer <= 10)
                {
                    StartCoroutine(Slowdown());
                }

            }
        }

    }

    IEnumerator Slowdown()
    {
        //Debug.Log("slowdon " + slowdownTimer);
        rb.freezeRotation = true;
        rb.linearDamping = 0.25f * slowdownTimer;

        yield return new WaitForFixedUpdate();

        slowdownTimer++;
    }
    
    /// <summary>
    /// Enable swipe-controlled animation mode for flick shot
    /// Called by FlickShotController when power phase starts
    /// </summary>
    public void StartSwipeControl()
    {
        isSwipeControlled = true;
        swipeProgress = 0f;
        lastSwipeProgress = 0f;
        
        // Start at backswing position
        anim.SetBool("mouseDown", false);
        anim.Play("Shooter_2_Backswing", 0, 1f); // Full backswing
        
        Debug.Log("[ShooterAnim] Swipe control STARTED - animation driven by player input!");
    }
    
    /// <summary>
    /// Set animation progress based on swipe position
    /// Called continuously by FlickShotController during power phase
    /// Progress: 0 = backswing, 1 = full release
    /// </summary>
    public void SetSwipeProgress(float progress)
    {
        swipeProgress = Mathf.Clamp01(progress);
        
        // Detect if we've crossed release threshold
        if (swipeProgress >= releaseThreshold && lastSwipeProgress < releaseThreshold)
        {
            Debug.Log($"[ShooterAnim] Crossed release threshold! Progress: {swipeProgress:F2}");
        }
        
        lastSwipeProgress = swipeProgress;
    }
    
    /// <summary>
    /// Update animation based on current swipe progress
    /// Smoothly transitions through backswing ? kick ? slide phases
    /// FIXED: Properly sets animator speed to 0 for manual control
    /// </summary>
    private void UpdateSwipeControlledAnimation()
    {
        // CRITICAL: Set animator speed to 0 so we can manually control frame position
        anim.speed = 0f;
        
        if (swipeProgress < 0.4f)
        {
            // Phase 1: Backswing (0-40% progress)
            // Gradually reduce backswing as swipe begins
            float backswingAmount = 1f - (swipeProgress / 0.4f);
            anim.Play("Shooter_2_Backswing", 0, backswingAmount);
            
            Debug.Log($"[ShooterAnim] Backswing phase: progress={swipeProgress:F2}, backswingAmount={backswingAmount:F2}");
        }
        else if (swipeProgress < 0.8f)
        {
            // Phase 2: Kick (40-80% progress) - THE MONEY ZONE
            // Map 0.4-0.8 progress to 0-1 kick animation
            float kickProgress = (swipeProgress - 0.4f) / 0.4f;
            anim.Play("Shooter_2_Kick", 0, kickProgress);
            
            Debug.Log($"[ShooterAnim] Kick phase: progress={swipeProgress:F2}, kickProgress={kickProgress:F2}");
            
            // Smooth rotation toward launch direction
            if (springDirection.magnitude > 0.1f)
            {
                angle = Mathf.Atan2(springDirection.y, springDirection.x) * Mathf.Rad2Deg;
                float targetAngle = angle - 90f;
                float currentAngle = transform.rotation.eulerAngles.z;
                float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
            }
        }
        else
        {
            // Phase 3: Slide/Follow-through (80-100% progress)
            // Map 0.8-1.0 progress to 0-1 slide animation
            float slideProgress = (swipeProgress - 0.8f) / 0.2f;
            anim.SetBool("extend", true);
            anim.Play("Shooter_2_Slide", 0, slideProgress);
            
            Debug.Log($"[ShooterAnim] Slide phase: progress={swipeProgress:F2}, slideProgress={slideProgress:F2}");
            
            // Enable relative joint for slide follow-through
            if (!rj.enabled && slideProgress > 0.2f)
            {
                rj.enabled = true;
                Debug.Log("[ShooterAnim] Relative joint enabled - shooter following rock!");
            }
        }
    }
    
    /// <summary>
    /// Check if swipe has reached valid release point
    /// </summary>
    public bool CanRelease()
    {
        return swipeProgress >= releaseThreshold;
    }
    
    /// <summary>
    /// Get current swipe progress (0-1)
    /// </summary>
    public float GetSwipeProgress()
    {
        return swipeProgress;
    }
    
    /// <summary>
    /// Complete the release and transition to normal follow-through
    /// Called by FlickShotController when mouse is released
    /// FIXED: Properly transitions to normal animation system that tracks rock
    /// </summary>
    public void CompleteRelease()
    {
        Debug.Log($"[ShooterAnim] === COMPLETE RELEASE CALLED === Swipe progress: {swipeProgress:F2}");
        
        // CRITICAL: Re-enable animator speed for normal playback
        anim.speed = 1f;
        
        // CRITICAL: Disable swipe control FIRST
        isSwipeControlled = false;
        
        // CRITICAL: Set flags so normal Update() logic takes over
        isPressed = false;
        springReleased = true;
        
        // CRITICAL: Make sure we're at the right animation state
        float currentProgress = Mathf.Clamp01(swipeProgress);
        
        if (currentProgress < 0.8f)
        {
            // Released during kick phase
            Debug.Log($"[ShooterAnim] Released during kick (progress: {currentProgress:F2})");
            Debug.Log($"[ShooterAnim] Normal animation will now drive kick/slide based on rock position");
            
            // Don't play static frame - let normal logic handle it
            // Normal logic will use rock.transform.position.y to drive animation
        }
        else
        {
            // Released during slide phase
            Debug.Log($"[ShooterAnim] Released during slide (progress: {currentProgress:F2})");
            
            // Enable extend flag for slide
            anim.SetBool("extend", true);
            
            Debug.Log($"[ShooterAnim] extend flag set, normal animation will continue slide");
        }
        
        // The normal Update() loop will now take over since:
        // - isSwipeControlled = false (exits early return)
        // - springReleased = true (enters normal animation block)
        // - Rock is moving, so throwDistance will drive animation naturally
        
        Debug.Log("[ShooterAnim] === HANDED OFF TO NORMAL SYSTEM === Animation speed restored to 1.0");
    }
    
    /// <summary>
    /// Cancel swipe control and reset to normal mode
    /// </summary>
    public void CancelSwipeControl()
    {
        isSwipeControlled = false;
        swipeProgress = 0f;
        lastSwipeProgress = 0f;
        
        Debug.Log("[ShooterAnim] Swipe control cancelled");
    }

}


