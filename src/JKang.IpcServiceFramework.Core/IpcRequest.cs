namespace JKang.IpcServiceFramework
{
    public class IpcRequest
    {
        public string InterfaceName { get; set; }
        public string MethodName { get; set; }
        public object[] Parameters { get; set; }
    }
}