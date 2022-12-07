using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;
public enum BattleState
{ START, PLAYERTURN, ENEMYTURN, WON, LOST
}

public abstract class Combat : MonoBehaviour
{
    #region Variables
    
    public BattleState state;

    [Header("UI Components")] 
    [SerializeField] private GameObject dialogueUI, combatUI, combatOptions;
    [SerializeField] private TMP_Text dialogue, hpCounter;
    [SerializeField] private Image portrait;
    [SerializeField] private Slider enemyHealth,playerHealth;

    [Header("Commands")] 
    [SerializeField] private string[] fightLines, fleeLines, blockLines, dodgeLines;
    [SerializeField] private AudioClip[] fightClips, fleeClips, blockClips, dodgeClips;
    private AudioSource _audio;
    
    [Header("Characters")] 
    [SerializeField] private Unit donQuixote, windmill;

    [SerializeField] private SpriteRenderer dqSpriteRenderer, wmSpriteRenderer;
    [SerializeField] private Sprite dqPortrait, spPortrait, dqShock, dqAttack, dqBlock, dqPrepDodge, dqDodge, dqIdle, dqDefeat;
    [SerializeField] private Sprite enemySprite;

    [SerializeField] private Loop loop;
    public NarrativeBeat winBeat;
    public NarrativeBeat loseBeat;

    [Header("Audio")] 
    [SerializeField] private Audio _audioMaster;
    [SerializeField] private AudioSource battleMusic, bgAmbience, bgMusic;

    [Header("Animation")] [SerializeField] private Animator wmAnim, dqAnim;

    [Header("Characters")]
    [SerializeField] private GameObject dqHorse, spHorse, wmCombat, dqCombat;

    private bool _isBlocking, _wmIsBlocking, _isDodging;

    [SerializeField] private float delay = 2.5f;
    [SerializeField] private int maxTurns = 5;
    [SerializeField] private int oddsIgnored = 4;
    private int _turnCounter = 0;

    private bool _isWon;
    
    #endregion

    public virtual void StartCombat()
    {
        state = BattleState.START;
        StartCoroutine(SetUp());
    }

    protected virtual IEnumerator SetUp()
    {

        bgAmbience.gameObject.SetActive(false);
        bgMusic.gameObject.SetActive(false);
        battleMusic.gameObject.SetActive(true);

        _audio = GetComponent<AudioSource>();

        dialogueUI.SetActive(false);
        dqHorse.SetActive(false);
        spHorse.SetActive(false);
        combatUI.SetActive(true);
        dqCombat.SetActive(true);
        wmCombat.SetActive(true);

        wmSpriteRenderer.sprite = enemySprite;
        dqSpriteRenderer.sprite = dqIdle;

        donQuixote.currentHP = donQuixote.maxHP;
        playerHealth.maxValue = donQuixote.maxHP;

        windmill.currentHP = windmill.maxHP;
        enemyHealth.maxValue = windmill.maxHP;

        dialogue.text = "A wild " + windmill.name + " approaches!";

        yield return new WaitForSeconds(delay);
        PlayerTurn();

    }

    protected virtual void PlayerTurn()
    {

        _turnCounter++;
        _isDodging = false;
        _isBlocking = false;

        state = BattleState.PLAYERTURN;
        combatOptions.SetActive(true);
        dialogue.text = "What will Don Quixote do?";

    }

    protected virtual void Update()
    {

        playerHealth.value = donQuixote.currentHP;
        enemyHealth.value = windmill.currentHP;

        hpCounter.text = Mathf.RoundToInt(donQuixote.currentHP) + "/" + donQuixote.maxHP;

        if (donQuixote.currentHP <= 0)
        {
            portrait.sprite = dqShock;
            dqSpriteRenderer.sprite = dqDefeat;
            _isWon = false;
            StartCoroutine(End());

        }

    }

