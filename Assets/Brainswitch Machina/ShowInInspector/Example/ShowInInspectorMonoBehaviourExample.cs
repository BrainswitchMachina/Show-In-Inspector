using UnityEngine;
using System.Collections;

public class ShowInInspectorMonoBehaviourExample : MonoBehaviour
{
    /// <summary>
    /// Note: Static fields do not reset when exiting play mode, thus they retain their edited value until you start play mode again.
    /// </summary>
    [ShowInInspector]
    public static int staticIntTest;

    [ShowInInspector]
    private string _textTest = "Test";

    #if UNITY_4_3
    [ShowInInspector]
    public Quaternion rotationPropertyTest
    {
        get
        {
            return Quaternion.identity;
        }
    }
    #endif

    [SerializeField, HideInInspector]
    private Vector2 _vector2;

    [ShowInInspector]
    public Vector2 vector2PropertyTest
    {
        get
        {
            return _vector2;
        }
        set
        {
            _vector2 = Vector2.ClampMagnitude(value, 10f);
        }
    }

}
