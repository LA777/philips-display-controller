namespace ArgumentParser
{
    public interface IArgument
    {
        string Name { get; }
        string ShortName { get; }
        string Description { get; }
        string Parameter { get; set; }
        void Action();
    }
}
