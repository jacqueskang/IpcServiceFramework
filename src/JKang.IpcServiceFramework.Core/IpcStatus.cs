namespace JKang.IpcServiceFramework
{
    public enum IpcStatus: int
    {
        Unknown = 0,
        Ok = 200,
        BadRequest = 400,
        InternalServerError = 500,
    }
}
