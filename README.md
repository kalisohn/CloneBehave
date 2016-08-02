# Clone Behave!
A performant and flexible deep clone engine for C#

Download as [NuGet Package](https://www.nuget.org/packages/Clone.Behave/)

## Introduction

Clone Behave! uses reflection with the help of [fasterflect](https://fasterflect.codeplex.com/) that offers methods for creating delegates and that improves reflection performance. It also contains some parts from https://github.com/Burtsev-Alexey/net-object-deep-copy/blob/master/ObjectExtensions.cs (Thanks for the inspiration!)
It works with reflection caches and is safe against circular references. The special thing about Clone Behave! is the possibility of changing the behaviour of the cloned code by using annotations. E.g. you can define that fields to be cloned memberwise instead of deep cloned.
Due to object and type caches the cloner will be even faster if you clone your code for a second run.

## Limitations (v1.0)

By now Clone Behave! has some limitations. You should check them first to be sure that Clone Behave! fits your needs.

* EventHandlers will be set to null and must be registered manually
* EventHandlers registered to the original object need to be "moved" manually after cloning
* DeepCloneBehaviour attributes are only allowed for fields. When you want a special treatment for a property, you'll have to create a backing field.
* You should avoid using native Dictionaries because it slows down the performance dramatically. You can instead use CloneableDictionary from CloneBehave! That will wrap a native Dictionary.

## Prepare your Code

Clone Behave! lets you change the cloning behaviour of the code you want to clone. This happens by adding annotations to the fields you want a special treatment for.

### DeepCloneBehaviour: SetToDefault

The SetToDefault Attribute sets the field to its default value after the cloning process. (E.g. null or 0)

```c#
    public class Address 
    {
      [DeepClone(DeepCloneBehavior.SetToDefault)]
      private Person _personLivingHere;
      
      public Person PersonLivingHere 
      {
        get { return _personLivingHere; }
        set { _personLivingHere = value; }
      }
    }
    
    Address adr = new Address(); 
    adr.PersonLivingHere = new Person("John");
    
    Address adrClone = adr.Clone(); //adrClone.PersonLivingHere will be null
```    
    
### DeepCloneBehaviour: Shallow

The Shallow DeepCloneBehaviour will perform a simple Shallow Copy of the defined field instead of a deep clone.

```c#
    public class Person 
    {
      public string Name { get; set; }
      
      [DeepClone(DeepCloneBehavior.Shallow)]
      private Person _bestFriend;
      
      public Person BestFriend 
      { 
        get { return _bestFriend; }
        set { _bestFriend = value; }
      }
    
      public Person(string name) 
      {
        Name = name;
      }
    }

    public class Address 
    {
      private Person _personLivingHere;
      
      public Person PersonLivingHere 
      {
        get { return _personLivingHere; }
        set { _personLivingHere = value; }
      }
    }
    
    Address adr = new Address();
    adr.PersonLivingHere = new Person("John");
    adr.PersonLivingHere.BestFriend = new Person("James");
    
    Address adrClone = adr.Clone();
    adr.PersonLivingHere == adrClone.PersonLivingHere //false
    adr.PersonLivingHere.BestFriend == adrClone.PersonLivingHere.BestFriend //true
```
### DeepCloneBehaviour: UpdateReferences

Will perform a default deep clone but will put the cloning of the defined field to the end of the cloning process. 
That helps especially in situations like linked lists to avoid a stackoverflow because it will clone the object itself without cloning the UpdateReference marked field.
At the end of the cloning process it will then just fill the references with the already cloned objects.
```c#
    public class Person 
    {
      [DeepClone(DeepCloneBehavior.UpdateReferences)]
      private Person _father;
      
      public string Name { get; set; }
      
      public Person Father 
      { 
        get { return _father; }
        set { _father = value; }
      } 
    
      public IList<Person> Children { get; set; }
    
      public Person(string name) 
      {
        Name = name;
      }
    }
    
    Person p = new Person("John");
    p.Clone();
```
