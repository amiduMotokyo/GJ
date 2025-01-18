using UnityEngine;

public class Surface : MonoBehaviour
{
    [SerializeField] private SurfaceType surfaceType = SurfaceType.Dry;
    
    public SurfaceType Type => surfaceType;
}