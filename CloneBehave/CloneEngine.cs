using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fasterflect;

namespace CloneBehave
{
    public class CloneEngine
    {
        private readonly MethodInfo _cloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly IDictionary<Type, IList<ReflectedField>> _reflectedTypes = new Dictionary<Type, IList<ReflectedField>>(); //also used as a cache for second clone run.
        private MethodInvoker _delegateForCallMethod;
        private bool _flagUpdateReferences;

        //The references to update will be stored here
        //Item1 = the field to update
        //Item2 = the original object
        //Item3 = the cloned object
        private IList<Tuple<ReflectedField, object, object>> _referencesToUpdate;

        private IDictionary<object, object> _tempVisited;
        private IDictionary<object, object> _visited;

        public T Clone<T>(T original)
        {
            _delegateForCallMethod = _cloneMethod.DelegateForCallMethod();
            _flagUpdateReferences = false;
            _visited = new Dictionary<object, object>();
            _tempVisited = new Dictionary<object, object>();
            _referencesToUpdate = new List<Tuple<ReflectedField, object, object>>();

            if (IsPrimitive(original.GetType()))
            {
                return original;
            }

            ResolveStaticFields(original.GetType());

            object copy = InternalCopy(original);
            _flagUpdateReferences = true;
            UpdateReferences();
            return (T)copy;
        }

        private void CloneFieldValue(object cloneObject, ReflectedField rField, object originalFieldValue)
        {
            object clonedFieldValue;

            if (rField.Behaviour == DeepCloneBehavior.Reinitialize)
            {
                object visitedObject;
                if (_visited.TryGetValue(originalFieldValue, out visitedObject))
                {
                    clonedFieldValue = visitedObject;
                }
                else
                {
                    if (rField.InitValue == null)
                    {
                        clonedFieldValue = GetDefault(rField.FieldInfo.FieldType);
                        rField.InitValue = clonedFieldValue;
                        _visited.Add(originalFieldValue, clonedFieldValue);
                    }
                    else
                    {
                        clonedFieldValue = InternalCopy(rField.InitValue);
                        _visited.Add(originalFieldValue, clonedFieldValue);
                    }
                }
            }
            else if (rField.Behaviour == DeepCloneBehavior.SetToDefault)
            {
                if (originalFieldValue.GetType().IsValueType)
                {
                    clonedFieldValue = Activator.CreateInstance(originalFieldValue.GetType());
                }
                else
                {
                    clonedFieldValue = null;
                }
            }
            else
            {
                clonedFieldValue = InternalCopy(originalFieldValue);
            }

            try
            {
                rField.MemberSetter(cloneObject, clonedFieldValue);
            }
            catch (InvalidCastException)
            {
                rField.FieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }

        private void CopyFields(object originalObject, object cloneObject, Type typeToReflect)
        {
            IList<ReflectedField> affectedFields = GetAffectedFields(typeToReflect);

            for (int i = 0; i < affectedFields.Count; i++)
            {
                ReflectedField rField = affectedFields[i];

                object originalFieldValue;

                //If fasterflect cannot resolve the type use native GetValue
                try
                {
                    originalFieldValue = rField.MemberGetter(originalObject);
                }
                catch (InvalidCastException)
                {
                    originalFieldValue = rField.FieldInfo.GetValue(originalObject);
                }

                if (_flagUpdateReferences == false && rField.Behaviour == DeepCloneBehavior.UpdateReferences)
                {
                    _referencesToUpdate.Add(Tuple.Create(rField, originalFieldValue, cloneObject));
                    continue;
                }

                if (originalFieldValue == null)
                {
                    continue;
                }

                CloneFieldValue(cloneObject, rField, originalFieldValue);
            }
        }

        private IList<ReflectedField> GetAffectedFields(Type typeToReflect)
        {
            IList<ReflectedField> affectedFields;

            if (!_reflectedTypes.TryGetValue(typeToReflect, out affectedFields))
            {
                affectedFields = new List<ReflectedField>();
                FieldInfo[] fieldInfos = typeToReflect.Fields().Where(fi => !IsPrimitive(fi.FieldType)).ToArray();

                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    FieldInfo fieldInfo = fieldInfos[i];

                    DeepCloneBehavior dcb = GetDeepCloneBehaviour(fieldInfo);

                    if (dcb != DeepCloneBehavior.Shallow)
                    {
                        affectedFields.Add(new ReflectedField
                        {
                            FieldInfo = fieldInfo,
                            MemberGetter = fieldInfo.DelegateForGetFieldValue(),
                            MemberSetter = fieldInfo.DelegateForSetFieldValue(),
                            Behaviour = dcb
                        });
                    }
                }

                _reflectedTypes.Add(typeToReflect, affectedFields);
            }

            return affectedFields;
        }

