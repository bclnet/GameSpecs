namespace GameSpec.Explorer
{
    public abstract class ExplorerManager
    {
        public abstract object FolderIcon { get; }
        public abstract object PackageIcon { get; }
        public abstract object GetIcon(string name);
        public abstract object GetImage(string name);
    }
}
