using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public string firstName;
    public string teamName;
    public int provRank;
    public float earnings;
    public int tourRank;
    public float tourPoints;

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
        CareerManager cm = FindObjectOfType<CareerManager>();
        firstName = cm.playerName;
        
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

        CareerManager cm = FindObjectOfType<CareerManager>();
        for (int i = 0; i < cm.provRankList.Count; i++)
        {
            if (cm.playerTeamIndex == cm.provRankList[i].team.id)
            {
                provRank = i + 1;
                earnings = cm.provRankList[i].team.earnings;
            }
        }
        for (int i = 0; i < cm.tourRankList.Count; i++)
        {
            if (cm.playerTeamIndex == cm.tourRankList[i].team.id)
            {
                tourRank = i + 1;
                tourPoints = cm.tourRankList[i].team.tourPoints;
            }
        }
        sentence = sentence.Replace("xxxxx", firstName); 
        if (provRank == 1)
            sentence = sentence.Replace("PROVRANK", provRank.ToString() + "st");
        else if (provRank == 2)
            sentence = sentence.Replace("PROVRANK", provRank.ToString() + "nd");
        else if (provRank == 3)
            sentence = sentence.Replace("PROVRANK", provRank.ToString() + "rd");
        else
            sentence = sentence.Replace("PROVRANK", provRank.ToString() + "th");

        if (tourRank == 1)
            sentence = sentence.Replace("TOURRANK", tourRank.ToString() + "st");
        else if (tourRank == 2)
            sentence = sentence.Replace("TOURRANK", tourRank.ToString() + "nd");
        else if (tourRank == 3)
            sentence = sentence.Replace("TOURRANK", tourRank.ToString() + "rd");
        else
            sentence = sentence.Replace("TOURRANK", tourRank.ToString() + "th");

        sentence = sentence.Replace("EARNINGS", "$" + earnings.ToString("n0"));
        sentence = sentence.Replace("TOURPOINTS", tourPoints.ToString());

        sentence = sentence.Replace("TEAMNAME", cm.teamName);
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
