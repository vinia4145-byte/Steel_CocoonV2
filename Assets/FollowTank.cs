using UnityEngine;

public class FollowTank : MonoBehaviour
{
    public FogGrid fog;
    public Transform tank;

    void Update()
    {
        if (fog != null && tank != null)
            fog.RevealFog(tank.position, fog.revealRadius);
    }
}
