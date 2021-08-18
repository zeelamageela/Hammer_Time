using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    private Queue<string> sentences;
    public Text dialogueText;
    public Text nameText;

    public Text contButton;

    public GameObject skipHead;
    public GameObject announcerHead;

    public GameObject dialogueCanvas;

    public int sentenceCount;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    private void Update()
    {
        sentenceCount = sentences.Count;
    }

    public void StartDialogue(Dialogue dialogue)
    {
        Debug.Log("Conversation with " + dialogue.name);
        nameText.text = dialogue.name;

        sentences.Clear();

        foreach(string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        Debug.Log(sentence);
        dialogueText.text = sentence;
    }

    void EndDialogue()
    {
        dialogueCanvas.SetActive(false);
        skipHead.SetActive(false);
        announcerHead.SetActive(false);
        Debug.Log("End of conversation");
    }

}
