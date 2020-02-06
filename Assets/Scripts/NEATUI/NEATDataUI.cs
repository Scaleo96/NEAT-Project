using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NEATDataUI : MonoBehaviour {

    [SerializeField] private Text generation;
    [SerializeField] private Text currenSpec;
    [SerializeField] private Text currentGenom;
    [SerializeField] private Text currenFit;
    [SerializeField] private Text bestFit;

    public void UpdateAll(int gen, int currenSpec, int currentGenom, int currenFit, int bestFit) {
        UpdateGen(gen);
        UpdateSpec(currenSpec);
        UpdateGenom(currentGenom);
        UpdateFit(currenFit);
        UpdateBestFit(bestFit);
    }

    public void UpdateGen(int gen) {
        generation.text = gen.ToString();
    }

    public void UpdateSpec(int currenSpec) {
        this.currenSpec.text = currenSpec.ToString();
    }

    public void UpdateGenom(int currentGenom) {
        this.currentGenom.text = currentGenom.ToString();
    }

    public void UpdateFit(int currenFit) {
        this.currenFit.text = currenFit.ToString();
    }

    public void UpdateBestFit(int bestFit) {
        this.bestFit.text = bestFit.ToString();
    }

}
