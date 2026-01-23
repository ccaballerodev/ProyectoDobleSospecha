using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WordReplacement
{
    public string targetWord;
    public string replacementWord;
}

public class MainMenuDOTween : MonoBehaviour
{
    [Header("Canvas References")]
    public CanvasGroup canvasGroup;
    public RectTransform menuPanel;

    [Header("Buttons")]
    public RectTransform[] buttons;

    [Header("Title")]
    public TMP_Text titleText;

    [Header("Title Word Replacements")]
    public WordReplacement[] replacements;
    public float minChangeTime = 4f;
    public float maxChangeTime = 9f;
    public float glitchDuration = 0.35f;

    string originalTitle;
    string[] words;

    void Awake()
    {
        DOTween.Init();
    }

    void Start()
    {
        originalTitle = titleText.text;
        words = originalTitle.Split(' ');

        PlayIntro();
        AmbientMotion();
        StartCoroutine(TitleWordReplacementRoutine());
    }

    // ================= INTRO =================
    void PlayIntro()
    {
        canvasGroup.alpha = 0;
        menuPanel.localScale = new Vector3(1, 0.9f, 1);

        Sequence seq = DOTween.Sequence();

        seq.Append(canvasGroup.DOFade(1, 1.8f));
        seq.Join(menuPanel.DOScaleY(1, 1.4f).SetEase(Ease.OutSine));

        foreach (RectTransform btn in buttons)
        {
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = btn.gameObject.AddComponent<CanvasGroup>();

            cg.alpha = 0;
            btn.localPosition += Vector3.left * 250;

            seq.AppendInterval(Random.Range(0.15f, 0.3f));
            seq.Append(btn.DOLocalMoveX(0, 0.6f).SetEase(Ease.OutExpo));
            seq.Join(cg.DOFade(1, 0.4f));
        }
    }

    // ================= BUTTON FX =================
    public void OnHover(RectTransform btn)
    {
        btn.DOKill();

        btn.DOScale(1.06f, 0.25f).SetEase(Ease.OutSine);

        btn.DOShakePosition(
            0.35f,
            new Vector3(4, 0, 0),
            vibrato: 12,
            randomness: 80
        );

        if (btn.name == "ButtonExit")
            btn.DOScale(1.08f, 0.4f);
    }

    public void OnExit(RectTransform btn)
    {
        btn.DOKill();
        btn.DOScale(1f, 0.2f);
    }

    // ================= AMBIENT =================
    void AmbientMotion()
    {
        Sequence titleSeq = DOTween.Sequence();

        titleSeq.Append(
            titleText.transform
                .DOScale(1.06f, 1.8f)
                .SetEase(Ease.InOutSine)
        );

        titleSeq.Join(
            titleText.rectTransform
                .DOAnchorPosY(
                    titleText.rectTransform.anchoredPosition.y + 8f,
                    1.8f
                )
                .SetEase(Ease.InOutSine)
        );

        titleSeq.SetLoops(-1, LoopType.Yoyo);

        menuPanel
            .DOLocalMoveY(menuPanel.localPosition.y + 6f, 4f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    // ================= TITLE LOGIC =================
    IEnumerator TitleWordReplacementRoutine()
    {
        if (replacements == null || replacements.Length == 0)
            yield break;

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minChangeTime, maxChangeTime));

            WordReplacement r = replacements[Random.Range(0, replacements.Length)];

            int index = System.Array.IndexOf(words, r.targetWord);
            if (index == -1)
                continue;

            words[index] = r.replacementWord;
            titleText.text = string.Join(" ", words);

            yield return new WaitForSeconds(glitchDuration);

            words[index] = r.targetWord;
            titleText.text = originalTitle;
        }
    }
}