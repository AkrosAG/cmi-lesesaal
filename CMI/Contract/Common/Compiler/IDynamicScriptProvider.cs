namespace CMI.Contract.Common.Compiler
{
    public interface IDynamicScriptProvider
    {
        T GetInstanceByType<T>();
    }
}
