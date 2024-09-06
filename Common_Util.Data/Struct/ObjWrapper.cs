using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    /// <summary>
    /// 对象包装器, 一般用于在不允许传入null的情况下, 传入默认值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjWrapper<T> : IEquatable<ObjWrapper<T>>
    {
        /// <summary>
        /// 取得代表默认值的包装器
        /// </summary>
        public static ObjWrapper<T> Default { get; set; } = new ObjWrapper<T>();

        private ObjWrapper() { }
        public ObjWrapper(T? obj)
        {
            Obj = obj;
        }

        /// <summary>
        /// 被包装的对象
        /// </summary>
        public T? Obj { get; set; }

        public bool Equals(ObjWrapper<T>? other)
        {
            if (Obj == null && (other == null || other.Obj == null)) return true;
            if (Obj != null && (other == null || other.Obj == null)) return false;
            if (Obj == null && (other != null && other.Obj != null)) return false;
            if (Obj != null && (other != null && other.Obj != null))
            {
                return Obj.Equals(other.Obj);
            }
            return base.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                if (Obj == null)
                {
                    return true;
                }
                else
                {
                    return base.Equals(obj);
                }
            }
            else
            {
                if (obj is ObjWrapper<T> wrapper)
                {
                    return Equals(wrapper);
                }
                else
                {
                    return base.Equals(obj);
                }
            }
        }

        public override int GetHashCode()
        {
            if (Obj == null) return 0;
            else
            {
                return Obj.GetHashCode();
            }
        }


        //#region 隐式转换
        //public static implicit operator T?(ObjWrapper<T> wrapper)
        //{
        //    return wrapper == null ? default : wrapper.Obj;
        //}
        //public static implicit operator ObjWrapper<T>(T? obj)
        //{
        //    return new(obj);
        //}
        //#endregion
    }
}
