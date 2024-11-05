using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.DynamicIL.Packet
{
    internal class SetCacheTypeGenerator(Type targetType, PacketTypeGenerateConfigure configure)
    {
        private readonly Type targetType = targetType;
        private readonly PacketTypeGenerateConfigure configure = configure;
        public Type Build()
        {
            throw new NotImplementedException();
        }
    }
}