        private DeepCloneBehavior GetDeepCloneBehaviour(FieldInfo field)
        {
            object[] customAttributes = field.GetCustomAttributes(typeof(DeepCloneAttribute), false);

            if (!customAttributes.Any())
            {
                return DeepCloneBehavior.Default;
            }
            return ((DeepCloneAttribute)customAttributes[0]).Behavior;
        }

        private object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition().MakeGenericType(type.GetGenericArguments());
                return Activator.CreateInstance(genericType);
            }

            return null;
        }

        private object InternalCopy(object originalObject)
        {
            if (originalObject == null)
            {
                return null;
            }

            object visitedObject;
            if (_visited.TryGetValue(originalObject, out visitedObject))
            {
                return visitedObject;
            }

            Type typeToReflect = originalObject.GetType();

            if (typeof(Delegate).IsAssignableFrom(typeToReflect))
            {
                return null;
            }

            object cloneObject = _delegateForCallMethod(originalObject, null);

            if (typeToReflect.IsArray)
            {
                Type arrayType = typeToReflect.GetElementType();
                if (!IsPrimitive(arrayType))
                {
                    Array clonedArray = (Array)cloneObject;
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices)), indices));
                }
            }

            _visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, cloneObject, typeToReflect);

            return cloneObject;
        }

        private bool IsPrimitive(Type type)
        {
            if (type == typeof(String))
            {
                return true;
            }
            return (type.IsValueType & type.IsPrimitive);
        }

        private void ResolveStaticFields(Type typeToReflect)
        {
            if (_tempVisited.ContainsKey(typeToReflect)) return;
            _tempVisited.Add(typeToReflect, typeToReflect);

            if (typeToReflect.IsArray)
            {
                ResolveStaticFields(typeToReflect.GetElementType());
            }
            else if (typeToReflect.IsGenericType)
            {
                foreach (var genericType in typeToReflect.GetGenericArguments())
                {
                    ResolveStaticFields(genericType);
                }
            }

            IList<FieldInfo> fieldInfos = typeToReflect.Fields(Flags.StaticInstanceAnyVisibility).Where(f => !IsPrimitive(f.FieldType)).ToList();
            for (int i = 0; i < fieldInfos.Count; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
                ResolveStaticFields(fieldInfo.FieldType);

                if (fieldInfo.IsStatic)
                {
                    object o = fieldInfo.GetValue(null);
                    object visitedObject;
                    if (o != null && !_visited.TryGetValue(o, out visitedObject))
                    {
                        _visited.Add(o, o);
                    }
                }
            }
        }

        private void UpdateReferences()
        {
            for (int i = 0; i < _referencesToUpdate.Count; i++)
            {
                Tuple<ReflectedField, object, object> refToUpdate = _referencesToUpdate[i];

                if (refToUpdate.Item2 != null)
                {
                    object clonedFieldValue; //can be null and thats ok
                    _visited.TryGetValue(refToUpdate.Item2, out clonedFieldValue);

                    if (clonedFieldValue == null)
                    {
                        clonedFieldValue = InternalCopy(refToUpdate.Item2);
                    }
                    ReflectedField rField = refToUpdate.Item1;

                    try
                    {
                        rField.MemberSetter(refToUpdate.Item3, clonedFieldValue);
                    }
                    catch (InvalidCastException)
                    {
                        rField.FieldInfo.SetValue(refToUpdate.Item3, clonedFieldValue);
                    }
                }
            }
        }
    }
}