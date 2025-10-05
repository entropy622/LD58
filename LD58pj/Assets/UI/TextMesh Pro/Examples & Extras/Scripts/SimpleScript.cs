using UnityEngine;
using System.Collections;


namespace TMPro.Examples
{
    
    public class SimpleScript : MonoBehaviour
    {
        public static SimpleScript Instance { get; private set; }

        private TextMeshPro m_textMeshPro;
        //private TMP_FontAsset m_FontAsset;

        private const string label = "The <#0050FF>count is: </color>{0:2}";
        private float m_frame;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // 可选：如果需要跨场景保留
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
           // ...existing code...
        }

        void Update()
        {
           // ...existing code...
        }
    }
}
