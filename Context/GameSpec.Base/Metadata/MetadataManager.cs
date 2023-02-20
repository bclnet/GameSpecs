namespace GameSpec.Metadata
{
    public abstract class MetadataManager
    {
        public abstract object FolderIcon { get; }
        public abstract object PackageIcon { get; }
        public abstract object GetIcon(string name);
        public abstract object GetImage(string name);
    }
}
