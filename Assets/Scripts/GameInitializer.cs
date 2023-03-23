using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("Menu");
    }
}
