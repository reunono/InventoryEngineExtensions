public interface IOverridable
{
    public IOverride NewOverride();
    public IOverridable WithOverride(IOverride @override) => @override.Apply(this);
}
