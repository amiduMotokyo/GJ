using UnityEngine;

public class ColliderCheck : MonoBehaviour
{
    private Surface _currentSurface;    // 存储当前接触的表面
    private ContactPoint2D[] _contactPoints = new ContactPoint2D[4];    // 存储碰撞点信息的数组
    private int _contactCount;          // 实际碰撞点的数量
    
    public Surface CurrentSurface => _currentSurface;           // 获取当前表面
    public ContactPoint2D[] ContactPoints => _contactPoints;    // 获取碰撞点数组
    public int ContactCount => _contactCount;                   // 获取碰撞点数量
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        // 获取所有碰撞点的信息
        _currentSurface = col.gameObject.GetComponent<Surface>();
        _contactCount = col.GetContacts(_contactPoints);
    }
    
    private void OnCollisionExit2D(Collision2D col)
    {
        // 清空当前表面记录和碰撞点数量
        if (_currentSurface && col.gameObject.GetComponent<Surface>() == _currentSurface)
        {
            _currentSurface = null;
            _contactCount = 0;
        }
    }
    
    private void OnCollisionStay2D(Collision2D col)
    {
        // 持续更新碰撞点信息
        _contactCount = col.GetContacts(_contactPoints);
    }
}