using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.Serialization;

public abstract class Loop : MonoBehaviour
{
    #region Variables

    [SerializeField] private int startingBeat;
    public NarrativeBeat currentBeat;
    
    [Header("UI Components")]
    [SerializeField] private TMP_Text nameText, dialogueText;
    [SerializeField] private Image portraitImage, borderImage;
    [SerializeField] private Button continueButton;
    
    [SerializeField] private GameObject dialogueUI, combatUI;

    [Header("Graphics")] [SerializeField] private Sprite panzaPortrait, quixotePortrait, panzaBorder, quixoteBorder; 
    [SerializeField] private SpriteRenderer dqSpriteRenderer, spSpriteRenderer;

    [Header("Animation")]
    [SerializeField] private Animator spAnim, dqAnim, fogAnim;

    [SerializeField] private NarrativeBeat[] beats;

    [SerializeField] private GameObject _options;
    private DialogueOptionsHandler _optionsHandler;

    // int loopNumber;
    //[SerializeField] private Loop nextLoop;

    [SerializeField] private Combat[] combats;
    private int _combatNum = 0;

    [SerializeField] private TMP_FontAsset charactersFont, narratorsFont;

    private Audio _audioMaster;
    [SerializeField] private AudioSource voiceLine;

    #endregion

    private void Start()
    {
        
        StartLoop();
    }

    public virtual void StartLoop()
    {
        _audioMaster = (Audio) FindObjectOfType<Audio>();
        _optionsHandler = (DialogueOptionsHandler) FindObjectOfType<DialogueOptionsHandler>();

        dialogueUI.SetActive(true);
        combatUI.SetActive(false);
        
        //LoadBeat(beats[0].numInOrder);
        LoadBeat(startingBeat);

    }

    protected virtual void StartCombat()
    {
        combats[currentBeat.combatToStart].gameObject.SetActive(true);

        combats[currentBeat.combatToStart].StartCombat();
        _combatNum++;
        
    }

    public virtual void LoadBeat(int numInOrder)
    {
        //if (beats.Length < numInOrder) StartNextLoop();
        
        currentBeat = beats[numInOrder - 1];

        //if (currentBeat.startNextLoop) StartNextLoop();

        if (currentBeat.isCombat) StartCombat();

        ChangeSpeaker(currentBeat.speaker);
        
        #region CheckComponents

        if (currentBeat.reactionPortrait != null)
        {
            portraitImage.sprite = currentBeat.reactionPortrait;
            borderImage.color = Color.white;
            portraitImage.color = Color.white;
        }

        if (currentBeat.whoChangesSprites != null) ChangeSprite();

        if (currentBeat.dialogueLine != null)
        {
            
            _options.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(true);
            dialogueText.text = currentBeat.dialogueLine;
            //TriggerVoice(currentBeat.voiceLine);
            
        }
        
        if (currentBeat.isOptions)
        {
            continueButton.gameObject.SetActive(false);
            dialogueText.text = null;
            
            _options.gameObject.SetActive(true);
            _optionsHandler.InitOptions();
            
        }

        if (currentBeat.sound != null) TriggerSound(currentBeat.sound);

        if (currentBeat.animTrigger != null) TriggerAnim(currentBeat.whoIsAnimated, currentBeat.animTrigger);
        
        if (currentBeat.fogTrigger != null) TriggerFog(currentBeat.fogTrigger);
        
        if(currentBeat.voiceLine != null) TriggerVoice();
        #endregion
        
    }

    /*
    protected virtual void StartNextLoop()
    {
        
        nextLoop.gameObject.SetActive(true);
        nextLoop.GetComponent<Second_Loop>().enabled = true;
        nextLoop.StartLoop();

        gameObject.GetComponent<First_Loop>().enabled = false;
        gameObject.SetActive(false);
        
        
    }
    */
    
    protected virtual void ChangeSprite()
    {
        int i = 0;
        foreach (Sprite sprite in currentBeat.spritesToSwap)
        {
            if (currentBeat.whoChangesSprites[i] == Speaker.DonQuixote)
            {
                dqSpriteRenderer.sprite = currentBeat.spritesToSwap[i];
                i++;
            }
            else
            {
                spSpriteRenderer.sprite = currentBeat.spritesToSwap[i];
                i++;
            }
        }
        
    }

    protected virtual void TriggerAnim(Speaker[] speakers, string trigger)
    {
        
        foreach (Speaker character in speakers)
        {
            if (character == Speaker.DonQuixote)
            {
           
                dqAnim.SetTrigger(trigger);
            
            }
            else if (character == Speaker.SanchoPanza)
            {
            
                spAnim.SetTrigger(trigger);
            
            }
        }
        
    }


    protected virtual void TriggerFog(string trigger)
    {
        if (trigger == "Fog Out")
        {
            fogAnim.SetTrigger("Fog Out");
        }
        
        if (trigger == "Fog In")
        {
            fogAnim.SetTrigger("Fog In");
        }
    }


    protected virtual void TriggerSound(AudioClip sound)
    {
        
        _audioMaster.PlaySound(sound);
        
    }
    
    protected virtual void TriggerVoice()
    {
        
        voiceLine.PlayOneShot(currentBeat.voiceLine);
        
    }

    protected virtual void ChangeSpeaker(Speaker speaker)
    {
        
    //Debug.Log(currentBeat.speaker);
    
        switch (currentBeat.speaker)
        {
            case Speaker.DonQuixote:
                portraitImage.color = Color.white;
                borderImage.color = Color.white;
                nameText.text = "Don Quixote";
                dialogueText.font = charactersFont;
                portraitImage.sprite = quixotePortrait;
                borderImage.sprite = quixoteBorder;
                break;
            case Speaker.SanchoPanza:
                portraitImage.color = Color.white;
                borderImage.color = Color.white;
                nameText.text = "Sancho Panza";
                dialogueText.font = charactersFont;
                portraitImage.sprite = panzaPortrait;
                borderImage.sprite = panzaBorder;
                break;
            default:
                portraitImage.color = Color.clear;
                borderImage.color = Color.clear;
                nameText.text = null;
                dialogueText.font = narratorsFont;
                break;
        }

    }
    
}
