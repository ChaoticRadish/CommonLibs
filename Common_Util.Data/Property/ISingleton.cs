using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Property
{
    public interface ISingleton<T> 
    {
        static T GetInstance() 
        {
            return TypeHelper.Singleton<T>();
        }
    }
}
