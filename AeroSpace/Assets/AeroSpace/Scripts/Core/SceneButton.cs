using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AeroSpace.Core
{
    [RequireComponent(typeof(Button))]
    public class SceneButton : MonoBehaviour
    {
        [SerializeField] string sceneName;

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(sceneName));
        }
    }
}
