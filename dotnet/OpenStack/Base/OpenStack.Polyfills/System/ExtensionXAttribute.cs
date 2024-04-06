namespace System
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ExtensionXAttribute : Attribute
    {
        public string Extension { get; }
        public ExtensionXAttribute(string extension) => Extension = extension;
    }
}
