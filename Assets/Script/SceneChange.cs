using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public void ChangeMain()
    {
        SceneManager.LoadScene(1);
    }

    public void ChangeTitle()
    {
        SceneManager.LoadScene(0);
    }
}
