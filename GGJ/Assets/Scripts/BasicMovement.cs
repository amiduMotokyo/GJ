using System;
using System.Collections;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    
    public event Action OnBubblePopped;                  // 泡泡破裂事件
    
    #region Variable Declarations
    
    [Header("基本移动设置")]
    [SerializeField] private float moveSpeed = 2f;       // 移动速度
    [SerializeField] private float dashForce = 5f;       // 冲刺力度
    [SerializeField] private float dashCooldown = 0.5f;  // 冲刺冷却时间
    [SerializeField] private float baseForce = 9.81f;    // 基础力（用于浮力和垂直移动）
    private bool _canDash = true;                        // 是否可以冲刺

    [Header("干燥表面设置")]
    [SerializeField] private float dryValue;              // 当前干燥值
    [SerializeField] private float maxDryValue = 5f;      // 最大干燥值
    [SerializeField] private float dryRecoveryTime = 5f;  // 干燥值恢复所需时间
    [SerializeField] private float dryBounceForce = 5f;   // 基础反弹力
    [SerializeField] private float bounceVelocityMultiplier = 0.5f;  // 速度对反弹力的影响系数
    private float _dryRecoveryTimer;                      // 干燥值恢复计时器
    private bool _canBounce = true;                       // 是否可以反弹
    
    [Header("湿润表面设置")]
    [SerializeField] private float wetSurfaceMoveSpeed = 1f;  // 在湿润表面上的移动速度
    [SerializeField] private float coyoteTime = 0.2f;         // 缓冲时间
    private bool _isAttached;                                 // 是否吸附在表面上
    private Vector2 _attachedNormal;                          // 吸附表面的法线方向
    private float _coyoteTimeCounter;                         // 缓冲时间计数器
    
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

        switch (surface.Type)
        {
            case SurfaceType.Dry:
                HandleDrySurfaceCollision(collision);
                break;
            case SurfaceType.Wet:
                HandleWetSurfaceCollision(collision);
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
        // 获取水平和垂直输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // 处理相反方向键的情况
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) horizontal = 0f;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S)) vertical = 0f;
        
        // 处理加速和移动
        float acceleration = Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1.0f;
        float targetHorizontalSpeed = horizontal * moveSpeed * acceleration;
        _rb.velocity = new Vector2(targetHorizontalSpeed, _rb.velocity.y);
        
        // 处理垂直移动
        if (vertical > 0)
        {
            _rb.AddForce(Vector2.up * (baseForce * 0.5f), ForceMode2D.Force);
        }
        else if (vertical < 0)
        {
            _rb.AddForce(Vector2.down * (baseForce * 0.3f), ForceMode2D.Force);
        }
        
        // 处理冲刺
        if (_canDash && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(PerformDash());
        }
        
        // 更新朝向
        UpdateFacingDirection(horizontal);
    }
    
    /// <summary>
    /// 执行冲刺
    /// </summary>
    private IEnumerator PerformDash()
    {
        _canDash = false;
        
        // 获取输入方向
        Vector2 dashDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        
        // 如果没有输入方向，默认向前冲刺
        if (dashDirection.magnitude < 0.1f)
        {
            dashDirection = transform.localEulerAngles.y > 90f ? Vector2.left : Vector2.right;
        }
        else
        {
            dashDirection.Normalize();
        }
        
        // 暂时关闭重力影响
        float originalGravity = _rb.gravityScale;
        _rb.gravityScale = 0f;
        
        // 应用冲刺力
        _rb.velocity = Vector2.zero;
        _rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
        PlayDashSound();
        
        // 短暂维持冲刺速度
        yield return new WaitForSeconds(0.1f);
        
        // 恢复重力
        _rb.gravityScale = originalGravity;
        
        // 冷却时间
        yield return new WaitForSeconds(dashCooldown);
        _canDash = true;
    }
    
    /// <summary>
    /// 应用浮力
    /// </summary>
    private void ApplyBuoyancy()
    {
        if (Mathf.Abs(_rb.velocity.y) > moveSpeed)
        {
            float clampedVelocityY = Mathf.Clamp(_rb.velocity.y, -moveSpeed, moveSpeed);
            _rb.velocity = new Vector2(_rb.velocity.x, clampedVelocityY);
        }
        
        _rb.AddForce(Vector2.up * baseForce);
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
            float impactVelocity = Mathf.Abs(collision.relativeVelocity.magnitude);
            // 计算反弹力
            float bounceMagnitude = impactVelocity * bounceVelocityMultiplier + dryBounceForce;
        
            // 应用反弹力
            _rb.velocity = new Vector2(_rb.velocity.x, 0.0f);
            _rb.AddForce(contact.normal * bounceMagnitude, ForceMode2D.Impulse);
        
            PlayBounceSound(impactVelocity / 10f);
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
        
        // 计算移动方向（沿着表面）
        Vector2 moveDirection = new Vector2(-_attachedNormal.y, _attachedNormal.x);
        float moveInput = Vector2.Dot(new Vector2(horizontal, vertical), moveDirection);
        
        // 应用移动
        _rb.velocity = moveDirection * (moveInput * wetSurfaceMoveSpeed);
        
        // 处理脱离
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 检查输入方向是否与表面法线方向匹配
            Vector2 inputDirection = new Vector2(horizontal, vertical).normalized;
            float dotProduct = Vector2.Dot(inputDirection, _attachedNormal);
            
            if (dotProduct > 0.7f || Mathf.Abs(moveInput) < 0.1f) // 允许一定的角度误差(Cos45°)
            {
                DetachFromSurface();
            }
        }

        // 处理图像翻转
        UpdateFacingDirection(horizontal);
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
    
    private void PlayDashSound()
    {
        float pitch = UnityEngine.Random.Range(minPitchVariation, maxPitchVariation);
        // AudioManager.Instance.PlaySound("BubbleDash", pitch);
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
