using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
    void PreActivation(System.Object data);
    void PostActivation(System.Object data);
    void PreDeactivation();
    void PostDeactivation();
}