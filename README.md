Show In Inspector
=================

Unity 3D utility for showing non-serialized (static, private, protected) fields and properties in the Inspector Window. Show In Inspector only exposes the fields and properties in the Inspector Window, it does not serialize them.

Example usage:

Static field:
	[ShowInInspector]
	public static int staticIntExample;

Property (with serialized private field for saving):
    [SerializeField, HideInInspector]
    private Vector2 _vector2;

    [ShowInInspector]
    public Vector2 vector2PropertyExample
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