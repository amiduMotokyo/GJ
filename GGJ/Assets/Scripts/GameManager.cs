using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;    // 泡泡预制体
    [SerializeField] private Transform spawnPoint;       // 重生点
    
    private GameObject _currentPlayer;
    private Vector3 _lastSpawnPosition;
    
    private void Start()
    {
        if (spawnPoint == null)
        {
            // 如果没有设置重生点，使用(0,0,0)
            _lastSpawnPosition = Vector3.zero;
        }
        else
        {
            _lastSpawnPosition = spawnPoint.position;
        }
        
        SpawnPlayer();
    }
    
    private void Update()
    {
        // 按R键重生
        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnPlayer();
        }
    }
    
    private void SpawnPlayer()
    {
        _currentPlayer = Instantiate(playerPrefab, _lastSpawnPosition, Quaternion.identity);
        
        // 获取并订阅泡泡破裂事件
        if (_currentPlayer.TryGetComponent<MovementController>(out var bubbleController))
        {
            bubbleController.OnBubblePopped += OnPlayerDied;
        }
    }
    
    private void RespawnPlayer()
    {
        if (_currentPlayer != null)
        {
            // 如果当前泡泡还存在，先销毁它
            Destroy(_currentPlayer);
        }
        SpawnPlayer();
    }
    
    private void OnPlayerDied()
    {
        // 可以在这里添加死亡效果、计分等逻辑
        Debug.Log("Player died!");
    }
}