    protected virtual IEnumerator End()
    {

        //Debug.Log("End");
        state = BattleState.LOST;
        yield return new WaitForSeconds(delay);

        bgAmbience.gameObject.SetActive(true);
        bgMusic.gameObject.SetActive(true);
        battleMusic.gameObject.SetActive(false);
        
        dialogueUI.SetActive(true);
        dqHorse.SetActive(true);
        spHorse.SetActive(true);
        
        combatUI.SetActive(false);
        dqCombat.SetActive(false);
        wmCombat.SetActive(false);

        if (_isWon)
        {
            loop.LoadBeat(winBeat.numInOrder);
            gameObject.SetActive(false);
        }
        else
        {
            loop.LoadBeat(loseBeat.numInOrder);
            gameObject.SetActive(false);
        }
    }

    #region Reusable

    private void ChangePortrait(Sprite newPortrait)
    {
        portrait.sprite = newPortrait;
    }

    #endregion

    #region Player Combat Options
    
    void TurnOffCOmabtOptions()
    {
        combatOptions.SetActive(false);
    }
    
    public virtual IEnumerator PickAttack()
    {

        if (state != BattleState.PLAYERTURN) yield break;
        TurnOffCOmabtOptions();
        state = BattleState.ENEMYTURN;

        ChangePortrait(spPortrait);
        int q = Random.Range(0, fightLines.Length);
        dialogue.text = fightLines[q];
        _audio.PlayOneShot(fightClips[q]);

        yield return new WaitForSeconds(delay);
        int i = Random.Range(1, oddsIgnored);

        if (i == 2)
        {
            dialogue.text = "The Don ignores Sancho Panza's commands...";
            yield return new WaitForSeconds(delay);
            RandomMove();
        }
        else
        {
            dialogue.text = "The Don heeds Sancho Panza's advice!";
            yield return new WaitForSeconds(delay);
            StartCoroutine(PlayerAttack());
        }
        
    }
    
    protected virtual IEnumerator PlayerAttack()
    {
        
        var damage = donQuixote.damage * Random.Range(0.5f, 1.5f);
        
        ChangePortrait(dqPortrait);
        dialogue.text = "Don Quixote strikes with his lance!";

        dqSpriteRenderer.sprite = dqAttack;
        dqAnim.SetTrigger("Attack");
        yield return new WaitForSeconds(delay);
        dqSpriteRenderer.sprite = dqIdle;

        if (_wmIsBlocking)
        {
            dialogue.text = "The windmill blocks!";
            yield return new WaitForSeconds(delay);

            windmill.currentHP -= damage * 0.5f;
            dialogue.text = "Don Quixote dealt " + Mathf.RoundToInt(damage * 0.5f) + " damage!";
        }
        else
        {
            windmill.currentHP -= damage;
            dialogue.text = "Don Quixote dealt " + Mathf.RoundToInt(damage) + " damage!";
        }

        yield return new WaitForSeconds(delay);

        if (windmill.currentHP <= 0)
        {
            _isWon = true;
            StartCoroutine(End());
        }
        else
        {
            StartCoroutine(EnemyTurn());
        }

    }

    public virtual IEnumerator PickFlee()
    {

        if (state != BattleState.PLAYERTURN) yield break;
        TurnOffCOmabtOptions();
        state = BattleState.ENEMYTURN;
        
        ChangePortrait(spPortrait);
        int q = Random.Range(0, fleeLines.Length);
        dialogue.text = fleeLines[q];
        _audio.PlayOneShot(fleeClips[q]);
        
        yield return new WaitForSeconds(delay);
        
        StartCoroutine(PlayerFlee());
    }

    protected virtual IEnumerator PlayerFlee()
    {

        ChangePortrait(dqPortrait);
        dialogue.text = "Don Quixote scoffs at your cowardice, The Don never retreats!";
        yield return new WaitForSeconds(delay);
        
        StartCoroutine(EnemyTurn());
    }

