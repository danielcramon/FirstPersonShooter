using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
        SpawnPlayer();
        InstantiateAis();
    }

    public MouseLook mouseLookScript;
    public Player player;
    public Camera fpsCam;
    public Transform gunContainer;
    public TextMeshProUGUI bulletText;

    public Transform[] PlayerSpawnPoints;
    public Transform[] EnemySpawnPoints;
    public Enemy enemies;
    public int numberOfEnemies;

    public void InstantiateAis()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            int random = Random.Range(0, EnemySpawnPoints.Length);
            Instantiate(enemies, EnemySpawnPoints[i].position, Quaternion.identity);
        }
    }

    public void SpawnPlayer()
    {
        int random = Random.Range(0, PlayerSpawnPoints.Length);
        player.transform.position = PlayerSpawnPoints[random].position;
    }

    public void SpawnAI(Enemy enemy)
    {
        int random = Random.Range(0, EnemySpawnPoints.Length);
        enemy.transform.position = EnemySpawnPoints[random].position;

    }
}
