using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public string firstName;
    public string teamName;

    private Queue<string> sentences;
    public Text dialogueText;
    public Text nameText;

    public Text contButtonText;
    public GameObject contButton;

    public GameObject skipHead;
    public GameObject announcerHead;

    public GameObject dialogueCanvas;

    public int sentenceCount;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
        firstName = FindObjectOfType<CareerManager>().playerName;
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
        sentence = sentence.Replace("xxxxx", firstName);
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
