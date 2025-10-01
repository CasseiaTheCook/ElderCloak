using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueScript : MonoBehaviour, IInteractable
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text notifier;

    [Header("Dialogue Pages")]
    [TextArea(2, 5)]
    public string[] pages;

    public float letterDelay = 0.05f;

    private int currentPage = 0;
    private Coroutine typingCoroutine;
    private bool isDialogueActive = false;

    private InputAction nextDialogueAction;


    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerSprite"))
        {
            notifier.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerSprite"))
        {
            notifier.gameObject.SetActive(false);
        }
    }

    public void StartDialogue()
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        currentPage = 0;
        ShowPage();
    }

    void ShowPage()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(pages[currentPage]));
    }

    IEnumerator TypeText(string page)
    {
        dialogueText.text = "";
        foreach (char c in page)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(letterDelay);
        }
    }
    public void Interact()
    {
        if(!isDialogueActive)
            StartDialogue();
        else
            OnNextDialogue();
    }
    private void OnNextDialogue()
    {
        if (!isDialogueActive) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = pages[currentPage];
            typingCoroutine = null;
        }
        else
        {
            currentPage++;
            if (currentPage < pages.Length)
            {
                ShowPage();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        isDialogueActive = false;
    }
}