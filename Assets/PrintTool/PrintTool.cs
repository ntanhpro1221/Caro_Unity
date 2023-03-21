using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PrintTool : MonoBehaviour
{
    [SerializeField] private GameObject textIDE;
    public static List<Tuple<GameObject, TMPro.TextMeshProUGUI>> destroyObject = new List<Tuple<GameObject, TMPro.TextMeshProUGUI>>();
    public static List<Tuple<float, float>> destroyObjectTime = new List<Tuple<float, float>>();
    public GameObject std_cout(string inp, Vector3 coord, Color color)
    {
        GameObject spawnedObject = Instantiate(textIDE);
        TMPro.TextMeshProUGUI spawnedText = spawnedObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        spawnedText.text = inp;
        spawnedText.transform.position += coord;
        spawnedText.color = color;
        return spawnedObject;
    }
    public void Update()
    {
        for (int i = 0; i < destroyObject.Count; i++)
        {
            if (destroyObjectTime[i].Item1 > 0)
            {
                destroyObject[i].Item2.alpha = destroyObjectTime[i].Item1 / destroyObjectTime[i].Item2;
                destroyObjectTime[i] = new Tuple<float, float>
                    (destroyObjectTime[i].Item1 - Time.deltaTime, destroyObjectTime[i].Item2);
            }
            else
            {
                Destroy(destroyObject[i].Item1);
                destroyObject.RemoveAt(i);
                destroyObjectTime.RemoveAt(i);
                i--;
            }
        }
    }
    public void DestroySlowly(GameObject spawnedObject, float time)
    {
        destroyObject.Add(new Tuple<GameObject, TMPro.TextMeshProUGUI>(spawnedObject, spawnedObject.GetComponentInChildren<TMPro.TextMeshProUGUI>()));
        destroyObjectTime.Add(new Tuple<float, float>(time, time));
    }
}
