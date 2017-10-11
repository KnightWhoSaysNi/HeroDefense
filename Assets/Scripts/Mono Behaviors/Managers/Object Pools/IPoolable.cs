using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
    void DoPreActivation(System.Object data);
    void DoPostActivation(System.Object data);
    void DoPreDeactivation();
    void DoPostDeactivation();
}