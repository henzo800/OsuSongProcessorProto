using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProgressController : MonoBehaviour
{
    public TMP_Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = this.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = GameManager.instance.progress.ToString();
    }
}
