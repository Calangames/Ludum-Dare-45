using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Movement : MonoBehaviour
{
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D rb;
    private AnimationScript anim;

    public static Movement instance = null;

    [Space]
    [Header("Stats")]
    public float speed = 10f;
    public float jumpForce = 50f;
    public float knockbackForce = 4f;
    public float slideSpeed = 5f;
    public float wallJumpLerp = 10f;
    public float dashSpeed = 20f;
    public int life = 3;
    public UnlockedMoves unlockedMoves = UnlockedMoves.walk;

    public enum UnlockedMoves
    {
        walk,
        walkJump,
        walkJumpWall,
        walkJumpWallDash,
        walkJumpWallDashDoubleJump
    }

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;
    public bool knockback;
    public bool dead;

    [Space]

    private bool groundTouch;
    private bool hasDashed;
    private bool recentlyKnockbacked;
    private int remainingJumps;

    [Space]

    public int side = 1;

    [Space]
    [Header("Polish")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;

    private BetterJumping betterJumping;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();
        betterJumping = GetComponent<BetterJumping>();
        remainingJumps = (int)unlockedMoves == 4 ? 2 : 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (knockback)
        {
            if (coll.onGround && !recentlyKnockbacked)
            {
                GroundTouch();
                dead = life <= 0;
                knockback = false;
            }
            return;
        }
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        Walk(dir);
        anim.SetHorizontalMovement(x, y, rb.velocity.y);

        if (coll.onGround && !isDashing)
        {
            wallJumped = false;
            betterJumping.enabled = true;
        }

        if((int)unlockedMoves >= 2 && coll.onWall && !coll.onGround)
        {
            if (x != 0)
            {
                wallSlide = true;

                WallSlide();
                remainingJumps = 1;
            }            
        }

        if (!coll.onWall || coll.onGround)
            wallSlide = false;

        if ((int)unlockedMoves >= 1 && Input.GetButtonDown("Jump"))
        {
            anim.SetTrigger("jump");

            if (coll.onGround || ((int)unlockedMoves >= 4 && !coll.onWall && remainingJumps > 0))
            {
                Jump(Vector2.up, false);
                remainingJumps --;
            }
            else if ((int)unlockedMoves >= 2 && coll.onWall && remainingJumps > 0)
            {
                WallJump();
                remainingJumps = (int)unlockedMoves == 4 ? 1 : 0;
            }
        }

        if ((int)unlockedMoves >= 3 && Input.GetButtonDown("Fire1") && !hasDashed)
        {
            if(xRaw != 0)
                Dash(xRaw);
        }

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if(!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        WallParticle(y);

        if (wallSlide || !canMove)
            return;

        if(x > 0)
        {
            side = 1;
            anim.Flip(side);
        }
        if (x < 0)
        {
            side = -1;
            anim.Flip(side);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hazard") && !knockback)
        {
            StartCoroutine(KnockbackCooldown(0.1f));
            knockback = true;
            life--;
            rb.velocity = Vector2.zero;
            rb.velocity = new Vector2(Mathf.Sign(transform.position.x - other.transform.position.x), 4f) * knockbackForce;
        }
    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;
        remainingJumps = (int)unlockedMoves == 4 ? 2 : 1;

        side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();
    }

    private void Dash(float x)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        hasDashed = true;

        anim.SetTrigger("dash");

        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, 0f);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        FindObjectOfType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        dashParticle.Play();
        rb.gravityScale = 0;
        betterJumping.enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        dashParticle.Stop();
        rb.gravityScale = 3;
        betterJumping.enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (coll.onGround)
            hasDashed = false;
    }

    private void WallJump()
    {
        anim.SetTrigger("wallJump");
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            anim.Flip(side);
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        Jump(((Vector2.up) + wallDir / 1.5f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {
        if(coll.wallSide != side)
         anim.Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = false;
        if((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(push, -slideSpeed);
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce;

        particle.Play();
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    IEnumerator KnockbackCooldown(float time)
    {
        recentlyKnockbacked = true;
        yield return new WaitForSeconds(time);
        recentlyKnockbacked = false;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    void WallParticle(float vertical)
    {
        var main = slideParticle.main;

        if (wallSlide || (vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }
}
