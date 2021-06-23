using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    #region Gamestates
    enum Gamestate
    {
        Idle,
        Intro,
        IntroIdle,
        ValueAccepted,
        ValueDenied,
        CodeBreakerThink,
        CodeBreakerTry,
        CodeMakerAnswer,
        Win
    }
    #endregion

    #region Objects

    // Codemaker
    public GameObject codemakerBackground;
    public Text codemakerText;
    public InputField codemakerInputField;

    // Codebreaker
    public GameObject codebreakerBackground;
    public Text codebreakerText;

    // Game Mode
    public GameObject playButton, demoButton;

    // Number
    public Text UINumberGuessed0, UINumberGuessed1, UINumberGuessed2, UINumberGuessed3, UINumberToGuess0, UINumberToGuess1, UINumberToGuess2, UINumberToGuess3;

    #endregion

    #region Variables
    Gamestate gamestate;
    bool demo = false;
    float deltaTime = 0f;
    float waitingTime = 0.5f;
    int[] codeMakerValue = { -1, -1, -1, -1};
    int[] codeBreakerAttempt = { -1, -1, -1, -1};
    int[] codeBreakerFinalAttemp = { -1, -1, -1, -1};
    int bulls, cows, previousAttemptBulls, previousAttemptCows, finalBulls, codeBreakerSideCounter, codeBreakerUpCounter;
        
    #endregion

    #region Start

    void Start()
    {
        gamestate = Gamestate.Idle;
        bulls = 0;
        cows = 0;
        previousAttemptBulls = 0;
        previousAttemptCows = 0;
        codeBreakerSideCounter = 0;
        codeBreakerUpCounter = 0;
        finalBulls = 0;
    }

    #endregion

    #region Update

    void Update()
    {
        if(gamestate == Gamestate.Intro)
        {
            deltaTime += Time.deltaTime;
            if(deltaTime >= 2 * waitingTime)
            {
                SetIntroIdle();
            }
        }
        else if(gamestate == Gamestate.ValueAccepted)
        {
            deltaTime += Time.deltaTime;
            if(deltaTime >= 1 * waitingTime || demo == true)
            {
                SetCodeBreakerThink();
            }
        }
        else if(gamestate == Gamestate.ValueDenied)
        {
            deltaTime += Time.deltaTime;
            if(deltaTime >= 2 * waitingTime)
            {
                SetIntroIdle();
            }
        }
        else if(gamestate == Gamestate.CodeBreakerThink)
        {
            deltaTime += Time.deltaTime;
            if(deltaTime >= 3 * waitingTime || demo == true)
            {
                SetCodeBreakerTry();
            }
            else if(deltaTime >= 2 * waitingTime)
            {
                SetCodeBreakerMessage("Hmm... let me think...");
            }
            else if(deltaTime >= 1 * waitingTime)
            {
                SetCodeBreakerMessage("Hmm... let me think..");
            }
        }  
        else if(gamestate == Gamestate.CodeBreakerTry)
        {
            deltaTime += Time.deltaTime;
            if(deltaTime >= 2 * waitingTime || demo == true)
            {
                SetCodeMakerAnswer();
            }
        }  
        else if(gamestate == Gamestate.CodeMakerAnswer)
        {
            deltaTime += Time.deltaTime;
            if(deltaTime >= 2 * waitingTime || demo == true)
            {
                SetCodeBreakerThink();
            }
        }
        else if(gamestate == Gamestate.Win)
        {
            deltaTime += Time.deltaTime;
            if(deltaTime >= 5 * waitingTime)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
            }
        }  
    }

    #endregion
    
    // Game Mode
        
    #region StartNormal
    public void StartNormal()
    {
        playButton.SetActive(false);
        demoButton.SetActive(false);
        demo = false;
        deltaTime = 0f;
        gamestate = Gamestate.Intro;
    }
    #endregion
        
    #region StartDemo
    public void StartDemo()
    {
        playButton.SetActive(false);
        demoButton.SetActive(false);
        demo = true;
        for (int i = 0; i < 4; i++)
        {
            codeMakerValue[i] = GenerateValidDigit(codeMakerValue);
        }
        SetValueAccepted();
    }
    #endregion

    // Game Control Functions

    #region SetIntroIdle
    void SetIntroIdle()
    {
        gamestate = Gamestate.IntroIdle;
        SetCodeBreakerMessage("Insert a four-digit number where each digit can only appear once in the number. Example: 3752.");
        codemakerBackground.SetActive(true);
        codemakerInputField.gameObject.SetActive(true);
    }
    #endregion

    #region ValidateValue
    public void ValidateValue()
    {
        if(gamestate == Gamestate.IntroIdle)
        {
            string temp_str = codemakerInputField.text;

            if(temp_str.Length < 4 || temp_str.Length > 4)
            {
                SetValueDenied();
                return;
            }
            for (int i = 0; i < 4; i++)
            {
                codeMakerValue[i] = (int)Char.GetNumericValue(temp_str[i]);
                if(codeMakerValue[i] < 0)
                {
                    SetValueDenied();
                    return;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if(i != j)
                    {
                        if(codeMakerValue[i] == codeMakerValue[j])
                        {
                            SetValueDenied();
                            return;
                        }
                    }
                }
            }

            SetValueAccepted();
        }
        
        return;
    }
    #endregion

    #region SetValueDenied
    void SetValueDenied()
    {
        gamestate = Gamestate.ValueDenied;
        deltaTime = 0f;
        SetCodeBreakerMessage("Please insert a valid number!");
    }
    #endregion

    #region SetValueAccepted
    void SetValueAccepted()
    {
        gamestate = Gamestate.ValueAccepted;
        deltaTime = 0f;
        SetCodeBreakerMessage("Valid number!");
        
        codemakerBackground.SetActive(true);
        codemakerInputField.gameObject.SetActive(true);
        UIUpdateNumberToGuessUI();
    }
    #endregion
    
    #region SetCodeBreakerThink
    void SetCodeBreakerThink()
    {
        deltaTime = 0;
        gamestate = Gamestate.CodeBreakerThink;
        SetCodeBreakerMessage("Hmm... let me think.");
        CodeBreakerGenerateAttempt();
        UIUpdateNumberGuessedUI();
    }
    #endregion
    
    #region SetCodeBreakerTry
    void SetCodeBreakerTry()
    {
        deltaTime = 0;
        gamestate = Gamestate.CodeBreakerTry;
        SetCodeBreakerMessage("Is it " + codeBreakerAttempt[0] + codeBreakerAttempt[1] + codeBreakerAttempt[2] + codeBreakerAttempt[3] + "?");
        CalculateCowsAndBulls();
    }
    #endregion
    
    #region SetCodeMakerAnswer
    void SetCodeMakerAnswer()
    {
        deltaTime = 0;
        SetCodeMakerMessage( "Attempt: " + codeBreakerAttempt[0] + codeBreakerAttempt[1] + codeBreakerAttempt[2] + codeBreakerAttempt[3] + "          Bulls: " + bulls + "          Cows: " + cows);
        codemakerInputField.gameObject.SetActive(false);
        gamestate = Gamestate.CodeMakerAnswer;
    }
    #endregion
    
    #region SetWin
    void SetWin()
    {
        UIUpdateNumberGuessedUI();
        deltaTime = 0;
        SetCodeMakerMessage("Yes! " + codeBreakerAttempt[0] + codeBreakerAttempt[1] + codeBreakerAttempt[2] + codeBreakerAttempt[3] + " is the correct number!");
        gamestate = Gamestate.Win;
    }
    #endregion

    // Game Functions

    #region CodeBreakerGenerateAttempt
    void CodeBreakerGenerateAttempt()
    {
        // first time or until having bulls and cows
        if(cows == 0 && bulls == 0 && previousAttemptBulls == 0 && previousAttemptCows == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                codeBreakerAttempt[i] = GenerateValidDigit(codeBreakerAttempt);
            }
        }
        // when have bulls but don't know where
        else if(bulls > 0 && bulls > finalBulls)
        {
            // test if it is a bull -> if the bull count decrease by one after adding one to his value we know it was a bull
            // if it's not a bull it increases one each turn until it become a bull
            if(codeBreakerFinalAttemp[codeBreakerSideCounter] == -1)
            {
                CycleUpCodeBreakerCounter(codeBreakerAttempt[codeBreakerSideCounter]);
            }
            else
            {
                CycleSideCodeBreakerCounter();
                CodeBreakerGenerateAttempt();
            }
        }
        // when have cows try to swap positions until getting a bull
        else if(cows > 0 )
        {
            if(codeBreakerFinalAttemp[codeBreakerSideCounter] == -1)
            {
                int aux = codeBreakerSideCounter;
                for (int j = 0; j < 4; j++)
                {
                    CycleSideCodeBreakerCounter();
                    if(codeBreakerFinalAttemp[codeBreakerSideCounter] == -1)
                    {
                        int temp = codeBreakerAttempt[codeBreakerSideCounter];
                        codeBreakerAttempt[codeBreakerSideCounter] = codeBreakerAttempt[aux];
                        codeBreakerAttempt[aux] = temp;
                        break;
                    }
                }
            }
            else
            {
                CycleSideCodeBreakerCounter();
                CodeBreakerGenerateAttempt();
            }
        }
        // when don't have nothing go to the first digit with no bull and increase the value by one
        else
        {
            for (int i = 0; i < 4; i++)
            {
                if(codeBreakerFinalAttemp[i] == -1)
                {
                    codeBreakerSideCounter = i;
                    CycleUpCodeBreakerCounter(codeBreakerAttempt[i]);
                    break;
                }
            }
        }
    }
    #endregion

    #region CalculateCowsAndBulls
    void CalculateCowsAndBulls()
    {
        previousAttemptBulls = bulls;
        previousAttemptCows = cows;

        int[] aux = new int[4];
        bulls = 0;
        cows = 0;
        // calculate bulls
        for (int i = 0; i < 4; i++)
        {
            if(codeBreakerAttempt[i] == codeMakerValue[i])
            {
                bulls++;
                aux[i] = 1;
            }
        }

        // calculate cows 
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if(aux[i] == 0 && aux[j] == 0)
                {
                    if(codeBreakerAttempt[i] == codeMakerValue[j])
                    {
                        cows++;
                    }
                }
            }
        }

        // win condition
        if(bulls >= 4)
        {
            SetWin();
        }
        // flag for on bull found
        else if(bulls == previousAttemptBulls - 1 && previousAttemptBulls > 0)
        {
            OneBullFound();
        }
        else if(bulls == previousAttemptBulls)
        {
            // if a cow is found, resets bull value by decreasing the value by one (previoulsy increase by one for testing)
            if(cows == previousAttemptCows - 1)
            {
               CycleDownCodeBreakerCounter(codeBreakerAttempt[codeBreakerSideCounter]);
            }
            // if no cow found, moves no next digit
            if(cows != previousAttemptCows)
            {
                CycleSideCodeBreakerCounter();
            }
        }
    }
    #endregion

    // Helper Functions

    #region GenerateValidDigit
    int GenerateValidDigit(int[] array)
    {
        int aux = 0;
        for (int i = 0; i < 1; i++)
        {
            aux = UnityEngine.Random.Range(0, 10);
            for (int j = 0; j < 4; j++)
            {
                if(aux == array[j])
                {
                    i--;
                    break;
                }
            }
        }
        return aux;
    }
    #endregion
    
    #region CycleSideCodeBreakerCounter
    void CycleSideCodeBreakerCounter()
    {
        codeBreakerSideCounter++;
        if(codeBreakerSideCounter >= 4)
        {
            codeBreakerSideCounter = 0;
        }
    }
    #endregion
    
    #region CycleUpCodeBreakerCounter
    void CycleUpCodeBreakerCounter(int value)
    {
        codeBreakerUpCounter = value;
        // increase to next available digit
        for (int i = 0; i < 1; i++)
        {
            codeBreakerUpCounter++;
            if(codeBreakerUpCounter >= 10)
            {
                codeBreakerUpCounter = 0;
            }
            codeBreakerAttempt[codeBreakerSideCounter] = codeBreakerUpCounter;
            for (int j = 0; j < 4; j++)
            {
                if(j != codeBreakerSideCounter)
                {
                    if(codeBreakerFinalAttemp[j] == codeBreakerUpCounter)
                    {
                        i--;
                    }
                }
            }
        }
    }
    #endregion
    
    #region CycleDownCodeBreakerCounter
    void CycleDownCodeBreakerCounter(int value)
    {
        codeBreakerUpCounter = value;
        // increase no next available digit
        for (int i = 0; i < 1; i++)
        {
            codeBreakerUpCounter--;
            if(codeBreakerUpCounter < 0)
            {
                codeBreakerUpCounter = 9;
            }
            codeBreakerAttempt[codeBreakerSideCounter] = codeBreakerUpCounter;
            for (int j = 0; j < 4; j++)
            {
                if(j != codeBreakerSideCounter)
                {
                    if(codeBreakerFinalAttemp[j] == codeBreakerUpCounter)
                    {
                        i--;
                    }
                }
            }
        }
    }
    #endregion
    
    #region OneBullFound
    void OneBullFound()
    {
        if(codeBreakerFinalAttemp[codeBreakerSideCounter] == -1)
        {
            CycleDownCodeBreakerCounter(codeBreakerAttempt[codeBreakerSideCounter]);
            codeBreakerFinalAttemp[codeBreakerSideCounter] = codeBreakerAttempt[codeBreakerSideCounter];
            finalBulls++;
            codeBreakerSideCounter = 0;
        }
    }
    #endregion

    // Update UI

    #region UIUpdateNumberGuessedUI
    void UIUpdateNumberGuessedUI()
    {
        if(codeBreakerFinalAttemp[0] >= 0)
        {
            UINumberGuessed0.gameObject.SetActive(true);
            UINumberGuessed0.text = codeBreakerAttempt[0].ToString();
        }
        if(codeBreakerFinalAttemp[1] >= 0)
        {
            UINumberGuessed1.gameObject.SetActive(true);
            UINumberGuessed1.text = codeBreakerAttempt[1].ToString();
        }
        if(codeBreakerFinalAttemp[2] >= 0)
        {
            UINumberGuessed2.gameObject.SetActive(true);
            UINumberGuessed2.text = codeBreakerAttempt[2].ToString();
        }
        if(codeBreakerFinalAttemp[3] >= 0)
        {
            UINumberGuessed3.gameObject.SetActive(true);
            UINumberGuessed3.text = codeBreakerAttempt[3].ToString();
        }
    }
    #endregion
    
    #region UIUpdateNumberToGuessUI
    void UIUpdateNumberToGuessUI()
    {
        UINumberToGuess0.text = codeMakerValue[0].ToString();
        UINumberToGuess1.text = codeMakerValue[1].ToString();
        UINumberToGuess2.text = codeMakerValue[2].ToString();
        UINumberToGuess3.text = codeMakerValue[3].ToString();
    }
    #endregion
    
    // Chatting

    #region SetCodeBreakerMessage
    void SetCodeBreakerMessage(string text)
    {
        codebreakerText.text = text;
    }
    #endregion

    #region SetCodeMakerMessage
    void SetCodeMakerMessage(string text)
    {
        codemakerText.gameObject.SetActive(true);
        codemakerText.text = text;
    }
    #endregion

}
