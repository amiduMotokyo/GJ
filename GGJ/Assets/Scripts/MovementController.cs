using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class MovementController : MonoBehaviour
{
    public event Action OnBubblePopped;                  // 泡泡破裂事件
    
    #region Variable Declarations
    
    [Header("泡泡配置")]
    [SerializeField] private BubbleConfig bubbleConfig;
    [SerializeField] private int currentLevel = 3;              // 默认为3级
    private BubbleLevelConfig _currentConfig;
    private float _originGravity;                               // 原始重力系数
    
    [Header("干燥表面设置")]
    [SerializeField] private float dryValue;                    // 当前干燥值
    [SerializeField] private float maxDryValue = 5f;            // 最大干燥值
    [SerializeField] private float dryRecoveryTime = 3f;        // 干燥值恢复所需时间
    private float _dryRecoveryTimer;                            // 干燥值恢复计时器
    private bool _canBounce = true;                             // 是否可以反弹
    
    [Header("湿润表面设置")]
    [SerializeField] private float coyoteTime = 0.2f;           // 缓冲时间
    [SerializeField] private float doubleTapInterval = 0.25f;   // 双击时间间隔
    private bool _isAttached;                                   // 是否吸附在表面上
    private Vector2 _attachedNormal;                            // 吸附表面的法线方向
    private float _coyoteTimeCounter;                           // 缓冲时间计数器
    private float _lastTapTime;                                 // 上次按键时间
    private bool _isFirstTap;                                   // 是否是第一次按键
    
    // 随机浮动相关
    private Vector2 _floatingCenter;                            // 浮动中心点
    private float _randomForceTimer;                            // 随机力计时器
    private float _centerUpdateTimer;                           // 中心点更新计时器
    private const float CenterUpdateInterval = 5f;              // 中心点更新间隔
    
    // 是否存在玩家输入
    private bool _hasPlayerInput;
    
    // 组件引用
    private Animator _animator;
    private Rigidbody2D _rb;
    private ColliderCheck _colliderCheck;
    private SpriteRenderer _spriteRenderer;

    // 常数
    private const float InputThreshold = 0.1f;
    private const float DetachmentDotThreshold = 0.7f;
    private const float BounceCooldown = 0.1f;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _colliderCheck = GetComponent<ColliderCheck>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 加载当前级别的配置
        LoadLevelConfig();
        _originGravity = _rb.gravityScale;
        
        // 初始化浮动中心点为当前位置
        _floatingCenter = transform.position;
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
    
    #region Bubble Config
    
    public void SetBubbleLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, 5);
        LoadLevelConfig();
        _originGravity = _rb.gravityScale;
        UpdateVisuals();
    }
    
    private void LoadLevelConfig()
    {
        _currentConfig = bubbleConfig.GetLevelConfig(currentLevel);
        _rb.gravityScale = _currentConfig.gravityScale;
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
        
        // 更新输入状态
        _hasPlayerInput = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
        
        Vector2 inputDirection = new Vector2(horizontal, vertical).normalized;
        
        // 应用移动力
        if (inputDirection.magnitude > 0.1f)
        {
            Vector2 currentVelocity = _rb.velocity;
            
            // 分别检查水平和垂直方向的速度限制
            bool canMoveHorizontal = Mathf.Abs(currentVelocity.x) < _currentConfig.maxHorizontalSpeed;
            bool canMoveVertical = vertical > 0 
                ? currentVelocity.y < _currentConfig.maxUpwardSpeed 
                : currentVelocity.y > -_currentConfig.maxDownwardSpeed;
            
            // 根据方向应用力
            Vector2 force = Vector2.zero;
            if (canMoveHorizontal) force.x = inputDirection.x;
            if (canMoveVertical) force.y = inputDirection.y;
            
            _rb.AddForce(force * _currentConfig.moveForce);
        }
        
        // 限制速度
        Vector2 velocity = _rb.velocity;
        velocity.x = Mathf.Clamp(velocity.x, -_currentConfig.maxHorizontalSpeed, _currentConfig.maxHorizontalSpeed);
        velocity.y = Mathf.Clamp(velocity.y, -_currentConfig.maxDownwardSpeed, _currentConfig.maxUpwardSpeed);
        _rb.velocity = velocity;
        
        // 应用阻力
        ApplyDrag();
        
        // 应用随机浮动力
        ApplyRandomForce();
        
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
        if (velocity.magnitude > _currentConfig.dragThreshold)
        {
            Vector2 dragForce = -velocity.normalized * _currentConfig.dragFactor;
            
            // 如果速度接近最小速度，减小阻力
            if (velocity.magnitude < _currentConfig.minMovementSpeed * 1.5f)
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
        if (!_hasPlayerInput)
        {
            _centerUpdateTimer += Time.fixedDeltaTime;
        
            if (_centerUpdateTimer >= CenterUpdateInterval)
            {
                _centerUpdateTimer = 0f;
                // 平滑地更新中心点
                _floatingCenter = Vector2.Lerp(_floatingCenter, transform.position, 0.8f);
            }
        }
        else
        {
            // 有输入时，中心点跟随当前位置，保证停止输入时从当前位置开始浮动
            _floatingCenter = transform.position;
            _centerUpdateTimer = 0f;
        }
    }
    
    /// <summary>
    /// 应用随机浮动力
    /// </summary>
    private void ApplyRandomForce()
    {
        // 只在没有玩家输入时应用随机力
        if (_hasPlayerInput)
        {
            _randomForceTimer = 0f;
            return;
        }
        
        _randomForceTimer += Time.deltaTime;
        
        // 定期应用随机力
        if (_randomForceTimer >= _currentConfig.randomForceInterval)
        {
            _randomForceTimer = 0f;
            
            // 计算当前位置与浮动中心的偏移
            Vector2 offset = (Vector2)transform.position - _floatingCenter;
            float offsetDistance = offset.magnitude;
            
            // 根据偏移距离决定力的方向和大小
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
    
    /// <summary>
    /// 应用浮力
    /// </summary>
    private void ApplyBuoyancy()
    {
        if (!_isAttached)
        {
            // 应用浮力
            _rb.AddForce(Vector2.up * _currentConfig.buoyancyForce);
            
            // 限制速度
            Vector2 velocity = _rb.velocity;
            velocity.y = Mathf.Clamp(velocity.y, -_currentConfig.maxDownwardSpeed, _currentConfig.maxUpwardSpeed);
            _rb.velocity = velocity;
        }
    }
    
    /// <summary>
    /// 更新朝向
    /// </summary>
    private void UpdateFacingDirection(float direction)
    {
        if (direction == 0 || _isAttached) return;
    
        transform.localEulerAngles = direction < 0 
            ? new Vector3(0f, 180f, 0f)
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
            Vector2 normal = contact.normal;
            
            // 使用碰撞的相对速度
            float impactVelocity = collision.relativeVelocity.magnitude;
            
            // 判断主要的碰撞方向
            bool isHorizontalCollision = Mathf.Abs(normal.x) > Mathf.Abs(normal.y);
            // 根据碰撞方向选择参考速度
            float maxReferenceSpeed = isHorizontalCollision 
                ? _currentConfig.maxHorizontalSpeed
                : (normal.y > 0 ? _currentConfig.maxDownwardSpeed : _currentConfig.maxUpwardSpeed);
            
            // 基于相对速度计算反弹力度，并限制在最大最小值之间
            float velocityFactor = Mathf.Clamp01(impactVelocity / maxReferenceSpeed); // 将速度标准化到0-1范围
            float bounceMagnitude = Mathf.Lerp(_currentConfig.minBounceForce, _currentConfig.maxBounceForce, velocityFactor);
        
            // 应用速度影响系数
            bounceMagnitude *= _currentConfig.bounceVelocityMultiplier;
            
            // 根据碰撞面方向清除相应的速度分量
            Vector2 currentVelocity = _rb.velocity;
            if (isHorizontalCollision)
            {
                // 碰到垂直面，清除水平速度
                currentVelocity.x = 0f;
            }
            else
            {
                // 碰到水平面，清除垂直速度
                currentVelocity.y = 0f;
            }
            _rb.velocity = currentVelocity;
            
            // 应用反弹力
            _rb.AddForce(contact.normal * bounceMagnitude, ForceMode2D.Impulse);
            
            // 播放声音效果（音量基于反弹力度）
            PlayBounceSound();
            
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
        yield return new WaitForSeconds(BounceCooldown); // 设置反弹冷却时间
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
        
        ContactPoint2D contact = collision.GetContact(0);
        if (contact.normal == Vector2.zero) return; // 防止无效的法线
        
        // 设置吸附状态
        _isAttached = true;
        _attachedNormal = contact.normal;

        SpriteRotation(_attachedNormal);
        
        // 清除现有的速度和角速度
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        
        // 根据表面方向调整重力
        AdjustGravityScale(_attachedNormal);
        
        PlayWetSurfaceSound();
        ResetDryValue();
    }
    
    private void AdjustGravityScale(Vector2 normal)
    {
        // 法线与上方向的点积绝对值接近1表示水平表面，接近0表示垂直表面
        float verticalAlignment = Mathf.Abs(Vector2.Dot(normal, Vector2.up));
    
        if (verticalAlignment > 0.9f) // 水平表面
        {
            _rb.gravityScale = 0f;
        }
        else if (verticalAlignment < 0.1f) // 垂直表面
        {
            _rb.gravityScale = _originGravity * 0.5f;
        }
        else // 斜面
        {
            // 在垂直和水平之间线性插值
            // verticalAlignment为0时是0.5倍重力（垂直表面）
            // verticalAlignment为1时是0倍重力（水平表面）
            float gravityFactor = Mathf.Lerp(0.5f, 0f, verticalAlignment);
            _rb.gravityScale = _originGravity * gravityFactor;
        }
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
        _rb.velocity = moveDirection * (moveInput * _currentConfig.wetSurfaceMoveSpeed);
        
        // 检查是否需要脱离表面
        CheckDetachment(inputDirection);

        // 更新朝向
        UpdateAttachedFacingDirection(moveInput, _attachedNormal);
    }
    
    private void UpdateAttachedFacingDirection(float moveInput, Vector2 normal)
    {
        if (Mathf.Abs(moveInput) < 0.1f) return;

        // 判断表面的主要方向
        bool isVerticalSurface = Mathf.Abs(normal.x) > Mathf.Abs(normal.y);
    
        if (isVerticalSurface)
        {
            // 在垂直表面上移动时的翻转
            bool isRightSurface = normal.x > 0;
            bool shouldFlipVertically = isRightSurface ? 
                (moveInput < 0) != (normal.x > 0) : // 向右的表面：向上翻转
                (moveInput > 0) != (normal.x > 0);  // 向左的表面：向下翻转
            
            transform.localEulerAngles = new Vector3(
                shouldFlipVertically ? 180f : 0f,
                0f,
                normal.x > 0 ? -90f : 90f
            );
        }
        else
        {
            // 检查是否是向下的表面
            bool isUpsideDown = normal.y < 0;
            
            // 在水平表面上，根据左右移动翻转
            // 如果是倒立状态，需要反转移动方向的判断
            bool shouldFlipHorizontally = isUpsideDown ? 
                (moveInput < 0) != (normal.y < 0) : 
                (moveInput > 0) != (normal.y < 0);
            
            transform.localEulerAngles = new Vector3(
                0f,
                shouldFlipHorizontally ? 180f : 0f,
                normal.y > 0 ? 0f : 180f
            );
        }
    }
    
    /// <summary>
    /// 检查是否需要从湿润表面脱离
    /// </summary>
    /// <param name="inputDirection">玩家的输入方向向量</param>
    private void CheckDetachment(Vector2 inputDirection)
    {
        // 计算输入方向与表面法线的点积，用于判断输入是否指向远离表面的方向
        // 点积为正表示输入方向与法线方向大致相同（指向远离表面的方向）
        float dotProduct = Vector2.Dot(inputDirection, _attachedNormal);
    
        // 如果没有足够的输入强度，直接返回
        if (inputDirection.magnitude < InputThreshold) return;
    
        // 检测与输入方向对应的按键是否被按下
        bool keyPressed = (inputDirection.x != 0 && Input.GetKeyDown(inputDirection.x > 0 ? KeyCode.D : KeyCode.A)) ||
                          (inputDirection.y != 0 && Input.GetKeyDown(inputDirection.y > 0 ? KeyCode.W : KeyCode.S));
    
        // 当输入方向足够接近法线方向（点积>阈值）且对应按键被按下时
        if (dotProduct > DetachmentDotThreshold && keyPressed)
        {
            float currentTime = Time.time;
        
            // 如果是第一次按键
            if (!_isFirstTap)
            {
                _isFirstTap = true;
                _lastTapTime = currentTime;
            }
            // 如果是双击（在规定时间间隔内的第二次按键）
            else if (currentTime - _lastTapTime <= doubleTapInterval)
            {
                DetachFromSurface();  // 脱离表面
                _isFirstTap = false;  // 重置第一次按键状态
            }
            // 如果超过了双击时间间隔，视为新的第一次按键
            else
            {
                _lastTapTime = currentTime;
            }
        }
        // 如果按下了不符合脱离条件的按键，重置第一次按键状态
        else if (keyPressed)
        {
            _isFirstTap = false;
        }
    }
    
    /// <summary>
    /// 从表面脱离
    /// </summary>
    private void DetachFromSurface()
    {
        // 根据表面法线判断应该触发哪种跳跃动画
        bool isVerticalSurface = Mathf.Abs(_attachedNormal.x) > Mathf.Abs(_attachedNormal.y);
        
        if (isVerticalSurface)
        {
            // 在垂直表面上，触发垂直跳跃
            _animator.SetTrigger("verticaljump");
        }
        else
        {
            // 在水平表面上，触发水平跳跃
            _animator.SetTrigger("horizontaljump");
        }
        
        _isAttached = false;
        _rb.gravityScale = _originGravity;
        
        // 获取当前的法线方向，用于决定脱离后的朝向
        bool shouldFaceLeft = _attachedNormal.x > 0;
        transform.localEulerAngles = shouldFaceLeft ? 
            new Vector3(0f, 180f, 0f) : 
            Vector3.zero;
        
        PlayJumpSound();
    }
    
    #endregion

    #region Public Methods
    
    public int GetCurrentLevel() => currentLevel;
    
    #endregion
    
    #region Visual and Audio
    
    private void UpdateVisuals()
    {
        // TODO: 根据当前级别更新Sprite和动画
        //_animator.SetInteger("BubbleLevel", currentLevel);
    }
    
    /// <summary>
    /// 更新动画状态
    /// </summary>
    private void UpdateAnimations()
    {
        _animator.SetBool("onground", _isAttached);
    }
    
    /// <summary>
    /// 更新干燥状态对应的Sprite
    /// </summary>
    private void UpdateDrySprite()
    {
        // TODO: 根据干燥值更新Sprite
        Debug.Log("干燥阶段: " + dryValue);
    }
    
    private void SpriteRotation(Vector2 normal)
    {
        // 计算法线向量与上方向的夹角
        float angle = Vector2.SignedAngle(Vector2.up, normal);
        transform.localEulerAngles = new Vector3(0f, 0f, angle);
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
    
    private void PlayBounceSound()
    {
        // AudioManager.Instance.PlaySound("BubbleBounce", pitch);
    }

    private void PlayJumpSound()
    {
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