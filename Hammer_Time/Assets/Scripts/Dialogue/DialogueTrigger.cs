using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue[] dialogue;
    public GameObject talkingHead;

    public void TriggerDialogue(int index)
    {
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue[index]);
        talkingHead.SetActive(true);
    }
}
