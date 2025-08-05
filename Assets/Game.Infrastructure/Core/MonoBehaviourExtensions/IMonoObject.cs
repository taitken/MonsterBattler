using System;
using UnityEngine;

public interface IMonoObject
{
    BaseObjectModel GetModel();
    MonoBehaviour AsMonoBehaviour(); 
}