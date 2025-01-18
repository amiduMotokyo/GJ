using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class BasicMovement : MonoBehaviour
{
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    
    public event Action OnBubblePopped;                  // 泡泡破裂事件
    
    #region Variable Declarations
    
    [Header("基本移动设置")]
    [SerializeField] private float moveForce = 5f;              // 移动力
    [SerializeField] private float maxSpeed = 2f;               // 最大速度
    [SerializeField] private float dragFactor = 0.5f;           // 空气阻力系数
    [SerializeField] private float buoyancyForce = 9.81f;       // 浮力大小
    [SerializeField] private float minMovementSpeed = 0.2f;     // 最小保持速度
    [SerializeField] private float dragThreshold = 0.3f;        // 开始应用阻力的速度阈值
    
    [Header("随机浮动设置")]
    [SerializeField] private float randomForceInterval = 0.5f;  // 随机力施加间隔
    [SerializeField] private float randomForceStrength = 0.1f;  // 随机力强度
    [SerializeField] private float maxRandomOffset = 0.2f;      // 最大随机偏移距离
    private Vector2 _floatingCenter;                            // 浮动中心点
    private float _randomForceTimer;                            // 随机力计时器
    private float _centerUpdateTimer;                           // 中心点更新计时器
    private const float CenterUpdateInterval = 5f;              // 中心点更新间隔
    
    [Header("干燥表面设置")]
    [SerializeField] private float dryValue;                    // 当前干燥值
    [SerializeField] private float maxDryValue = 5f;            // 最大干燥值
    [SerializeField] private float dryRecoveryTime = 3f;        // 干燥值恢复所需时间
    [SerializeField] private float minBounceForce = 3f;         // 最小反弹力
    [SerializeField] private float maxBounceForce = 8f;         // 最大反弹力
    [SerializeField] private float bounceVelocityMultiplier = 0.8f;  // 速度对反弹力的影响系数
    private float _dryRecoveryTimer;                      // 干燥值恢复计时器
    private bool _canBounce = true;                       // 是否可以反弹
    
    [Header("湿润表面设置")]
    [SerializeField] private float wetSurfaceMoveSpeed = 1f;  // 在湿润表面上的移动速度
    [SerializeField] private float coyoteTime = 0.2f;         // 缓冲时间
    [SerializeField] private float doubleTapInterval = 0.25f; // 双击时间间隔
    private bool _isAttached;                                 // 是否吸附在表面上
    private Vector2 _attachedNormal;                          // 吸附表面的法线方向
    private float _coyoteTimeCounter;                         // 缓冲时间计数器
    private float _lastTapTime;                               // 上次按键时间
    private bool _isFirstTap;                                 // 是否是第一次按键
    
    [Header("音效设置")]
    [SerializeField] private float minPitchVariation = 0.9f;    // 最小音调变化
    [SerializeField] private float maxPitchVariation = 1.1f;    // 最大音调变化
    
    // 组件引用
    private Animator _animator;
    private Rigidbody2D _rb;
    private ColliderCheck _colliderCheck;
    private SpriteRenderer _spriteRenderer;

    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _colliderCheck = GetComponent<ColliderCheck>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimations();
        DryValueRecover();
    }
    
    private void FixedUpdate()
    {
        if (!_isAttached)
        {
            ApplyBuoyancy();
            UpdateFloatingCenter();
        }
        else
        {
            StayAttached();
        }
    }
    
    /// <summary>
    /// 处理碰撞进入事件
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Surface surface = collision.gameObject.GetComponent<Surface>();
        if (surface == null) return;

        // 如果当前在吸附状态且碰到干燥表面，立即解除吸附
        if (_isAttached && surface.Type == SurfaceType.Dry)
        {
            DetachFromSurface();
        }
        
        switch (surface.Type)
        {
            case SurfaceType.Dry:
                HandleDrySurfaceCollision(collision);
                break;
            case SurfaceType.Wet:
                // 只有在未吸附状态下才处理新的湿润表面
                if (!_isAttached)
                {
                    HandleWetSurfaceCollision(collision);
                }
                break;
            case SurfaceType.Dangerous:
                // 泡泡破裂
                PopBubble();
                break;
        }
    }
    
    #endregion
    
    #region Movement System
    
    /// <summary>
    /// 处理移动输入
    /// </summary>
    private void HandleInput()
    {
        if (_isAttached)
        {
            HandleAttachedMovement();
        }
        else
        {
            HandleNormalMovement();
        }
    }
    
    /// <summary>
    /// 处理正常移动
    /// </summary>
    private void HandleNormalMovement()
    {
        // 获取输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // 处理相反方向键的情况
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) horizontal = 0f;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S)) vertical = 0f;
        
        Vector2 inputDirection = new Vector2(horizontal, vertical).normalized;
        
        // 应用移动力
        if (inputDirection.magnitude > 0.1f)
        {
            Vector2 currentVelocity = _rb.velocity;
            float currentSpeed = currentVelocity.magnitude;
            
            if (currentSpeed < maxSpeed)
            {
                _rb.AddForce(inputDirection * moveForce);
            }
        }
        
        // 应用阻力
        ApplyDrag();
        
        // 应用随机浮动力
        ApplyRandomForce();
        
        // 限制速度
        Vector2 velocity = _rb.velocity;
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
            _rb.velocity = velocity;
        }
        
        // 更新朝向
        UpdateFacingDirection(horizontal);
    }
    
    /// <summary>
    /// 应用阻力
    /// </summary>
    private void ApplyDrag()
    {
        Vector2 velocity = _rb.velocity;
        
        // 只在没有对应方向的输入时应用阻力
        if (velocity.magnitude > dragThreshold)
        {
            Vector2 dragForce = -velocity.normalized * dragFactor;
            
            // 如果速度接近最小速度，减小阻力
            if (velocity.magnitude < minMovementSpeed * 1.5f)
            {
                dragForce *= 0.5f;
            }
            
            _rb.AddForce(dragForce);
        }
    }
    
    /// <summary>
    /// 更新浮动中心
    /// </summary>
    private void UpdateFloatingCenter()
    {
        // 只在没有玩家输入时更新浮动中心点
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.1f && 
            Mathf.Abs(Input.GetAxisRaw("Vertical")) < 0.1f)
        {
            _centerUpdateTimer += Time.fixedDeltaTime;
            
            if (_centerUpdateTimer >= CenterUpdateInterval)
            {
                _centerUpdateTimer = 0f;
                _floatingCenter = transform.position;
            }
        }
        else
        {
            // 有输入时重置计时器
            _centerUpdateTimer = 0f;
        }
    }
    
    /// <summary>
    /// 应用随机浮动力
    /// </summary>
    private void ApplyRandomForce()
    {
        _randomForceTimer += Time.deltaTime;
        
        // 定期应用随机力
        if (_randomForceTimer >= randomForceInterval)
        {
            _randomForceTimer = 0f;
            
            // 计算当前位置与初始位置的偏移
            Vector2 offset = (Vector2)transform.position - _floatingCenter;
            
            // 生成随机方向的力
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector2 finalForce = randomDirection * randomForceStrength;
            
            // 如果偏离中心点太远，添加一个返回力
            if (offset.magnitude > maxRandomOffset)
            {
                Vector2 returnDirection = (_floatingCenter - (Vector2)transform.position).normalized;
                finalForce += returnDirection * randomForceStrength;
            }
            
            _rb.AddForce(finalForce, ForceMode2D.Impulse);
        }
    }
    
    /// <summary>
    /// 应用浮力
    /// </summary>
    private void ApplyBuoyancy()
    {
        if (!_isAttached)
        {
            // 应用浮力
            _rb.AddForce(Vector2.up * 9.81f);
            
            // 限制速度
            Vector2 velocity = _rb.velocity;
            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
                _rb.velocity = velocity;
            }
        }
    }
    
    /// <summary>
    /// 更新朝向
    /// </summary>
    private void UpdateFacingDirection(float direction)
    {
        if (direction == 0) return;
        
        transform.localEulerAngles = direction < 0 
            ? new Vector3(0.0f, 180.0f, 0.0f)
            : Vector3.zero;
    }
    
    #endregion
    
    #region Dry Surface System
    
    /// <summary>
    /// 处理与干燥表面的碰撞
    /// </summary>
    private void HandleDrySurfaceCollision(Collision2D collision)
    {
        // 增加干燥值
        dryValue = Mathf.Min(dryValue + 1f, maxDryValue);
        UpdateDrySprite();
        _dryRecoveryTimer = 0f;
        
        if (dryValue >= maxDryValue)
        {
            PopBubble();
            return;
        }

        // 处理反弹效果
        if (_canBounce)
        {
            // 获取碰撞点信息
            ContactPoint2D contact = collision.GetContact(0);
            
        
            // 使用碰撞的相对速度
            float impactVelocity = collision.relativeVelocity.magnitude;
            
            // 基于相对速度计算反弹力度，并限制在最大最小值之间
            float velocityFactor = Mathf.Clamp01(impactVelocity / maxSpeed); // 将速度标准化到0-1范围
            float bounceMagnitude = Mathf.Lerp(minBounceForce, maxBounceForce, velocityFactor);
        
            // 应用速度影响系数
            bounceMagnitude *= bounceVelocityMultiplier;
            
            // 清除原有的垂直速度
            Vector2 currentVelocity = _rb.velocity;
            _rb.velocity = new Vector2(currentVelocity.x, 0f);
            
            // 应用反弹力
            _rb.AddForce(contact.normal * bounceMagnitude, ForceMode2D.Impulse);
            
            // 播放声音效果（音量基于反弹力度）
            PlayBounceSound(velocityFactor);
            
            // 开始反弹冷却
            StartCoroutine(BounceDelay());
        }
    }
    
    /// <summary>
    /// 处理干燥值
    /// </summary>
    private void DryValueRecover()
    {
        // 只在不接触任何表面时恢复干燥值
        if (!_colliderCheck.CurrentSurface)
        {
            _dryRecoveryTimer += Time.deltaTime;
        
            // 到达恢复时间时减少干燥值
            if (_dryRecoveryTimer >= dryRecoveryTime && dryValue > 0)
            {
                dryValue = Mathf.Max(0, dryValue - 1f);
                _dryRecoveryTimer = 0;
                UpdateDrySprite();
            }
        }
    }
    
    /// <summary>
    /// 重置干燥值
    /// </summary>
    private void ResetDryValue()
    {
        dryValue = 0f;
        _dryRecoveryTimer = 0f;
        UpdateDrySprite();
    }
    
    /// <summary>
    /// 反弹延迟协程
    /// </summary>
    private IEnumerator BounceDelay()
    {
        _canBounce = false;
        yield return new WaitForSeconds(0.1f); // 设置反弹冷却时间
        _canBounce = true;
    }
    
    #endregion
    
    #region Wet Surface System
    
    /// <summary>
    /// 处理与湿润表面的碰撞
    /// </summary>
    private void HandleWetSurfaceCollision(Collision2D collision)
    {
        if (_isAttached) return; // 已经吸附则不处理
        
        // 设置吸附状态
        _isAttached = true;
        _attachedNormal = collision.GetContact(0).normal;
        
        // 清除现有的速度和角速度
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        
        // 改变重力缩放以减弱重力影响
        _rb.gravityScale /= 2f;
        
        PlayWetSurfaceSound();
        ResetDryValue();
    }
    
    /// <summary>
    /// 保持吸附状态
    /// </summary>
    private void StayAttached()
    {
        if (!_isAttached) return;

        // 检查是否还有碰撞点
        if (_colliderCheck.ContactCount > 0)
        {
            _attachedNormal = _colliderCheck.ContactPoints[0].normal;
            _coyoteTimeCounter = coyoteTime;  // 重置缓冲时间
        }
        else
        {
            // 开始缓冲时间计数
            _coyoteTimeCounter -= Time.fixedDeltaTime;
        
            // 缓冲时间结束且确实离开表面时才解除吸附
            if (_coyoteTimeCounter <= 0 && 
                (!_colliderCheck.CurrentSurface || _colliderCheck.CurrentSurface.Type != SurfaceType.Wet))
            {
                DetachFromSurface();
            }
        }
    }
    
    /// <summary>
    /// 处理吸附状态下的移动
    /// </summary>
    private void HandleAttachedMovement()
    {
        // 获取输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // 创建输入向量并标准化
        Vector2 inputDirection = new Vector2(horizontal, vertical).normalized;
        
        // 计算沿表面的移动方向（垂直于法线）
        Vector2 moveDirection = new Vector2(-_attachedNormal.y, _attachedNormal.x);
        float moveInput = Vector2.Dot(inputDirection, moveDirection);
        
        // 应用移动
        _rb.velocity = moveDirection * (moveInput * wetSurfaceMoveSpeed);
        
        // 检查是否需要脱离表面
        CheckDetachment(inputDirection);
        
        // 处理图像翻转
        UpdateFacingDirection(horizontal);
    }
    
    private void CheckDetachment(Vector2 inputDirection)
    {
        // 检查输入方向是否与表面法线方向匹配
        float dotProduct = Vector2.Dot(inputDirection, _attachedNormal);
        bool keyPressed = false;
        
        // 检测按键
        if (inputDirection.x != 0)
        {
            keyPressed = inputDirection.x > 0 ? Input.GetKeyDown(KeyCode.D) : Input.GetKeyDown(KeyCode.A);
        }
        if (inputDirection.y != 0)
        {
            keyPressed = keyPressed || (inputDirection.y > 0 ? Input.GetKeyDown(KeyCode.W) : Input.GetKeyDown(KeyCode.S));
        }
        
        if (dotProduct > 0.7f && keyPressed) // 允许一定的角度误差
        {
            float currentTime = Time.time;
            
            if (!_isFirstTap)
            {
                _isFirstTap = true;
                _lastTapTime = currentTime;
            }
            else
            {
                // 检查是否在时间间隔内进行了第二次按键
                if (currentTime - _lastTapTime <= doubleTapInterval)
                {
                    DetachFromSurface();
                    _isFirstTap = false;
                }
                else
                {
                    // 超出时间窗口，重置为第一次按键
                    _lastTapTime = currentTime;
                }
            }
        }
        else if (!keyPressed)
        {
            // 保持当前的双击状态
            return;
        }
        else
        {
            // 输入方向改变，重置双击状态
            _isFirstTap = false;
        }
    }
    
    /// <summary>
    /// 从表面脱离
    /// </summary>
    private void DetachFromSurface()
    {
        _isAttached = false;
        _rb.gravityScale = 1f;
        
        PlayJumpSound();
    }
    
    #endregion
    
    #region Visual and Audio
    
    /// <summary>
    /// 更新动画状态
    /// </summary>
    private void UpdateAnimations()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        _animator.SetBool(Horizontal, horizontal != 0.0f);
        
    }
    
    /// <summary>
    /// 更新干燥状态对应的Sprite
    /// </summary>
    private void UpdateDrySprite()
    {
        // TODO: 根据干燥值更新Sprite
        Debug.Log("干燥阶段: " + dryValue);
    }
    
    /// <summary>
    /// 处理泡泡破裂效果
    /// </summary>
    private void PopBubble()
    {
        PlayPopSound();
        OnBubblePopped?.Invoke();
        gameObject.SetActive(false);
    }
    
    private void PlayBounceSound(float velocityFactor)
    {
        // 根据撞击强度调整音调
        float pitch = Mathf.Lerp(minPitchVariation, maxPitchVariation, velocityFactor);
        // AudioManager.Instance.PlaySound("BubbleBounce", pitch);
    }

    private void PlayJumpSound()
    {
        float pitch = UnityEngine.Random.Range(minPitchVariation, maxPitchVariation);
        // AudioManager.Instance.PlaySound("BubbleJump", pitch);
    }

    private void PlayWetSurfaceSound()
    {
        // AudioManager.Instance.PlaySound("BubbleWet");
    }

    private void PlayPopSound()
    {
        // AudioManager.Instance.PlaySound("BubblePop");
    }
    
    #endregion
}
