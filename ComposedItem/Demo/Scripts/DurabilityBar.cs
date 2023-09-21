using System;
using MoreMountains.Tools;
using UnityEngine;

public class DurabilityBar : MonoBehaviour
{
    private MMProgressBar _bar;
    [NonSerialized] public DurabilityUseComponent Component;
    private void Awake() => _bar = GetComponent<MMProgressBar>();
    private void Update()
    {
        if (Component) _bar.SetBar01(Component.Durability);
    }
}
