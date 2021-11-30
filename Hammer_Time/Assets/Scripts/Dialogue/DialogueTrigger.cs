using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue[] dialogue;

    public Dialogue[] qualDialogue;
    public Dialogue[] reviewDialogue;
    public Dialogue[] introDialogue;
    public Dialogue[] storyDialogue;

    public GameObject talkingHead;

    void Awake()
    {

    }
    public void TriggerDialogue(int index)
    {
        //switch (dialogueType)
        //{
        //    case "Qualifiers":
        //        FindObjectOfType<DialogueManager>().StartDialogue(dialogue[index]);
        //        break;
        //    case "Review":
        //        FindObjectOfType<DialogueManager>().StartDialogue(dialogue[index]);
        //        break;
        //    case "Intro":
        //        FindObjectOfType<DialogueManager>().StartDialogue(dialogue[index]);
        //        break;
        //    case "Story":
        //        FindObjectOfType<DialogueManager>().StartDialogue(dialogue[index]);
        //        break;
        //}
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue[index]);
        talkingHead.SetActive(true);
    }
}
