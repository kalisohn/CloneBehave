using System;

namespace CloneBehave
{
    public enum DeepCloneBehavior
    {
        Default = 0,
        Reinitialize = 1,
        UpdateReferences = 2,
        Shallow = 3,
        SetToDefault = 4,       //Sets the cloned item to its default value (instead of cloning it)
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DeepCloneAttribute : Attribute
    {
        public DeepCloneAttribute(DeepCloneBehavior behavior)
        {
            Behavior = behavior;
        }

        public DeepCloneBehavior Behavior { get; set; }
    }
}