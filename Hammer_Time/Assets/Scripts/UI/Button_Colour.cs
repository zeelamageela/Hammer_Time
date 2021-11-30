using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class Button_Colour : MonoBehaviour
{
    public Selectable AnySelectable;
    private PropertyInfo _selectableStateInfo = null;

    
    Button button;

    public Color colour1;
    public Color colour2;
    public Color colour3;
    public Text main;

    Vector2 mainPos1;
    Vector2 mainPos2;
    Vector2 shadowPos;

    private void Awake()
    {
        _selectableStateInfo = typeof(Selectable).GetProperty("currentSelectionState", BindingFlags.NonPublic | BindingFlags.Instance);
    }
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.image.color = colour1;

        mainPos1 = main.rectTransform.anchoredPosition;
        mainPos2 = new Vector2(main.rectTransform.anchoredPosition.x - 15f, main.rectTransform.anchoredPosition.y - 15f);
        shadowPos = new Vector2(-15f, -15f);
    }

    private void Update()
    {
        int selectableState = (int)_selectableStateInfo.GetValue(button);
        switch (selectableState)
        {
            case 0:
                //Normal Selection State
                button.image.color = colour1;
                main.rectTransform.anchoredPosition = mainPos1;
                main.color = colour1;
                main.gameObject.GetComponent<Shadow>().effectDistance = shadowPos;
                break;
            case 1:
                //Highlighted Selection State
                button.image.color = colour1;
                break;
            case 2:
                //Pressed Selection State
                button.image.color = colour2;
                main.rectTransform.anchoredPosition = mainPos2;
                main.color = colour2;
                main.gameObject.GetComponent<Shadow>().effectDistance = Vector2.zero;
                break;
            case 3:
                //Selected Selection State
                button.image.color = colour2;
                break;
            case 4:
                //Disabled Selection State
                button.image.color = colour3;
                break;
        }
    }
    public void ChangeColour()
    {
        if (button.image.color == colour1)
        {
            button.image.color = colour2;
            main.rectTransform.anchoredPosition = mainPos2;
            main.color = colour2;
            main.gameObject.GetComponent<Shadow>().effectDistance = Vector2.zero;
        }
    }
}
