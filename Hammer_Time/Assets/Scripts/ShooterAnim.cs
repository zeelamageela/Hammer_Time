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

    Vector2 startPoint;
    Vector2 endPoint;
    public float springDistance;
    public Vector2 springDirection;
    float angle;
    public bool amIRock;

    // the delivery: from the moment the rock leaves until the shooter lets go (see Deliver)
    bool delivering, letGo, moving;
    Vector2 startPivot, startGap, sideways;
    float startAlong, moveStartAlong, letGoTime, coastDrag;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rj = GetComponent<RelativeJoint2D>();

        gmGO = GameObject.Find("GameManager");
        gm = gmGO.GetComponent<GameManager>();

        rock = gm.rockList[gm.rockCurrent].rock;
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();

        // The prefabs' joint is no longer used (see Deliver); it must stay off so the shooter never pulls on the rock.
        if (rj != null) rj.enabled = false;

        // Each shooter body used to weigh 1 (three of them: body, torso, head) against a 145 rock and took about 2% of its speed
        // through the joint. Kept nearly weightless so nothing it touches is affected.
        if (rb != null) rb.mass = ShooterBodyMass;
    }

    const float ShooterBodyMass = 0.01f;

    /// <summary>The back line at the throwing end, where the shot timer starts.</summary>
    public const float DefaultReleasePointY = -24.077f;

    // Where the sheet's rock is drawn from the shooter's pivot, pose by pose (Shooter_Yellow/Red_135_1, 85 pixels per unit).
    // The push-off is drawn with the shooter staying put while the rock swings forward, so these say which pose goes with
    // where the rock is. Re-measure them if that art changes.
    static readonly Vector2[] BackswingRock =
    {
        new Vector2(0.064f, 0.682f), new Vector2(0.064f, 0.671f), new Vector2(0.064f, 0.612f), new Vector2(0.087f, 0.488f),
        new Vector2(0.117f, 0.282f), new Vector2(0.146f, 0.024f), new Vector2(0.170f, -0.224f), new Vector2(0.187f, -0.341f),
        new Vector2(0.199f, -0.406f), new Vector2(0.205f, -0.424f), new Vector2(0.205f, -0.435f), new Vector2(0.205f, -0.441f)
    };
    static readonly Vector2[] KickRock =
    {
        new Vector2(0.164f, -0.088f), new Vector2(0.099f, 0.353f), new Vector2(0.064f, 0.671f),
        new Vector2(0.064f, 0.941f), new Vector2(0.064f, 1.165f), new Vector2(0.064f, 1.353f)
    };
    static readonly Vector2[] SlideRock =
    {
        new Vector2(0.064f, 1.506f), new Vector2(0.064f, 1.635f), new Vector2(0.064f, 1.729f),
        new Vector2(0.064f, 1.729f), new Vector2(0.064f, 1.729f), new Vector2(0.064f, 1.729f)
    };
    // How far forward the body (middle of head and shirt) is drawn in those same poses. All four characters are within a
    // few pixels of each other. Slide poses past the third are the third again.
    static readonly float[] BackswingBody = { 0.21f, 0.19f, 0.16f, 0.12f, 0.09f, 0.065f, 0.035f, 0.02f, 0.01f, 0.003f, -0.002f, -0.005f };
    static readonly float[] KickBody = { 0.06f, 0.16f, 0.29f, 0.445f, 0.605f, 0.765f };
    static readonly float[] SlideBody = { 0.915f, 0.94f, 0.955f, 0.955f, 0.955f, 0.955f };
    const int FirstBehindFrame = 6;   // backswing frames from here on have the rock behind where the kick begins

    /// <summary>The rock's place in the settled slide: from here on shooter and rock travel as one.</summary>
    public static readonly Vector2 DrawnRockOffset = new Vector2(0.064f, 1.729f);

    /// <summary>The release clip plays once at 12 frames a second; change this if it gets more or fewer frames.</summary>
    public const float ReleaseSeconds = 13f / 12f;

    /// <summary>The pivot height at which the shooter lets go.</summary>
    const float LetGoPivotY = -19.75f;

    // after release the shooter eases off: over about this distance, with the drag coming in over this long
    const float CoastDistance = 2f;
    const float CoastEaseSeconds = 0.4f;
    // any sideways difference between the drawn aim and the rock's path is taken up over this distance
    const float SidewaysSettleDistance = 1f;

    public enum Clip { Backswing, Kick, Slide }

    /// <summary>
    /// The pose that goes with a rock this far ahead of where the shooter started (along the way the shooter faces): the
    /// forward swing while it is still behind, then the kick and into the slide, whichever drawing has the rock nearest.
    /// </summary>
    public static void PoseFor(float along, out Clip clip, out int frame, out Vector2 drawnRock)
    {
        if (along < KickRock[0].y)
        {
            clip = Clip.Backswing;
            frame = Nearest(BackswingRock, FirstBehindFrame, along);
            drawnRock = BackswingRock[frame];
            return;
        }
        int kick = Nearest(KickRock, 0, along);
        int slide = Nearest(SlideRock, 0, along);
        if (Mathf.Abs(KickRock[kick].y - along) <= Mathf.Abs(SlideRock[slide].y - along))
        {
            clip = Clip.Kick; frame = kick; drawnRock = KickRock[kick];
        }
        else
        {
            clip = Clip.Slide; frame = slide; drawnRock = SlideRock[slide];
        }
    }

    /// <summary>
    /// Where the body should be for a rock this far along: the drawings' body positions, joined up between one pose and the
    /// next. Moving the shooter by the difference from the drawing on screen lets the body travel evenly while the poses step.
    /// </summary>
    public static float BodyAt(float along)
    {
        float rockBefore = BackswingRock[BackswingRock.Length - 1].y, bodyBefore = BackswingBody[BackswingBody.Length - 1];
        if (along <= rockBefore) return bodyBefore;
        // in the order the rock passes them: back of the swing forward, then the kick, then into the slide
        for (int i = BackswingRock.Length - 2; i >= FirstBehindFrame; i--)
            if (Between(along, BackswingRock[i].y, BackswingBody[i], ref rockBefore, ref bodyBefore, out float body)) return body;
        for (int i = 0; i < KickRock.Length; i++)
            if (Between(along, KickRock[i].y, KickBody[i], ref rockBefore, ref bodyBefore, out float body)) return body;
        for (int i = 0; i < SlideRock.Length; i++)
            if (Between(along, SlideRock[i].y, SlideBody[i], ref rockBefore, ref bodyBefore, out float body)) return body;
        return bodyBefore;
    }

    static bool Between(float along, float rockNext, float bodyNext, ref float rockBefore, ref float bodyBefore, out float body)
    {
        if (along <= rockNext)
        {
            body = Mathf.Lerp(bodyBefore, bodyNext, Mathf.InverseLerp(rockBefore, rockNext, along));
            return true;
        }
        rockBefore = rockNext;
        bodyBefore = bodyNext;
        body = bodyNext;
        return false;
    }

    static int Nearest(Vector2[] rocks, int from, float y)
    {
        int best = from;
        for (int i = from + 1; i < rocks.Length; i++)
            if (Mathf.Abs(rocks[i].y - y) < Mathf.Abs(rocks[best].y - y)) best = i;
        return best;
    }

    /// <summary>Normalized time for a clip played once and then held on its last frame.</summary>
    static float Once(float seconds, float length)
    {
        return Mathf.Clamp(seconds / length, 0f, 0.999f);
    }

    /// <summary>Shows one drawing of a clip.</summary>
    void Show(string state, int frame, int frames)
    {
        anim.Play(state, 0, (frame + 0.5f) / frames);
    }

    /// <summary>
    /// The rock has left. Everything goes at the rock's pace: it starts where the shooter holds it and builds up speed (see
    /// RockSlide.PushOff) while the shooter stays put and poses to match, then they travel together, and the shooter lets go. (The prefabs' joint is not used.) The rock drawn in the
    /// shooter sheet - the "amIRock" copy - is kept on the real rock throughout, so the real one can take over unnoticed.
    /// </summary>
    void Deliver()
    {
        if (!delivering) Begin();

        if (letGo)
        {
            float since = Time.time - letGoTime;
            anim.Play("Shooter_2_Release", 0, Once(since, ReleaseSeconds));
            if (rb != null) rb.linearDamping = coastDrag * Mathf.SmoothStep(0f, 1f, since / CoastEaseSeconds);
            return;
        }

        Vector2 forward = transform.up;
        Vector2 rockPos = rock.transform.position;
        float along = Vector2.Dot(rockPos - startPivot, forward);

        // any sideways difference between the pull-back drawing and the rock's line is taken up during the push-off
        float closed = Mathf.InverseLerp(startAlong, DrawnRockOffset.y, along);
        Vector2 seen = rockPos + startGap * (1f - Mathf.SmoothStep(0f, 1f, closed));
        float seenAlong = Vector2.Dot(seen - startPivot, forward);

        Clip clip; int frame; Vector2 drawn;
        PoseFor(seenAlong, out clip, out frame, out drawn);
        bool settled = seenAlong >= DrawnRockOffset.y;
        if (settled) { clip = Clip.Slide; frame = SlideRock.Length - 1; drawn = DrawnRockOffset; }

        if (clip == Clip.Backswing) Show("Shooter_2_Backswing", frame, BackswingRock.Length);
        else if (clip == Clip.Kick) Show("Shooter_2_Kick", frame, KickRock.Length);
        else Show("Shooter_2_Slide", frame, SlideRock.Length);

        Vector2 behind = seen - (Vector2)(transform.rotation * DrawnRockOffset);   // the pivot that has the settled rock on it
        Vector2 pivot;
        if (amIRock)
        {
            pivot = seen - (Vector2)(transform.rotation * drawn);
        }
        else if (!settled)
        {
            // slide the drawing so the body moves evenly instead of jumping from one pose's place to the next
            float[] bodies = clip == Clip.Backswing ? BackswingBody : clip == Clip.Kick ? KickBody : SlideBody;
            Vector2 ahead = transform.up;
            pivot = startPivot + ahead * (BodyAt(seenAlong) - bodies[frame]);
        }
        else
        {
            if (!moving)
            {
                moving = true;
                moveStartAlong = seenAlong;
                Vector2 side = transform.right;
                sideways = side * Vector2.Dot(startPivot - behind, side);
            }
            float taken = Mathf.SmoothStep(0f, 1f, (seenAlong - moveStartAlong) / SidewaysSettleDistance);
            pivot = behind + sideways * (1f - taken);
        }
        transform.position = new Vector3(pivot.x, pivot.y, transform.position.z);

        if (settled && behind.y >= LetGoPivotY) LetGo();
    }

    void Begin()
    {
        delivering = true;
        anim.speed = 0f;   // every pose is set from here
        startPivot = transform.position;
        // The push starts from the pull-back pose on screen (see Update), no further forward than where the kick begins. A
        // shooter that was never pulled pushes from the full backswing.
        int last = BackswingRock.Length - 1;
        int pulled = pullback > 0f ? Mathf.Min(last, Mathf.FloorToInt(pullback * BackswingRock.Length)) : last;
        Vector2 held = BackswingRock[pulled];
        held.y = Mathf.Min(held.y, KickRock[0].y);
        Vector2 heldAt = startPivot + (Vector2)(transform.rotation * held);

        // every shooter layer asks; the first moves the rock
        RockSlide slide = rock.GetComponent<RockSlide>();
        if (slide != null) slide.PushOff(heldAt.y, startPivot.y + (transform.rotation * DrawnRockOffset).y);

        Vector2 rockPos = rock.transform.position;
        startAlong = Vector2.Dot(rockPos - startPivot, transform.up);
        Vector2 side = transform.right;
        startGap = side * Vector2.Dot(heldAt - rockPos, side);

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// The real rock takes over from the drawn one, and the shooter becomes a free body again, carrying on at the rock's speed
    /// before easing off.
    /// </summary>
    void LetGo()
    {
        letGo = true;
        letGoTime = Time.time;
        rock.GetComponent<SpriteRenderer>().enabled = true;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.position = transform.position;
            rb.freezeRotation = true;
            rb.linearDamping = 0f;
            Rigidbody2D rockBody = rock.GetComponent<Rigidbody2D>();
            if (rockBody != null) rb.linearVelocity = rockBody.linearVelocity;
            // drag in proportion to speed, so the shooter coasts about as far on every pace
            coastDrag = rb.linearVelocity.magnitude / CoastDistance;
        }
        if (amIRock) gameObject.SetActive(false);
        else anim.Play("Shooter_2_Release", 0, 0f);
    }

    void Update()
    {

        isPressed = rockFlick.isPressed;
        springReleased = rockFlick.springReleased;
        isReleased = rockInfo.released;
        springDistance = rockFlick.springDistance;
        springDirection = rockFlick.springDirection;

        // a player holding the rock, or the computer drawing it back (see AI_Shooter.DrawBack)
        if (isPressed || (rockFlick.isPressedAI && !springReleased))
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
            Deliver();
        }

    }

}


