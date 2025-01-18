using UnityEngine;

[System.Serializable]
public class BubbleLevelConfig
{
    [Header("基本移动设置")]
    public float moveForce = 5f;              // 移动力
    public float maxHorizontalSpeed = 2f;     // 水平最大速度
    public float maxUpwardSpeed = 2f;         // 向上最大速度
    public float maxDownwardSpeed = 2f;       // 向下最大速度
    public float dragFactor = 0.5f;           // 空气阻力系数
    public float buoyancyForce = 9.81f;       // 浮力大小
    public float gravityScale = 1f;           // 重力缩放
    public float minMovementSpeed = 0.2f;     // 最小保持速度
    public float dragThreshold = 0.3f;        // 开始应用阻力的速度阈值

    [Header("随机浮动设置")]
    public float randomForceInterval = 0.5f;  // 随机力施加间隔
    public float randomForceStrength = 0.1f;  // 随机力强度
    public float maxRandomOffset = 0.2f;      // 最大随机偏移距离

    [Header("干燥表面设置")]
    public float minBounceForce = 3f;         // 最小反弹力
    public float maxBounceForce = 8f;         // 最大反弹力
    public float bounceVelocityMultiplier = 0.8f;  // 速度对反弹力的影响系数

    [Header("湿润表面设置")]
    public float wetSurfaceMoveSpeed = 1f;    // 在湿润表面上的移动速度
}

[CreateAssetMenu(fileName = "BubbleConfig", menuName = "Game/BubbleConfig")]
public class BubbleConfig : ScriptableObject
{
    [Header("泡泡级别配置")]
    [SerializeField] private BubbleLevelConfig[] levelConfigs = new BubbleLevelConfig[5];
    
    // 预设每个级别的参数
    private void OnValidate()
    {
        // 确保数组长度为5
        if (levelConfigs.Length != 5)
        {
            System.Array.Resize(ref levelConfigs, 5);
        }

        // 如果是首次创建，设置默认值
        for (int i = 0; i < levelConfigs.Length; i++)
        {
            levelConfigs[i] ??= new BubbleLevelConfig();

            // 根据级别设置默认参数
            switch (i)
            {
                case 0: // 一级：布朗运动状态
                    SetBrownianMotionConfig(levelConfigs[i]);
                    break;
                case 1: // 二级：上浮状态
                    SetFloatingUpConfig(levelConfigs[i]);
                    break;
                case 2: // 三级：默认状态
                    SetDefaultConfig(levelConfigs[i]);
                    break;
                case 3: // 四级：下沉状态
                    SetSinkingConfig(levelConfigs[i]);
                    break;
                case 4: // 五级：重重状态
                    SetHeavyConfig(levelConfigs[i]);
                    break;
            }
        }
    }

    private void SetBrownianMotionConfig(BubbleLevelConfig config)
    {
        config.moveForce = 10f;
        config.maxHorizontalSpeed = 5f;
        config.maxUpwardSpeed = 5f;
        config.maxDownwardSpeed = 5f;
        config.dragFactor = 0f;
        config.buoyancyForce = 9.81f;
        config.gravityScale = 1f;
        config.minMovementSpeed = 0.5f;
        config.dragThreshold = 0f;
        config.randomForceInterval = 0.2f;
        config.randomForceStrength = 0.3f;
        config.maxRandomOffset = 0.5f;
        config.minBounceForce = 8f;
        config.maxBounceForce = 15f;
        config.bounceVelocityMultiplier = 1.2f;
        config.wetSurfaceMoveSpeed = 3f;
    }

    private void SetFloatingUpConfig(BubbleLevelConfig config)
    {
        config.moveForce = 7f;
        config.maxHorizontalSpeed = 3f;
        config.maxUpwardSpeed = 4f;
        config.maxDownwardSpeed = 2f;
        config.dragFactor = 0.3f;
        config.buoyancyForce = 12f;
        config.gravityScale = 0.8f;
        config.minMovementSpeed = 0.3f;
        config.dragThreshold = 0.2f;
        config.randomForceInterval = 0.4f;
        config.randomForceStrength = 0.15f;
        config.maxRandomOffset = 0.3f;
        config.minBounceForce = 5f;
        config.maxBounceForce = 10f;
        config.bounceVelocityMultiplier = 1f;
        config.wetSurfaceMoveSpeed = 2f;
    }

    private void SetDefaultConfig(BubbleLevelConfig config)
    {
        // 使用当前BasicMovement中的默认值
        config.moveForce = 5f;
        config.maxHorizontalSpeed = 2f;
        config.maxUpwardSpeed = 2f;
        config.maxDownwardSpeed = 2f;
        config.dragFactor = 0.5f;
        config.buoyancyForce = 9.81f;
        config.gravityScale = 1f;
        config.minMovementSpeed = 0.2f;
        config.dragThreshold = 0.3f;
        config.randomForceInterval = 0.5f;
        config.randomForceStrength = 0.1f;
        config.maxRandomOffset = 0.2f;
        config.minBounceForce = 3f;
        config.maxBounceForce = 8f;
        config.bounceVelocityMultiplier = 0.8f;
        config.wetSurfaceMoveSpeed = 1f;
    }

    private void SetSinkingConfig(BubbleLevelConfig config)
    {
        config.moveForce = 4f;
        config.maxHorizontalSpeed = 1.5f;
        config.maxUpwardSpeed = 1f;
        config.maxDownwardSpeed = 3f;
        config.dragFactor = 0.7f;
        config.buoyancyForce = 7f;
        config.gravityScale = 1.2f;
        config.minMovementSpeed = 0.15f;
        config.dragThreshold = 0.4f;
        config.randomForceInterval = 0.6f;
        config.randomForceStrength = 0.08f;
        config.maxRandomOffset = 0.15f;
        config.minBounceForce = 2f;
        config.maxBounceForce = 6f;
        config.bounceVelocityMultiplier = 0.6f;
        config.wetSurfaceMoveSpeed = 0.8f;
    }

    private void SetHeavyConfig(BubbleLevelConfig config)
    {
        config.moveForce = 2f;
        config.maxHorizontalSpeed = 1f;
        config.maxUpwardSpeed = 0.5f;
        config.maxDownwardSpeed = 5f;
        config.dragFactor = 1f;
        config.buoyancyForce = 4f;
        config.gravityScale = 1.5f;
        config.minMovementSpeed = 0.1f;
        config.dragThreshold = 0.5f;
        config.randomForceInterval = 0.8f;
        config.randomForceStrength = 0.05f;
        config.maxRandomOffset = 0.1f;
        config.minBounceForce = 1f;
        config.maxBounceForce = 4f;
        config.bounceVelocityMultiplier = 0.4f;
        config.wetSurfaceMoveSpeed = 0.5f;
    }

    // 获取指定级别的配置
    public BubbleLevelConfig GetLevelConfig(int level)
    {
        // 确保level在有效范围内（1-5）
        level = Mathf.Clamp(level, 1, 5);
        return levelConfigs[level - 1];
    }
}