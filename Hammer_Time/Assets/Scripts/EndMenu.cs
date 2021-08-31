using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndMenu : MonoBehaviour
{
    GameSettingsPersist gsp;
    public Text endNumber;
    public Text redScore;
    public Text yellowScore;
    public GameObject yellowSpinner;
    public GameObject yellowSpinnerAI;

    // Start is called before the first frame update
    void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();

        if (gsp)
        {
            endNumber.text = gsp.endCurrent.ToString();
            redScore.text = gsp.redScore.ToString();
            yellowScore.text = gsp.yellowScore.ToString();

            if (gsp.aiYellow)
            {
                yellowSpinner.SetActive(false);
                yellowSpinnerAI.SetActive(true);
            }
            else
            {
                yellowSpinner.SetActive(true);
                yellowSpinnerAI.SetActive(false);
            }
        }
        
    }
}
