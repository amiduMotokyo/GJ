using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Collider2D))]
public class FreeBubble : MonoBehaviour
{
    [Header("配置引用")]
    [SerializeField] private BubbleConfig bubbleConfig;
    private BubbleLevelConfig _currentConfig;
    
    [Header("组件引用")]
    private Rigidbody2D _rb;
    private Animator _animator;
    
    [Header("状态")]
    private bool _isDestroying = false;
    private Vector2 _floatingCenter;
    private float _randomForceTimer;
    
    [Header("吸收延迟")]
    [SerializeField] private float absorptionDelay = 0.5f;  // 发射后多久才能被吸收
    private bool _canBeAbsorbed = false;                    // 是否可以被吸收
    
    private void Awake()
    {
        // 获取组件引用
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        // 加载二级泡泡配置
        _currentConfig = bubbleConfig.GetLevelConfig(2);
        _rb.gravityScale = _currentConfig.gravityScale;
        
        // 初始化浮动中心点为当前位置
        _floatingCenter = transform.position;
        
        // 启动延迟吸收计时器
        StartCoroutine(DelayAbsorption());
    }
    
    private void FixedUpdate()
    {
        if (!_isDestroying)
        {
            ApplyBuoyancy();
            ApplyDrag();
            ApplyRandomForce();
            UpdateFloatingCenter();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isDestroying) return;

        Surface surface = collision.gameObject.GetComponent<Surface>();
        if (surface == null) return;

        switch (surface.Type)
        {
            case SurfaceType.Dry:
                HandleDrySurfaceCollision(collision);
                break;
            case SurfaceType.Dangerous:
                PopBubble();
                break;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDestroying || !_canBeAbsorbed) return;  // 吸收条件检查

        // 检查是否碰到主角泡泡
        MovementController playerBubble = other.GetComponent<MovementController>();
        if (playerBubble != null)
        {
            Debug.Log("Touched");
            // 通知主角泡泡吸收此泡泡
            playerBubble.AbsorbBubble(this);
        }
    }
    
    private void ApplyBuoyancy()
    {
        // 应用浮力
        _rb.AddForce(Vector2.up * _currentConfig.buoyancyForce);
        
        // 限制速度
        Vector2 velocity = _rb.velocity;
        velocity.y = Mathf.Clamp(velocity.y, -_currentConfig.maxDownwardSpeed, _currentConfig.maxUpwardSpeed);
        _rb.velocity = velocity;
    }
    
    private void ApplyDrag()
    {
        Vector2 velocity = _rb.velocity;
        
        if (velocity.magnitude > _currentConfig.dragThreshold)
        {
            Vector2 dragForce = -velocity.normalized * _currentConfig.dragFactor;
            
            if (velocity.magnitude < _currentConfig.minMovementSpeed * 1.5f)
            {
                dragForce *= 0.5f;
            }
            
            _rb.AddForce(dragForce);
        }
    }
    
    // 随机力相关
    private float _centerUpdateTimer;
    private const float CenterUpdateInterval = 5f;
    
    private void UpdateFloatingCenter()
    {
        _centerUpdateTimer += Time.fixedDeltaTime;
        
        if (_centerUpdateTimer >= CenterUpdateInterval)
        {
            _centerUpdateTimer = 0f;
            // 平滑地更新中心点
            _floatingCenter = Vector2.Lerp(_floatingCenter, transform.position, 0.5f);
        }
    }
    
