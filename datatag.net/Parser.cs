namespace Datatag
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Globalization;
    using System.Linq;

    public static class Parser
    {
        private const string TOK_NULL = "null";
        private const string TOK_TRUE = "true";
        private const string TOK_FALSE = "false";
        private const char TOK_ARRAY_START = '[';
        private const char TOK_OBJECT_START = '{';
        private const char TOK_OBJECT_END = '}';
        private const char TOK_ARRAY_END = ']';
        private const char TOK_KEY_SEPARATOR = ':';
        private const char TOK_STRING_SEPARATOR = '"';
        private const string TOK_HEX_START = "0x";
        private const char TOK_ESCAPE_CHAR = '\\';
        private const char TOK_COMMENT_START = ';';
        public static readonly Regex RX_KEYS = new Regex(@"^([_a-zA-Z0-9]+)");

        public static Node Read(string input)
        {
            input = ConsumeComment(input);
            Node result;
            if (input.StartsWith(TOK_OBJECT_START.ToString()))
            {
                ConsumeObject(input, out result);
            }
            else if (input.StartsWith(TOK_ARRAY_START.ToString()))
            {
                ConsumeArray(input, out result);
            }
            else
            {
                result = Node.NewObject();
                while (input.Length > 0)
                {
                    input = ConsumeKeyValuePair(input, out var key, out var node);
                    result[key] = node;
                    input = ConsumeComment(input);
                }
            }
            return result;
        }

        public static Node ReadObject(object obj)
        {
            if (obj == null)
            {
                return new Node(null);
            }

            if (obj is Node node)
            {
                return node;
            }

            if (obj is IDatatagSerializable dts)
            {
                return dts.Serialize();
            }

            var type = obj.GetType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: return new Node(Convert.ToBoolean(obj));

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64: return new Node(Convert.ToInt64(obj));

                case TypeCode.Single:
                case TypeCode.Double: return new Node(Convert.ToDouble(obj));

                case TypeCode.Char:
                case TypeCode.String: return new Node(obj.ToString());

                default: break;
            }

            if (type.IsEnum)
            {
                var values = Enum.GetValues(type);
                var names = Enum.GetNames(type);
                var i = 0;
                foreach (var value in values)
                {
                    if (value.Equals(obj))
                    {
                        return new Node(names[i]);
                    }
                    i++;
                }
            }

            Node result;
            if (type.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()) && type.GetGenericArguments()[0] == typeof(string))
            {
                result = Node.NewObject();

                var keys = type.GetProperty("Keys").GetValue(obj) as IEnumerable<string>;
                var values = (type.GetProperty("Values").GetValue(obj) as IEnumerable).GetEnumerator();

                foreach (var key in keys)
                {
                    values.MoveNext();
                    result.Add(key, ReadObject(values.Current));
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                result = Node.NewArray();

                var valueType = type.GetGenericArguments()[0];
                var values = obj as IEnumerable;

                foreach (var value in values)
                {
                    result.Add(ReadObject(value));
                }
            }
            else
            {
                result = Node.NewObject();

                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField)
                                 .Where(f => (f.IsPublic && f.GetCustomAttribute<NonSerializedAttribute>(true) == null)
                                          || f.GetCustomAttribute<SerializableAttribute>(true) != null
                                          || f.FieldType.GetCustomAttribute<DatatagSerializableAttribute>(true) != null);
                foreach ((string name, object field) in fields.Select(f => (f.Name, f.GetValue(obj))))
                {
                    result.Add(name, ReadObject(field));
                }

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty)
                                     .Where(p => (p.GetMethod.IsPublic && p.GetCustomAttribute<NonSerializedAttribute>(true) == null && p.GetMethod.GetCustomAttribute<NonSerializedAttribute>(true) == null)
                                              || p.GetCustomAttribute<SerializableAttribute>(true) != null
                                              || p.GetMethod.GetCustomAttribute<SerializableAttribute>(true) != null
                                              || p.PropertyType.GetCustomAttribute<DatatagSerializableAttribute>(true) != null);
                foreach ((string name, object field) in properties.Select(f => (f.Name, f.GetValue(obj))))
                {
                    result.Add(name, ReadObject(field));
                }
            }
            return result;
        }

        public static string Write(Node node)
            => Write(node, EncodeSettings.Compact, default);

        public static string Write(Node node, EncodeSettings settings)
            => Write(node, settings, default);

        public static string Write<T>(T obj) where T : IDatatagSerializable
            => Write(obj.Serialize(), EncodeSettings.Compact, default);

        public static string Write<T>(T obj, EncodeSettings settings) where T : IDatatagSerializable
            => Write(obj.Serialize(), settings, default);

        public static string Write(object obj)
            => Write(ReadObject(obj), EncodeSettings.Compact, default);

        public static string Write(object obj, EncodeSettings settings)
            => Write(ReadObject(obj), settings, default);

        private static string Write(Node node, EncodeSettings settings, EncodeState existingState = null)
        {
            var state = existingState ?? new EncodeState();
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            var result = new List<string>();
            var whitespace = settings.ExpandArrays || settings.ExpandObjects ? " " : string.Empty;
            var newlineMinusOne = Environment.NewLine + whitespace.Repeat(settings.Indent * (state.Nesting.Count - 1));
            var newline = Environment.NewLine + whitespace.Repeat(settings.Indent * state.Nesting.Count);
            var newlinePlusOne = Environment.NewLine + whitespace.Repeat(settings.Indent * (state.Nesting.Count + 1));
            if (node.IsNull)
            {
                result.Add(TOK_NULL);
            }
            else if (node.IsBool)
            {
                result.Add(node.Bool.ToString());
            }
            else if (node.IsInt)
            {
                result.Add(node.Long.ToString());
            }
            else if (node.IsFloat)
            {
                result.Add(node.Double.ToString());
            }
            else if (node.IsString)
            {
                if (node.String.IndexOfAny(' ', '\t', '\r', '\n', '"') > -1)
                {
                    result.Add($"\"{Regex.Escape(node.String)}\"");
                }
                else
                {
                    result.Add(Regex.Escape(node.String));
                }
            }
            else if (node.IsArray)
            {
                state.Nesting.Add(EncodeState.NestingType.Array);
                result.Add(TOK_ARRAY_START.ToString());
                result.Add(settings.ExpandArrays ? newlinePlusOne : whitespace);
                var i = 0;
                foreach (var elem in node.Array)
                {
                    result.Add(Write(elem, settings, state));
                    result.Add(settings.ExpandArrays ? (i == node.Array.Count - 1 ? newline : newlinePlusOne) : elem.IsPrimitive && i < node.Array.Count - 1 ? " " : whitespace);
                    i++;
                }
                result.Add(TOK_ARRAY_END.ToString());
                state.Nesting.RemoveAt(state.Nesting.Count - 1);
            }
            else if (node.IsObject)
            {
                state.Nesting.Add(EncodeState.NestingType.Object);
                result.Add(TOK_OBJECT_START.ToString());
                result.Add(settings.ExpandObjects ? newlinePlusOne : whitespace);
                foreach (var pair in node.Object)
                {
                    result.Add(pair.Key);
                    result.Add(TOK_KEY_SEPARATOR.ToString());
                    result.Add(whitespace);
                    result.Add(Write(pair.Value, settings, state));
                    result.Add(settings.ExpandObjects ? newline : whitespace);
                }
                result.Add(TOK_OBJECT_END.ToString());
                state.Nesting.RemoveAt(state.Nesting.Count - 1);
            }
            return string.Join(string.Empty, result);
        }

        public static void WriteObject(object obj, Node value)
        {
            if (obj == null || obj is Node node)
            {
                return;
            }

            if (obj is IDatatagSerializable dts)
            {
                dts.Deserialize(value);
                return;
            }

            // TODO(dy): THE REST OF THIS FUNCTION
        }

        private static string ConsumeToken(string input, char token, bool consumeWhitespaceBefore = true, bool consumeWhitespaceAfter = true)
        {
            if (consumeWhitespaceBefore)
            {
                input = input.TrimStart();
            }
            if (input.StartsWith(token.ToString()))
            {
                input = input.Substring(1);
                return ConsumeComment(input, consumeWhitespaceBefore, consumeWhitespaceAfter);
            }
            throw new InvalidSyntaxException($"Expected token '{token}'");
        }

        private static string ConsumeToken(string input, string token)
        {
            input = input.TrimStart();
            if (input.StartsWith(token))
            {
                input = input.Substring(token.Length);
                return ConsumeComment(input);
            }
            throw new InvalidSyntaxException($"Expected token '{token}'");
        }

        private static string ConsumeKey(string input, out string key)
        {
            int i = 0;
            while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_'))
            {
                i++;
            }

            if (i > 0)
            {
                key = input.Substring(0, i);
                return input.Substring(i).TrimStart();
            }
            else
            {
                throw new InvalidSyntaxException("Expected key");
            }
        }

        private static string ConsumeValue(string input, out Node value)
        {
            value = null;
            input = input.TrimStart();
            if (input.StartsWith(TOK_NULL))
            {
                value = new Node(null);
                input = input.Substring(TOK_NULL.Length);
            }
            else if (input.StartsWith(TOK_TRUE))
            {
                value = new Node(true);
                input = input.Substring(TOK_TRUE.Length);
            }
            else if (input.StartsWith(TOK_FALSE))
            {
                value = new Node(false);
                input = input.Substring(TOK_FALSE.Length);
            }
            else if (input.StartsWith(TOK_ARRAY_START.ToString()))
            {
                input = ConsumeArray(input, out value);
            }
            else if (input.StartsWith(TOK_OBJECT_START.ToString()))
            {
                input = ConsumeObject(input, out value);
            }
            else
            {
                var valueEndIndex = input.IndexOfAny(' ', '\t', '\r', '\n', '}', ']', ';', '"');
                var upToValueEnd = input.Substring(0, valueEndIndex);
                if (upToValueEnd.StartsWith(TOK_HEX_START) && long.TryParse(upToValueEnd, NumberStyles.HexNumber, null, out var hexValue))
                {
                    value = new Node(hexValue);
                    input = input.Substring(upToValueEnd.Length);
                }
                if (long.TryParse(upToValueEnd, NumberStyles.Integer, null, out var intValue))
                {
                    value = new Node(intValue);
                    input = input.Substring(upToValueEnd.Length);
                }
                else if (double.TryParse(upToValueEnd, NumberStyles.Number, null, out var floatValue))
                {
                    value = new Node(floatValue);
                    input = input.Substring(upToValueEnd.Length);
                }
                else if (input.StartsWith(TOK_STRING_SEPARATOR.ToString()))
                {
                    input = ConsumeString(input, out value);
                }
                else
                {
                    value = new Node(Regex.Unescape(upToValueEnd));
                    input = input.Substring(upToValueEnd.Length);
                }
            }
            if (value != null)
            {
                return ConsumeComment(input);
            }
            else
            {
                throw new InvalidSyntaxException("Invalid value");
            }
        }

        public static string ConsumeObject(string input, out Node value)
        {
            value = Node.NewObject();

            input = ConsumeToken(input, TOK_OBJECT_START);

            while (true)
            {
                if (input.Length == 0)
                {
                    throw new InvalidSyntaxException($"Expected token '{TOK_OBJECT_END}'");
                }

                if (input.StartsWith(TOK_OBJECT_END.ToString()))
                {
                    input = ConsumeToken(input, TOK_OBJECT_END);
                    break;
                }

                input = ConsumeKeyValuePair(input, out var key, out var node);
                value[key] = node;
            }

            return ConsumeComment(input);
        }

        public static string ConsumeKeyValuePair(string input, out string key, out Node value)
        {
            input = ConsumeKey(input, out key);
            input = ConsumeToken(input, TOK_KEY_SEPARATOR);
            return ConsumeValue(input, out value);
        }

        public static string ConsumeArray(string input, out Node value)
        {
            value = Node.NewArray();

            input = ConsumeToken(input, TOK_ARRAY_START);

            while (true)
            {
                if (input.Length == 0)
                {
                    throw new InvalidSyntaxException($"Expected token '{TOK_ARRAY_END}'");
                }

                if (input.StartsWith(TOK_ARRAY_END.ToString()))
                {
                    input = ConsumeToken(input, TOK_ARRAY_END);
                    break;
                }

                input = ConsumeValue(input, out var node);
                value.Add(node);
                input = ConsumeComment(input);
            }
            return ConsumeComment(input);
        }

        public static string ConsumeString(string input, out Node value)
        {
            var str = "";

            input = input.TrimStart();
            input = ConsumeToken(input, TOK_STRING_SEPARATOR, false, false);

            var escaped = false;
            var end = -1;

            for (var i = 0; i < input.Length; ++i)
            {
                if (escaped)
                {
                    str += input[i];
                    escaped = false;
                }
                else if (input[i] == TOK_ESCAPE_CHAR)
                {
                    str += input[i];
                    escaped = true;
                }
                else if (input[i] == TOK_STRING_SEPARATOR)
                {
                    end = i;
                    break;
                }
                else
                {
                    str += input[i];
                }
            }
            if (end > 0)
            {
                value = new Node(Regex.Unescape(str));
                input = input.Substring(end + 1);
                return ConsumeComment(input);
            }
            throw new InvalidSyntaxException("Unterminated string");
        }

        public static string ConsumeComment(string input, bool consumeWhitespaceBefore = true, bool consumeWhitespaceAfter = true)
        {
            if (consumeWhitespaceBefore)
            {
                input = input.TrimStart();
            }
            if (input.Length > 0 && input.StartsWith(TOK_COMMENT_START.ToString()))
            {
                var skip = input.UpToNewline();
                input = input.Substring(skip.Length);
                if (consumeWhitespaceAfter)
                {
                    input = input.TrimStart();
                }
            }
            return input;
        }

    }

}