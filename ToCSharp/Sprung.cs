using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ToCSharp
{
    // Read all spring objects
    // for each one, iterate over properties, and find appropriate constructor
    // output string rep. of instantiation
    //
    // also for each nested object, save it to a different file/class, such that the objects
    // can be reused and shared.
    public class Sprung
    {
        public static void ToCSharp<T>(string springObjectName)
        {
            var obj = SpringFactory.Instance.GetByName<T>(springObjectName);
            var type = typeof(T);

            // 4. Write to File
            //    Based off object name
            var result = "public class " + springObjectName + "{" +
                         "public static " + FullTypeName(type) + " Get(){" +
                         "return " + Build(obj) + ";" +
                         "}}";

            ToFile(result, name: springObjectName);
        }

        public static string Build(dynamic obj)
        {
            var type = obj.GetType();
            var typeName = type.Name;
            var typeArgs = new List<string>();
            var paramList = new List<string>();
            string result;

            // 1. Find Fields
            var fields = type.GetFields(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Static);
            // 2. Find Constructor that has as many of the fields as possible
            var constructors = type.GetConstructors();
            var constructor = BestConstructor(fields,
                                              constructors);


            // 3. Build string of instantiation
            //   3.1 - See if any fields are complex objects (if so, go back to 1)
            //   3.2 - to string

            if (constructor != null)
            {
                var parameters = constructor.GetParameters();
                foreach (var param in parameters)
                {
                    var paramString = "";
                    var name = param.Name;
                    var value = ValueFromFields(fields,
                                                name,
                                                obj);

                    var valueAsString = AsString(value);
                    paramString = name + ": " + valueAsString;


                    paramList.Add(paramString);
                }

                result = "new " + FullTypeName(type) + "(" +
                   string.Join(", ", paramList.ToArray()) +
                   ")";

            }
            else
            {
                // no constructor usually will mean that we are dealing with a collection
                foreach (var value in obj)
                {
                    var valueAsString = AsString(value);
                    paramList.Add(valueAsString);
                }

                result = "new " + FullTypeName(type) + "(){" +
                    string.Join(", ", paramList.ToArray()) +
                    "}";

            }

            return result;
        }

        public static string AsString(dynamic obj)
        {
            if (obj == null)
            {
                return "null";
            }

            var type = obj.GetType();

            if (obj is DateTime)
            {
                var date = (DateTime)obj;
                return "new DateTime(" +
                       date.Year + ", " +
                       date.Month + ", " +
                       date.Day + ", " +
                       date.Hour + ", " +
                       date.Minute + ", " +
                       date.Second + ")";
            }

            if (!IsPrimitive(type))
            {
                // recurse on the value
                return Build(obj);
            }

            if (obj is Enum)
            {
                return type.Name + "." + obj.ToString();
            }

            if (obj is bool)
            {
                return ((bool)obj).ToString().ToLower();
            }

            if (type == typeof(string))
            {
                return "\"" + obj + "\"";
            }

            return obj.ToString();
        }

        public static string FullTypeName(Type t)
        {
            var typeName = t.Name;
            var typeArgs = new List<string>();

            foreach (var gt in t.GenericTypeArguments)
            {
                if (gt.Name == "Double")
                {
                    typeArgs.Add("double");
                }
                else if (gt.Name == "String")
                {
                    typeArgs.Add("string");
                }
                else
                {
                    typeArgs.Add(gt.Name);
                }

            }

            var typeNameParts = typeName.Split('`');
            typeName = typeNameParts[0];



            var result = typeName;

            if (typeNameParts.Count() <= 1) return result;

            var numberOfTypeArguments = typeNameParts[1];

            if (Convert.ToInt32(numberOfTypeArguments) > 0)
            {
                result = result + "<" +
                         string.Join(", ",
                             typeArgs.ToArray()) + ">";
            }


            return result;
        }

        public static bool IsPrimitive(Type t)
        {
            return (
                t.IsPrimitive ||
                t.IsValueType ||
                (t == typeof(string))
            );
        }

        public static dynamic ValueFromFields(FieldInfo[] fields, string name, dynamic source)
        {
            var publicName = VariableNameToPublicName(name);
            var variableName = PublicNameToVariableName(publicName);

            var applicableField = (
                    from field in fields
                    where field.Name == name ||
                        field.Name == publicName ||
                        field.Name == variableName
                    select field
                ).First();

            var fieldType = applicableField.FieldType;
            // Check if we are a generic list -> convert to a real one
            if (fieldType.IsGenericType)
            {
                var typeParams = fieldType.GetGenericArguments();
                var listType = typeParams[0];

                // if we have two, we have a dictionary, probably
                // maybe we want to do something hacky, and check if
                // fieldType.Name Contains IReadOnlyList
                if (typeParams.Count() == 1)
                {
                    var oldValue = applicableField.GetValue(source);
                    // http://stackoverflow.com/a/9860408/356849
                    Type typeOfList = typeof(List<>).MakeGenericType(new[] { listType });
                    IList list = (IList)Activator.CreateInstance(typeOfList);

                    foreach (var element in oldValue)
                    {
                        list.Add(element);
                    }

                    return list;
                }
            }

            return applicableField.GetValue(source);
        }

        /// <summary>
        /// converts to camelCase from PascalCase
        /// </summary>
        /// <param name="publicName"></param>
        /// <returns></returns>
        public static string PublicNameToVariableName(string publicName)
        {
            var result = char.ToLower(
                publicName[0]) + publicName.Substring(1);

            return result;
        }

        /// <summary>
        /// converts PascalCase to camelCase
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static string VariableNameToPublicName(string variableName)
        {
            var result = char.ToUpper(
                variableName[0]) + variableName.Substring(1);

            return result;
        }

        public static void ToFile(string output, string name)
        {
            var path = Directory.GetCurrentDirectory() + "/../../../../" + name + ".cs";
            Console.WriteLine(path);
            File.WriteAllText(path, output);
        }

        public static ConstructorInfo BestConstructor(
            FieldInfo[] fields,
            ConstructorInfo[] constructors)
        {
            if (constructors.Count() == 1)
            {
                return constructors.First();
            }

            // find the best constructor,
            // having the most in common with the fields
            ConstructorInfo bestConstructor = null;
            // we want this to be as close to 0 as possible
            var similarity = double.PositiveInfinity;

            foreach (var constructor in constructors)
            {
                var numFields = fields.Count();
                var numArgs = constructor.GetParameters().Count();

                var currentSimilarity = numFields - numArgs;
                if (currentSimilarity <= 0)
                {
                    similarity = currentSimilarity;
                    bestConstructor = constructor;
                }
            }

            return bestConstructor;
        }
    }
}