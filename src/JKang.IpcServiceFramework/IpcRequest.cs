namespace JKang.IpcServiceFramework
{
    internal class IpcRequest
    {
        public string InterfaceName { get; set; }
        public string MethodName { get; set; }
        public object[] Parameters { get; set; }
    }
}