    public virtual IEnumerator PickBlock()
    {

        if (state != BattleState.PLAYERTURN) yield break;
        TurnOffCOmabtOptions();
        state = BattleState.ENEMYTURN;

        ChangePortrait(spPortrait);
        int q = Random.Range(0, blockLines.Length);
        dialogue.text = blockLines[q];
        _audio.PlayOneShot(blockClips[q]);
        
        yield return new WaitForSeconds(delay);
        int i = Random.Range(1, oddsIgnored);

        if (i == 2)
        {
            dialogue.text = "The Don ignores Sancho Panza's commands...";
            yield return new WaitForSeconds(delay);
            RandomMove();
        }
        else
        {
            dialogue.text = "The Don heeds Sancho Panza's advice!";
            yield return new WaitForSeconds(delay);
            StartCoroutine(PlayerBlock());
        }
        
    }

    protected virtual IEnumerator PlayerBlock()
    {
        var block = Random.Range(0.25f, 0.75f);
        
        ChangePortrait(dqPortrait);
        dialogue.text = "Don Quixote braces himself!";
        
        dqSpriteRenderer.sprite = dqBlock;
        donQuixote.damageReduction = block;
        _isBlocking = true;
        yield return new WaitForSeconds(delay);

        StartCoroutine(EnemyTurn());

    }

    public virtual IEnumerator PickDodge()
    {

        if (state != BattleState.PLAYERTURN) yield break;
        TurnOffCOmabtOptions();
        state = BattleState.ENEMYTURN;
        
        ChangePortrait(spPortrait);
        int q = Random.Range(0, dodgeLines.Length);
        dialogue.text = dodgeLines[q];
        _audio.PlayOneShot(dodgeClips[q]);
        
        yield return new WaitForSeconds(delay);
        int i = Random.Range(1, oddsIgnored);

        if (i == 2)
        {
            dialogue.text = "The Don ignores Sancho Panza's commands...";
            yield return new WaitForSeconds(delay);
            RandomMove();
        }
        else
        {
            dialogue.text = "The Don heeds Sancho Panza's advice!";
            yield return new WaitForSeconds(delay);
            StartCoroutine(PlayerDodge());
        }
        
    }

    protected virtual IEnumerator PlayerDodge()
    {

        ChangePortrait(dqPortrait);
        dialogue.text = "Don Quixote prepares to dodge.";
        
        dqSpriteRenderer.sprite = dqPrepDodge;
        _isDodging = true;
        yield return new WaitForSeconds(delay);

        StartCoroutine(EnemyTurn());
    }
    
    public virtual IEnumerator PickTaunt()
    {

        if (state != BattleState.PLAYERTURN) yield break;
        TurnOffCOmabtOptions();
        state = BattleState.ENEMYTURN;
        
        ChangePortrait(spPortrait);
        dialogue.text = "This is patetico to watch. If Dulcinea could see you now she'd die of laughter!";
        
        yield return new WaitForSeconds(delay);
        
        _isWon = false;
        wmAnim.SetTrigger("Attack");

        yield return new WaitForSeconds(delay);
        
        StartCoroutine(End());

    }
    
    public virtual IEnumerator PickEncourage()
    {

        if (state != BattleState.PLAYERTURN) yield break;
        TurnOffCOmabtOptions();
        state = BattleState.ENEMYTURN;
        
        ChangePortrait(spPortrait);
        dialogue.text = "Buen trabajo! A couple more hits like that and you might stand a chance!";
        
        yield return new WaitForSeconds(delay);
        
        _isWon = true;
        
        StartCoroutine(End());

    }

    private void RandomMove()
    {
        int i = Random.Range(1, 3);
        {
            if (i == 1)
            {
                StartCoroutine(PlayerDodge());
            }
            else if (i == 2)
            {
                StartCoroutine(PlayerBlock());
            }
            else
            {
                StartCoroutine(PlayerAttack());
            }
        }
    }

    #endregion

