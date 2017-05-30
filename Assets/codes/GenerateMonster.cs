using UnityEngine;
using System.Collections;

public class GenerateMonster : MonoBehaviour {

    public GameObject monsterPrefab;

	// Use this for initialization
	void Start () {
	
	}

    public float SpawnRate=2.0f;
    private float LastSpawnTime;
	// Update is called once per frame
	void Update () {
        if (Time.time > LastSpawnTime + SpawnRate)
        {
            LastSpawnTime = Time.time;
            CreateMonster();
        }
	}
    public float SpawnRadius=15.0f;

    public void CreateMonster()
    {
        Vector2 _pos2D = Random.insideUnitCircle * SpawnRadius;
        Vector3 _pos3D = new Vector3(_pos2D.x, 0, _pos2D.y);
        Debug.Log(_pos3D);
        Instantiate(monsterPrefab, _pos3D, Quaternion.identity);
    }
}
