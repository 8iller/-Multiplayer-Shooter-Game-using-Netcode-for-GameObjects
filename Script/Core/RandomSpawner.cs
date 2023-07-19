using UnityEngine;
using Unity.Netcode;

namespace Spawner
{
    public class RandomSpawner : NetworkBehaviour
    {
        [SerializeField] Transform[] SpawnPoints;

        private int previousIndex = -1;

        public void SpawnRandomObject(GameObject Myobject)
        {   
            
            int randomIndex = Random.Range(0, SpawnPoints.Length);

            // Check if the random index is the same as the previous index
            if (randomIndex == previousIndex)
            {
                // Increment the index by 1 (modulo SpawnPoints.Length) to choose a different one
                randomIndex = (randomIndex + 1) % SpawnPoints.Length;
            }

            // Update the previous index
            previousIndex = randomIndex;

            Transform randomSpawnPoint = SpawnPoints[randomIndex];

            Myobject.transform.SetPositionAndRotation(randomSpawnPoint.position, randomSpawnPoint.rotation);
            Debug.Log("changed: " + randomIndex + "postion is "+ randomSpawnPoint.position);
            
        }   


    }
}
