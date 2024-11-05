using Common_Util.Exceptions.General;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.DynamicIL.Packet
{
    /// <summary>
    /// 报文包 (<see langword="struct"/>) 创建器的帮助类
    /// </summary>
    public static class PacketCreatorHelper
    {

        /// <summary>
        /// 使用传入配置 <paramref name="configure"/> 生成报文包的实现类型
        /// </summary>
        /// <typeparam name="TPacket">需传入接口类型, 作为报文包的约定</typeparam>
        /// <param name="configure">如果为 <see langword="null"/>, 则采用缺省配置 <see cref="PacketTypeGenerateConfigure.Default"/></param>
        public static void Generate<TPacket>(PacketTypeGenerateConfigure? configure = null)
        {
            configure ??= PacketTypeGenerateConfigure.Default;
            ArgumentNullException.ThrowIfNull(configure);

            Type type = typeof(TPacket);
            _dic.AddOrUpdate(type,
                t => generate(t, configure),
                (t, old) => generate(t, configure)
                );
        }

        /// <summary>
        /// 取得特定报文包 <typeparamref name="TPacket"/> 的创建器
        /// </summary>
        /// <typeparam name="TPacket"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IPacketCreator<TPacket> Get<TPacket>()
        {
            if (!_dic.TryGetValue(typeof(TPacket), out var impl))
            {
                throw new TypeNotSupportedException(typeof(TPacket), "未生成类型对应的实现");
            }
            return Activator.CreateInstance(impl.CreatorImpl) as IPacketCreator<TPacket>
                ?? throw new InvalidOperationException("实例化创建器的实现失败! ");
        }

        #region 动态生成出来的类型
        private static ConcurrentDictionary<Type, ImplType> _dic = [];

        private struct ImplType
        {
            public Type CreatorImpl { get; set; }

            public Type DataImpl { get; set; }

            public Type SetCacheImpl { get; set; }
        }

        #endregion

        #region 动态生成
        private static ImplType generate(Type type, PacketTypeGenerateConfigure configure)
        {
            if (!type.IsInterface)
            {
                throw new TypeNotSupportedException(type, "需要传入一个接口类型作为生成模板");
            }
            return new ImplType()
            {
                CreatorImpl = new CreatorTypeGenerator(type, configure).Build(),
                DataImpl = new DataTypeGenerator(type, configure).Build(),
                SetCacheImpl = new SetCacheTypeGenerator(type, configure).Build(),
            };
        }

        #endregion

    }

    #region 配置

    /// <summary>
    /// 报文包相关类型的生成配置
    /// </summary>
    public class PacketTypeGenerateConfigure
    {
        /// <summary>
        /// 缺省配置
        /// </summary>
        public static PacketTypeGenerateConfigure Default { get; } = new()
        {

        };



    }

    /// <summary>
    /// 报文包各字段排列的方式
    /// </summary>
    public enum PacketPropertyLayout
    {
        /// <summary>
        /// 顺序紧凑地排列, 原始数据分割为若干段后, 与各属性一一对应
        /// </summary>
        SequenceCompact,
        /// <summary>
        /// 固定总长度, 在固定的位置读取属性的数据
        /// </summary>
        Fixed,
    }

    #endregion


    #region 接口

    /// <summary>
    /// 使用创建器创建报文包的过程中, 临时缓存数据到创建器内的设置处理委托
    /// </summary>
    /// <typeparam name="TPacket"></typeparam>
    /// <param name="packet"></param>
    public delegate void PacketDataSetHandler<TPacket>(ref TPacket packet);

    /// <summary>
    /// 报文包 (<see langword="struct"/>) 创建器
    /// </summary>
    /// <typeparam name="TPacket">报文包定义</typeparam>
    public interface IPacketCreator<TPacket>
    {
        IPacketCreator<TPacket> New();
        IPacketCreator<TPacket> Set(PacketDataSetHandler<TPacket> setHandler);
        TPacket Create();
    }
    /// <summary>
    /// 报文包数据
    /// </summary>
    public interface IPacketData
    {
        /// <summary>
        /// 取得报文包的原始数据
        /// </summary>
        /// <returns></returns>
        byte[] GetData();
    }
    #endregion

}
