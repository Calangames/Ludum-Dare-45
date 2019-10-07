using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    public AnimationCurve maskGrowthSpeed;

    public Transform colorSpriteMask;

    private Vector3 maskSize;
    private bool activated;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        maskSize = colorSpriteMask.localScale;
        colorSpriteMask.localScale = Vector3.zero;
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!activated && other.CompareTag("Player"))
        {
            activated = true;
            animator.SetTrigger("Activate");
            Movement.instance.unlockedMoves = Movement.UnlockedMoves.walkJump;
            StartCoroutine(GrowColorMask());
        }
    }

    private IEnumerator GrowColorMask()
    {
        float t = 0f;
        while (colorSpriteMask.localScale != maskSize)
        {
            colorSpriteMask.localScale = Vector3.MoveTowards(colorSpriteMask.localScale, maskSize, maskGrowthSpeed.Evaluate(t) * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
    }
}
