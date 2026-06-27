using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    public void GoTo(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
