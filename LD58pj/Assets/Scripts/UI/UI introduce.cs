using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UIintroduce : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnPointerEnter(PointerEventData eventData)

    {
        //给这个物体添加一个文本对象
        GameObject textObj = new GameObject("WorldText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, -50, 0);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = gettext();
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        //设置成白色
        text.color = Color.white;

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 删除文本对象
        GameObject textObj = GameObject.Find("WorldText");
        if (textObj != null)
        {
            Destroy(textObj);
        }

    }
    
    private string gettext()
    {
        string name = gameObject.name;
        switch (name)
        {
            case "Movement":
                return "Movement: A basic ability: Move left and right";
            case "Jump":
                return "Jump: A basic ability: Jump up";
            case "IronBlock":
                return "IronBlock: ??";
            case "IceBlock":
                return "IceBlock: Increase your speed";
            case "GravityFlip":
                return "GravityFlip:press G to Flip gravity to walk on the ceiling";
            case "Dash":
                return "Dash: Quickly dash in the direction you are facing";
            case "Balloon":
                return "Balloon: Press Space to create a balloon to slow your fall";
            case "DoubleJump":
                return "DoubleJump: Jump again while in the air";    
            default:
                return "";
        }
    }
    
    
}
