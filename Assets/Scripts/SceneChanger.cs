using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour {

    //public string sceneSelect;
    //public int selectScene;

    static public int team;
    static public int scene;

    public int getTeam;
    public int getScene;

     public void sceneChange()
    {
        team = getTeam;
        scene = getScene;
        SceneManager.LoadScene("Caverna");
    }

    public void nextScene()
    {
        Debug.Log(scene);
        if (scene <= 5)
        {
            scene = scene + 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void prevScene()
    {
        Debug.Log(scene);
        if (scene >= 1)
        {
            scene = scene - 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void resetCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void sceneIntro()
    {
        SceneManager.LoadScene("Intro");
    }
}
