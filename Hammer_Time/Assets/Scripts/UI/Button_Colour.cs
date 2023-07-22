using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Lofelt.NiceVibrations;

public class Button_Colour : MonoBehaviour
{
    public Selectable AnySelectable;
    private PropertyInfo _selectableStateInfo = null;

    AudioManager am;

    Button button;

    public Color colour1;
    public Color colour2;
    public Color colour3;
    public Text main;

    Vector2 mainPos1;
    Vector2 mainPos2;
    Vector2 shadowPos;

    public int orderNo;
    public bool writeOn;

    private void Awake()
    {
        button = GetComponent<Button>();
        _selectableStateInfo = typeof(Selectable).GetProperty("currentSelectionState", BindingFlags.NonPublic | BindingFlags.Instance);
        button.interactable = false;

        // TODO: add optional delay when to start
        if (writeOn)
        {
            string story = main.text;
            main.text = "";
            StartCoroutine(PlayText(story));
        }

        button.interactable = true;
        Debug.Log("button is interactable? - " + button.interactable);
    }

    IEnumerator PlayText(string story)
    {
        yield return new WaitForSeconds(0.125f * orderNo);

        foreach (char c in story)
        {
            main.text += c;
            yield return new WaitForSeconds(0.75f / story.Length);
        }
        yield return new WaitForSeconds(0.125f);
    }

    // Start is called before the first frame update
    void Start()
    {
        am = FindObjectOfType<AudioManager>();
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
                main.rectTransform.anchoredPosition = mainPos1;
                main.gameObject.GetComponent<Shadow>().effectDistance = shadowPos;
                break;
            case 2:
                //Pressed Selection State
                button.image.color = colour2;
                main.rectTransform.anchoredPosition = mainPos2;
                main.color = colour2;
                main.gameObject.GetComponent<Shadow>().effectDistance = Vector2.zero;
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection); 
                if (am != null)
                    am.Play("Button");
                break;
            case 3:
                //Selected Selection State
                button.image.color = colour2;
                main.rectTransform.anchoredPosition = mainPos1;
                main.gameObject.GetComponent<Shadow>().effectDistance = shadowPos;
                break;
            case 4:
                //Disabled Selection State
                button.image.color = colour3;
                main.rectTransform.anchoredPosition = mainPos1;
                main.gameObject.GetComponent<Shadow>().effectDistance = shadowPos;
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

        if (!button.isActiveAndEnabled)
            button.gameObject.SetActive(true);
        button.interactable = false;

        if (button.isActiveAndEnabled)
            StartCoroutine(ButtonWait());
        else
            button.interactable = true;
    }

    IEnumerator ButtonWait()
    {
        yield return new WaitForSeconds(1f);
        button.interactable = true;
    }

    
}
