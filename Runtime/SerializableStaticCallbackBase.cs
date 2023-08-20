using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using static UnityEngine.GraphicsBuffer;
using System.Reflection;
using UnityEditor;
using B83;

public abstract class SerializableStaticCallbackBase<TReturn> : SerializableStaticCallbackBase 
{ 
    public InvokableCallbackBase<TReturn> func;
    
    public override void ClearCache()
    {
        base.ClearCache();
        func = null;
    }

    protected InvokableCallbackBase<TReturn> GetPersistentMethod()
    {
        Type[] types = new Type[ArgRealTypes.Length + 1];
        Array.Copy(ArgRealTypes, types, ArgRealTypes.Length);
        types[types.Length - 1] = typeof(TReturn);

        Type genericType = null;
        switch (types.Length)
        {
            case 1:
                genericType = typeof(InvokableCallback<>).MakeGenericType(types);
                break;
            case 2:
                genericType = typeof(InvokableCallback<,>).MakeGenericType(types);
                break;
            case 3:
                genericType = typeof(InvokableCallback<,,>).MakeGenericType(types);
                break;
            case 4:
                genericType = typeof(InvokableCallback<,,,>).MakeGenericType(types);
                break;
            case 5:
                genericType = typeof(InvokableCallback<,,,,>).MakeGenericType(types);
                break;
            default:
                throw new ArgumentException(types.Length + "args");
        }
        return Activator.CreateInstance(genericType, new object[] {/*target,*/ methodName }) as InvokableCallbackBase<TReturn>;
    }
}

/// <summary> An inspector-friendly serializable function </summary>
[System.Serializable]

public abstract class SerializableStaticCallbackBase : ISerializationCallbackReceiver
{

    /// <summary> Target object </summary>
    //public Object target { get { return _target; } set { _target = value; ClearCache(); } }
    //public Type targetType { get { return _targetType; }/* set { _targetMonoScript = value; ClearCache(); }*/ }
    //public Type targetType { get { Debug.Log(_targetType.GetClass()); return _targetType.GetClass(); }/* set { _targetMonoScript = value; ClearCache(); }*/ }
    //public Type targetType { get { return Type.GetType(_targetType, true); }/* set { _targetMonoScript = value; ClearCache(); }*/ }
    public Type targetType { get { return _targetType.Type; }/* set { _targetMonoScript = value; ClearCache(); }*/ }
    /// <summary> Target method name </summary>
    
    public string methodName { get { return _methodName; } set { _methodName = value; ClearCache(); } }
    public bool isStatic { get => _isStatic; }
    //public bool isStatic { get => targetType.GetMethod(methodName).IsStatic; }
    public object[] Args { get { return args != null ? args : args = _args.Select(x => x.GetValue()).ToArray(); } }
    public object[] args;
    public Type[] ArgTypes { get { return argTypes != null ? argTypes : argTypes = _args.Select(x => Arg.RealType(x.argType)).ToArray(); } }
    public Type[] argTypes;
    public Type[] ArgRealTypes { get { return argRealTypes != null ? argRealTypes : argRealTypes = _args.Select(x => Type.GetType(x._typeName,true)).ToArray(); } }
    public Type[] argRealTypes;
    //public bool dynamic { get { return _dynamic; } set { _dynamic = value; ClearCache(); } }

    //[SerializeField] protected Object _target;

    //[SerializeField] protected Type _targetType;
    //[SerializeField] protected MonoScript _targetType;//! cant be used for build. it uses unityengine. Use Type instead
    //[SerializeField] protected string _targetType;
    [SerializeField] protected SerializableMonoScript _targetType;
    [SerializeField] protected string _methodName;
    [SerializeField] protected Arg[] _args;
    //[SerializeField] protected bool _dynamic;
    [SerializeField] protected bool _isStatic;
#pragma warning disable 0414
    [SerializeField] private string _typeName;
#pragma warning restore 0414

    [SerializeField] private bool dirty;

#if UNITY_EDITOR
    protected SerializableStaticCallbackBase()
    {
        _typeName = base.GetType().AssemblyQualifiedName;
    }
#endif

    public virtual void ClearCache()
    {
        argTypes = null;
        args = null;
    }

    public void SetMethod(string methodName, bool dynamic, params Arg[] args)
    {
        _methodName = methodName;
        //_dynamic = dynamic;
        _isStatic=targetType.GetMethod(methodName).IsStatic;
        _args = args;
        ClearCache();
    }

    protected abstract void Cache(params object[] args);

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (dirty) { ClearCache(); dirty = false; }
#endif
    }

    public void OnAfterDeserialize()
    {
#if UNITY_EDITOR
        _typeName = base.GetType().AssemblyQualifiedName;
#endif
    }
}
