namespace CMI.Manager.Index.Compiler
{
    public interface IDynamicScriptProvider
    {
        T GetInstanceByType<T>();
    }
}
