using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Datatag;

using IObject = IDictionary<string, Node>;
using TObject = Dictionary<string, Node>;

using IArray = IList<Node>;
using TArray = List<Node>;

public enum NodeType : byte
{
    Null,
    Bool,
    Int,
    Float,
    String,
    Array,
    Object,
}

public class Node : IArray, IObject
{
    private static string ERR_NOT_TYPE<T>() => $"Node is not of type {typeof(T).Name}";
    private const string ERR_NOT_OBJECT = "Node is not an object";
    private const string ERR_NOT_ARRAY = "Node is not an array";
    private const string ERR_NOT_ARRAY_OR_OBJECT = "Node is neither an array or an object";
    protected object _value;

    public Node(object value) => _value = value;

    public Node this[int index]
    {
        get
        {
            if (_value is TArray arr)
            {
                return arr[index];
            }
            else
            {
                throw new InvalidOperationException(ERR_NOT_ARRAY);
            }
        }
        set
        {
            if (_value is TArray arr)
            {
                arr[index] = value;
            }
            else
            {
                throw new InvalidOperationException(ERR_NOT_ARRAY);
            }
        }
    }

    public Node this[string key]
    {
        get
        {
            if (_value is TObject obj)
            {
                return obj[key];
            }
            else
            {
                throw new InvalidOperationException(ERR_NOT_OBJECT);
            }
        }
        set
        {
            if (_value is TObject obj)
            {
                obj[key] = value;
            }
            else
            {
                throw new InvalidOperationException(ERR_NOT_OBJECT);
            }
        }
    }

    public static Node NewObject() => new Node(new TObject());

    public static Node NewArray() => new Node(new TArray());

    public bool IsNull => _value == null;

    public bool IsBool => _value is bool;

    public bool IsInt => _value is long;

    public bool IsFloat => _value is double;

    public bool IsString => _value is string;

    public bool IsPrimitive => IsNull || IsBool || IsInt || IsFloat || IsString;

    public bool IsArray => _value is TArray;

    public bool IsObject => _value is TObject;

    public bool Bool
    {
        get => _value is bool value ? value : throw new InvalidCastException(ERR_NOT_TYPE<bool>());
        set => _value = value;
    }

    public long Long
    {
        get => _value is long value ? value : throw new InvalidCastException(ERR_NOT_TYPE<long>());
        set => _value = value;
    }

    public ulong ULong
    {
        get => _value is long value ? (ulong)value : throw new InvalidCastException(ERR_NOT_TYPE<ulong>());
        set => _value = (long)value;
    }

    public int Int
    {
        get => _value is long value ? (int)value : throw new InvalidCastException(ERR_NOT_TYPE<int>());
        set => _value = (long)value;
    }

    public uint UInt
    {
        get => _value is long value ? (uint)value : throw new InvalidCastException(ERR_NOT_TYPE<uint>());
        set => _value = (long)value;
    }

    public short Short
    {
        get => _value is long value ? (short)value : throw new InvalidCastException(ERR_NOT_TYPE<short>());
        set => _value = (long)value;
    }

    public ushort UShort
    {
        get => _value is long value ? (ushort)value : throw new InvalidCastException(ERR_NOT_TYPE<ushort>());
        set => _value = (long)value;
    }

    public sbyte SByte
    {
        get => _value is long value ? (sbyte)value : throw new InvalidCastException(ERR_NOT_TYPE<sbyte>());
        set => _value = (long)value;
    }

    public byte Byte
    {
        get => _value is long value ? (byte)value : throw new InvalidCastException(ERR_NOT_TYPE<byte>());
        set => _value = (long)value;
    }

    public double Double
    {
        get => _value is double value ? value : throw new InvalidCastException(ERR_NOT_TYPE<double>());
        set => _value = value;
    }

    public float Float
    {
        get => _value is double value ? (float)value : throw new InvalidCastException(ERR_NOT_TYPE<float>());
        set => _value = (double)value;
    }

    public string String
    {
        get => _value is string value ? value : throw new InvalidCastException(ERR_NOT_TYPE<string>());
        set => _value = value;
    }

    public char Char
    {
        get => _value is string value ? value[0] : throw new InvalidCastException(ERR_NOT_TYPE<char>());
        set => _value = value.ToString();
    }

    public TArray Array
    {
        get => _value is TArray list ? list : throw new InvalidCastException(ERR_NOT_OBJECT);
        set => _value = value;
    }

