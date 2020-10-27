using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TestHistoryListItem : MonoBehaviour
{
    private Text text;
    private TestInfo testInfo;

    public GameObject browseTestHistoryPanel;
    private BrowseTestHistoryPanelControl c;

    void Awake()
    {
        // find the child Text object of the Button
        text = transform.Find("Text").gameObject.GetComponent<Text>();

        c = browseTestHistoryPanel.GetComponent<BrowseTestHistoryPanelControl>();
    }

    // when these are instantiated to populate the test history list, they're
    // given a label and a ref to a TestInfo object
    public void setData(string text, TestInfo ti)
    {
        this.text.text = text;
        this.testInfo = ti;
    }

    public void TestHistoryListItem_Click()
    {
        Debug.Log("Hello from list item at " + testInfo.dateTime.ToString("yyyy-MMM-dd HH:mm:ss"));

        c.displayTest(testInfo);
    }
}
