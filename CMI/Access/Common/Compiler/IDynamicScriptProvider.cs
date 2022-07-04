namespace CMI.Access.Common.Compiler
{
    public interface IDynamicScriptProvider
    {
        T GetInstanceByType<T>();
    }
}