    public TObject Object
    {
        get => _value is TObject dict ? dict : throw new InvalidCastException(ERR_NOT_OBJECT);
        set => _value = value;
    }

    public int Count
    {
        get
        {
            if (_value is TArray arr)
            {
                return arr.Count;
            }
            else if (_value is TObject obj)
            {
                return obj.Count;
            }
            else
            {
                throw new InvalidOperationException(ERR_NOT_ARRAY_OR_OBJECT);
            }
        }
    }

    public bool IsReadOnly => (_value is IObject obj && obj.IsReadOnly) || (_value is IArray arr && arr.IsReadOnly);

    public ICollection<string> Keys => _value is IObject obj ? obj.Keys : throw new InvalidOperationException(ERR_NOT_OBJECT);

    public ICollection<Node> Values => _value is IObject obj ? obj.Values : throw new InvalidOperationException(ERR_NOT_OBJECT);

    public ICollection<KeyValuePair<string, Node>> Items => _value is IObject obj ? this : throw new InvalidOperationException(ERR_NOT_OBJECT); 

    public void Add(Node item)
    {
        if (_value is TArray arr)
        {
            arr.Add(item);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_ARRAY);
        }
    }

    public void Add(string key, Node value)
    {
        if (_value is TObject obj)
        {
            obj.Add(key, value);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_OBJECT);
        }
    }

    public void Add(KeyValuePair<string, Node> item)
    {
        if (_value is IObject obj)
        {
            obj.Add(item);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_OBJECT);
        }
    }

    public void Clear()
    {
        if (_value is TArray arr)
        {
            arr.Clear();
        }
        else if (_value is TObject obj)
        {
            obj.Clear();
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_ARRAY_OR_OBJECT);
        }
    }

    public bool Contains(Node item)
    {
        if (_value is TArray arr)
        {
            return arr.Contains(item);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_ARRAY);
        }
    }

    public bool Contains(KeyValuePair<string, Node> item)
    {
        if (_value is TObject obj)
        {
            return obj.Contains(item);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_OBJECT);
        }
    }

    public bool ContainsKey(string key)
    {
        if (_value is TObject obj)
        {
            return obj.ContainsKey(key);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_OBJECT);
        }
    }

    public void CopyTo(Node[] array, int arrIndex)
    {
        if (_value is TArray arr)
        {
            arr.CopyTo(array, arrIndex);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_ARRAY);
        }
    }

    public void CopyTo(KeyValuePair<string, Node>[] array, int arrIndex)
    {
        if (_value is IObject obj)
        {
            obj.CopyTo(array, arrIndex);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_OBJECT);
        }
    }

    public IEnumerator<Node> GetEnumerator()
    {
        if (_value is TArray arr)
        {
            return arr.GetEnumerator();
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_ARRAY);
        }
    }

    public int IndexOf(Node item)
    {
        if (_value is TArray arr)
        {
            return arr.IndexOf(item);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_ARRAY);
        }
    }

    public void Insert(int index, Node item)
    {
        if (_value is TArray arr)
        {
            arr.Insert(index, item);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_ARRAY);
        }
    }

    public bool Remove(Node item)
    {
        if (_value is TArray arr)
        {
            return arr.Remove(item);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_ARRAY);
        }
    }

    public bool Remove(string key)
    {
        if (_value is TObject obj)
        {
            return obj.Remove(key);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_OBJECT);
        }
    }

    public bool Remove(KeyValuePair<string, Node> item)
    {
        if (_value is IObject obj)
        {
            return obj.Remove(item);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_OBJECT);
        }
    }

    public void RemoveAt(int index)
    {
        if (_value is TArray arr)
        {
            arr.RemoveAt(index);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_ARRAY);
        }
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out Node value)
    {
        if (_value is TObject obj)
        {
            return obj.TryGetValue(key, out value);
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_OBJECT);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        if (_value is TArray arr)
        {
            return arr.GetEnumerator();
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_ARRAY);
        }
    }

    IEnumerator<KeyValuePair<string, Node>> IEnumerable<KeyValuePair<string, Node>>.GetEnumerator()
    {
        if (_value is TObject obj)
        {
            return obj.GetEnumerator();
        }
        else
        {
            throw new InvalidOperationException(ERR_NOT_OBJECT);
        }
    }
}