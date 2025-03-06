using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageManager : MonoBehaviour
{
    public List<GameObject> pageList = new List<GameObject>();
    public GameObject nextButton;
    public GameObject prevButton;
    public GameObject finishbutton;
    public Text number;
    public Text hint_text;

    public List<string> hint_text_list= new List<string>();

    [HideInInspector] public int pageIndex = 0;
    private void Start()
    {
        InitPage();
    }

    public void showHintText()
    {
        hint_text.text = "";
        if (pageIndex < hint_text_list.Count)
        {
            hint_text.text = hint_text_list[pageIndex];
        }
        
    }

    public void NextPage()
    {
        pageIndex = pageIndex + 1 >= pageList.Count ? pageList.Count - 1 : pageIndex + 1;
        ShowPage(pageIndex);
        CheckButtonDisplay();
        if (number != null)
        {
            number.text = (pageIndex + 1).ToString() + " / " + pageList.Count.ToString();
            showHintText();
        }

        
    }

    public void PrevPage()
    {
        pageIndex = pageIndex - 1 < 0 ? 0 : pageIndex - 1;
        ShowPage(pageIndex);
        CheckButtonDisplay();
        if (number != null)
        {
            number.text = (pageIndex + 1).ToString() + " / " + pageList.Count.ToString();
        }
        finishbutton.SetActive(false);
    }

    public void InitPage()
    {
        pageIndex = 0;
        ShowPage(pageIndex);
        CheckButtonDisplay();
        if(number != null)
        {
            number.text = (pageIndex + 1).ToString() + " / " + pageList.Count.ToString();
        }
        finishbutton.SetActive(false);
    }

    public void AddPage(GameObject page)
    {
        pageList.Add(page);
    }

    public void ClearPage()
    {
        pageList.Clear();
        InitPage();
        
    }

    private void ShowPage(int index)
    {
        if(index > pageList.Count)
        {
            Debug.LogError("Page index out of range!");
            return;
        }

        for(int i = 0; i < pageList.Count; i++)
        {
            pageList[i].SetActive(false);
        }

        pageList[index].SetActive(true);
    }
    
    private void CheckButtonDisplay()
    {
        if (nextButton != null)
        {
            nextButton.SetActive(true);
            if (pageIndex == pageList.Count - 1)
            {
                nextButton.SetActive(false);
                
                finishbutton.SetActive(true);
            }
        }

        if(prevButton != null)
        {
            prevButton.SetActive(true);
            if (pageIndex == 0)
            {
                prevButton.SetActive(false);
            }
        }
    }
}
