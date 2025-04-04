using System;
using UnityEngine;

public class CheckpointBehaviour : MonoBehaviour
{

    public GameObject effect;

    public void showEffect(Boolean show){
        effect.SetActive(show);
    }

}
