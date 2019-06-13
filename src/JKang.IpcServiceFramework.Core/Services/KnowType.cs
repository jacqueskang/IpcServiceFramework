using System;

namespace JKang.IpcServiceFramework.Services
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class KnowType : Attribute
    {
        public Type Type { get; }

        public KnowType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            
            if (type.IsClass && !type.IsAbstract)
            {
                Type = type;
            }
        }
    }
}
