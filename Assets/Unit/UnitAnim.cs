using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnim : MonoBehaviour
{
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartIdle()
    {
        animator.Play("Idle");
    }

    public void StartAttack()
    {
        animator.Play("Attack");
    }

    public void StartSkill()
    {
        animator.Play("Skill");
    }
}