    protected virtual IEnumerator EnemyTurn()
    {
        state = BattleState.ENEMYTURN;
        _wmIsBlocking = false;

        if (_turnCounter < maxTurns)
        {
            int i = Random.Range(0, 2);

            if (i == 1)
            {
                dialogue.text = "The giant attacks!";
                yield return new WaitForSeconds(delay);
                wmAnim.SetTrigger("Attack");

                if (_isBlocking == true)
                {
                    float damage = windmill.damage * Random.Range(0.5f, 1.5f);
                    dialogue.text = "Don Quixote blocks the blow!";
                    donQuixote.currentHP -= damage * donQuixote.damageReduction;
                    dqSpriteRenderer.sprite = dqIdle;

                    dialogue.text = "Don Quixote takes " + Mathf.RoundToInt(damage * donQuixote.damageReduction) + " damage!";
                    donQuixote.currentHP -= damage;
                    yield return new WaitForSeconds(delay);

                    PlayerTurn();
                }
                else if (_isDodging == true)
                {

                    dqSpriteRenderer.sprite = dqDodge;
                    dialogue.text = "Don Quixote does a barrel roll!";
                    dqAnim.SetTrigger("Roll");
                    yield return new WaitForSeconds(delay);
                    dqSpriteRenderer.sprite = dqIdle;

                    int q = Random.Range(0, 2);
                    if (q == 0)
                    {

                        dialogue.text = "He fails to dodge the attack!";
                        yield return new WaitForSeconds(delay);

                        float damage = windmill.damage * Random.Range(0.5f, 1.5f);
                        dialogue.text = "Don Quixote takes " + Mathf.RoundToInt(damage).ToString() + " damage!";
                        donQuixote.currentHP -= damage;
                        yield return new WaitForSeconds(delay);

                        PlayerTurn();

                    }
                    else
                    {
                        dialogue.text = "Don Quixote sticks the landing!";
                        yield return new WaitForSeconds(delay);

                        PlayerTurn();
                    }
                }
                else
                {

                    float damage = windmill.damage * Random.Range(0.5f, 1.5f);
                    dialogue.text = "Don Quixote takes " + Mathf.RoundToInt(damage).ToString() + " damage!";
                    donQuixote.currentHP -= damage;
                    yield return new WaitForSeconds(delay);

                    PlayerTurn();

                }
            }
            else
            {
                dialogue.text = "The giant braces itself!";
                yield return new WaitForSeconds(delay);

                float damage = donQuixote.damage * Random.Range(0.5f, 1.5f);
                _wmIsBlocking = true;
                yield return new WaitForSeconds(delay);

                PlayerTurn();

            }

        }
        else
        {
            
            StartCoroutine(UltimateAttack());

        }
    }

    [SerializeField] private GameObject laser, charge, explosion;
    [SerializeField] private AudioClip laserSFX, chargeSFX, explosionSFX;
    
    protected virtual IEnumerator UltimateAttack()
    {
        
        _audioMaster._audioSource.volume *= 2;
        _audioMaster.PlaySound(chargeSFX);
        charge.gameObject.SetActive(true);
        dialogue.text = "The windmill charges up its ultimate attack!";
        yield return new WaitForSeconds(delay);

        _audioMaster.StopSound();
        charge.gameObject.SetActive(false);
        laser.gameObject.SetActive(true);
        _audioMaster.PlaySound(laserSFX);
        
        if (_isBlocking == true)
        {

            dialogue.text = "Don Quixote tries to block such a blow but can't!";
            yield return new WaitForSeconds(delay);

        }

        if (_isDodging == true)
        {

            dialogue.text = "Don Quixote tries to dodge but the attack consumes all!";
            yield return new WaitForSeconds(delay);

        }

        _audioMaster.StopSound();
        laser.gameObject.SetActive(false);
        explosion.gameObject.SetActive(true);
        _audioMaster._audioSource.volume *= 2;
        _audioMaster.PlaySound(explosionSFX);
        donQuixote.currentHP -= 1000f;
        yield return new WaitForSeconds(2);
        explosion.gameObject.SetActive(false);
        _audioMaster.StopSound();
        _audioMaster._audioSource.volume *= 0.25f;
        
    }
    
}
