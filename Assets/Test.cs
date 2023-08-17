using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;
using System.Linq;

public class Test : MonoBehaviour
{
    public NNModel modelAsset;
    private Model m_RuntimeModel;
    private IWorker worker;

    public Texture2D inputTex;
    public Transform dotParent;
    public GameObject dot;
    public GameObject box;

    private void Start()
    {
        m_RuntimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, m_RuntimeModel);

        Predict();
    }

    public void Predict()
    {
        using Tensor inputTensor = new Tensor(inputTex, channels: 3);

        worker.Execute(inputTensor);

        Tensor outputTensor = worker.PeekOutput();

        //get highest confidence
        var classProbabilities = new List<float>();
        for (var boxIndex = 0; boxIndex < outputTensor.width; boxIndex++)
        {
            float confidence = outputTensor[0, 0, boxIndex, 4];
            classProbabilities.Add(confidence);
        }

        var maxIndex = classProbabilities.Any() ? classProbabilities.IndexOf(classProbabilities.Max()) : 0;
        UnityEngine.Debug.Log("Highest confidence:" + outputTensor[0, 0, maxIndex, 4] + " and its index:" + maxIndex);

        Vector2 boxCenter = new Vector2(outputTensor[0, 0, maxIndex, 0], outputTensor[0, 0, maxIndex, 1]);
        Vector2 boxSize = new Vector2(outputTensor[0, 0, maxIndex, 2], outputTensor[0, 0, maxIndex, 3]);
        CreateBox(boxCenter, boxSize);

        for (int i = 5; i < outputTensor.channels; i += 3)
        {
            Vector2 pos = new Vector2(outputTensor[0, 0, maxIndex, i], outputTensor[0, 0, maxIndex, i + 1]);
            CreateRedDot(i, pos);
        }
    }

    //create red dots on specific position
    void CreateRedDot(int boxIndex, Vector2 pos)
    {
        GameObject newDot = Instantiate(dot).gameObject;
        newDot.transform.SetParent(dotParent);
        newDot.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x, -pos.y);
        newDot.name = boxIndex.ToString();
        newDot.SetActive(true);
    }

    void CreateBox(Vector2 pos, Vector2 size)
    {
        box.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x, -pos.y);
        box.GetComponent<RectTransform>().sizeDelta = size;
        box.SetActive(true);
    }
}
