namespace OpenStack.Configuration
{
    public enum SettingElementType
    {
        Unknown,

        Configuration,

        /** ---- Known sections --- **/

        ActivePackageSource,

        BindingRedirects,

        Config,

        PackageManagement,

        PackageRestore,

        PackageSourceCredentials,

        PackageSources,

        /** ---- Known items --- **/

        Add,

        Author,

        Certificate,

        Clear,

        Owners,

        Repository,

        FileCert,

        StoreCert,

        /** Package Source Mapping **/

        PackageSourceMapping,

        PackageSource,

        Package,
    }
}
