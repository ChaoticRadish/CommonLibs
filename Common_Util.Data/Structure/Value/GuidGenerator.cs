using Common_Util.Data.Mechanisms;
using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Value
{
    public interface IGuidGenerator<TArgs> : IGenerator<TArgs, Guid>
    {
    }

    public static class GuidGeneratorHelper
    {
        public static IGuidGenerator<TArgs> Create<TArgs>(Func<TArgs, Guid> generator)
        {
            return new FuncGuidGenerator<TArgs>(generator);
        }
        private sealed class FuncGuidGenerator<TArgs>(Func<TArgs, Guid> generator) : IGuidGenerator<TArgs>
        {
            private readonly Func<TArgs, Guid> generator = generator;

            public Guid Generating(TArgs args)
            {
                return generator(args);
            }
        }
    }
}
