namespace JKang.IpcServiceFramework.Hosting.Abstractions
{
    public abstract class IpcEndpointOptions
    {
        public int MaxConcurrentCalls { get; set; } = 4;

        public bool IncludeFailureDetailsInResponse { get; set; }
    }
}
