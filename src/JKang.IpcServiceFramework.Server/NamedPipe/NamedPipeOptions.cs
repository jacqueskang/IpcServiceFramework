namespace JKang.IpcServiceFramework
{
    public class NamedPipeOptions
    {
        public int ThreadCount { get; set; } = 4;
        public bool UseThreadNames { get; set; }
    }
}