    private void ApplyRandomForce()
    {
        _randomForceTimer += Time.deltaTime;
        
        if (_randomForceTimer >= _currentConfig.randomForceInterval)
        {
            _randomForceTimer = 0f;
            
            // 计算当前位置与浮动中心的偏移
            Vector2 offset = (Vector2)transform.position - _floatingCenter;
            float offsetDistance = offset.magnitude;
            
            Vector2 finalForce;
            if (offsetDistance < _currentConfig.maxRandomOffset * 0.5f)
            {
                // 在安全范围内，只施加随机力
                finalForce = Random.insideUnitCircle.normalized * _currentConfig.randomForceStrength;
            }
            else if (offsetDistance < _currentConfig.maxRandomOffset)
            {
                // 在过渡范围内，混合随机力和返回力
                float t = (offsetDistance - _currentConfig.maxRandomOffset * 0.5f) / 
                          (_currentConfig.maxRandomOffset * 0.5f);
            
                Vector2 randomForce = Random.insideUnitCircle.normalized * (_currentConfig.randomForceStrength * (1 - t));
                Vector2 returnForce = -offset.normalized * (_currentConfig.randomForceStrength * t);
            
                finalForce = randomForce + returnForce;
            }
            else
            {
                // 超出最大范围，只施加返回力
                finalForce = -offset.normalized * _currentConfig.randomForceStrength;
            }
        
            // 施加最终计算的力
            _rb.AddForce(finalForce, ForceMode2D.Impulse);
        }
    }
    
    private void HandleDrySurfaceCollision(Collision2D collision)
    {
        // 获取碰撞点信息
        ContactPoint2D contact = collision.GetContact(0);
        
        // 使用碰撞的相对速度
        float impactVelocity = collision.relativeVelocity.magnitude;
        
        // 判断主要的碰撞方向
        bool isHorizontalCollision = Mathf.Abs(contact.normal.x) > Mathf.Abs(contact.normal.y);
        
        // 根据碰撞面方向清除相应的速度分量
        Vector2 currentVelocity = _rb.velocity;
        if (isHorizontalCollision)
        {
            currentVelocity.x = 0f;
        }
        else
        {
            currentVelocity.y = 0f;
        }
        _rb.velocity = currentVelocity;
        
        // 根据碰撞方向选择参考速度
        float maxReferenceSpeed = isHorizontalCollision 
            ? _currentConfig.maxHorizontalSpeed
            : (contact.normal.y > 0 ? _currentConfig.maxDownwardSpeed : _currentConfig.maxUpwardSpeed);
        
        // 基于相对速度计算反弹力度
        float velocityFactor = Mathf.Clamp01(impactVelocity / maxReferenceSpeed);
        float bounceMagnitude = Mathf.Lerp(_currentConfig.minBounceForce, _currentConfig.maxBounceForce, velocityFactor);
        bounceMagnitude *= _currentConfig.bounceVelocityMultiplier;
        
        // 应用反弹力
        _rb.AddForce(contact.normal * bounceMagnitude, ForceMode2D.Impulse);

        // 播放反弹音效
        PlayBounceSound();
    }
    
    /// <summary>
    /// 初始化发射参数
    /// </summary>
    public void InitializeAsProjectile(Vector2 force)
    {
        _rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void PopBubble()
    {
        if (_isDestroying) return;
        _isDestroying = true;
    
        // 将刚体改为运动学模式，不再受物理影响
        _rb.isKinematic = true;
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0f;
    
        // 触发破裂动画
        _animator.SetTrigger("dead");
        
        // 播放破裂音效
        PlayPopSound();
        
        // 启动销毁协程
        StartCoroutine(DestroyAfterAnimation());
    }
    
    private IEnumerator DestroyAfterAnimation()
    {
        // 等待直到进入破裂动画状态
        float timeout = 0f;
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("explode") && timeout < 1f)
        {
            timeout += Time.deltaTime;
            yield return null;
        }
    
        if (timeout >= 1f)
        {
            Debug.LogWarning("等待进入破裂动画状态超时");
            Destroy(gameObject);
            yield break;
        }
    
        // 获取动画片段长度并等待播放完成
        float animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);
    
        // 销毁对象
        Destroy(gameObject);
    }

    // 用于吸收时直接销毁，跳过破裂动画
    public void DestroyImmediately()
    {
        if (_isDestroying) return;
        _isDestroying = true;
        Destroy(gameObject);
    }
    
    private IEnumerator DelayAbsorption()
    {
        yield return new WaitForSeconds(absorptionDelay);
        _canBeAbsorbed = true;
    }
    
    private void PlayBounceSound()
    {
        // AudioManager.Instance.PlaySound("FreeBubbleBounce");
    }

    private void PlayPopSound()
    {
        // AudioManager.Instance.PlaySound("FreeBubblePop");
    }
}
