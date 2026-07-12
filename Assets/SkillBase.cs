using UnityEngine;

public abstract class SkillBase
{
    protected Unit owner;
    public SkillBase(Unit u)
    {
        owner = u;
    }
    public abstract void Activate();
}
