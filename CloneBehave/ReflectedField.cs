using System.Reflection;
using Fasterflect;

namespace CloneBehave
{
    public struct ReflectedField
    {
        public FieldInfo FieldInfo;
        public object InitValue;
        public MemberGetter MemberGetter;
        public MemberSetter MemberSetter;
        public DeepCloneBehavior Behaviour;
    }
}