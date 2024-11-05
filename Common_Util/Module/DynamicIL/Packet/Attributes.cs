using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.DynamicIL.Packet
{
    /// <summary>
    /// 标记一个接口在使用 <see cref="PacketCreatorHelper"/> 生成报文包相关类型时, 需要以什么样的布局布置数据
    /// </summary>
    /// <param name="layout"></param>
    [AttributeUsage(AttributeTargets.Interface)]
    public class PropertyLayout(PacketPropertyLayout layout) : Attribute
    {
        public PacketPropertyLayout Layout { get; } = layout;
    }

    /// <summary>
    /// 标记一个属性在 <see cref="PacketPropertyLayout.SequenceCompact"/> 的情况下的排列顺序索引
    /// </summary>
    /// <remarks>
    /// 从 0 开始, 带有标记的属性会使用传入的固定值, 剩余的将会按顺序取得不重复的允许的值. <br/>
    /// 例如一共有 5 个属性, 其中第 3 个被标记为 4, 第四个被标记为 1, 那么其对应关系将会是: <br/>
    /// 属性一 -> 0, 属性二 -> 2, 属性三 -> 4, 属性四 -> 1, 属性五 -> 3
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexAttribute : Attribute
    {
        public IndexAttribute(ushort index)
        {
            Index = index;
        }
        public int Index { get; }
    }

    /// <summary>
    /// 标记一个属性占用的长度
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LengthAttribute : Attribute
    {
        public LengthAttribute(uint length)
        {
            Length = length;
            PropertyName = string.Empty;
            UseFixedLength = true;
        }
        public LengthAttribute(string propertyName)
        {
            PropertyName = propertyName;
            Length = 0;
            UseFixedLength = false;
        }

        /// <summary>
        /// 是否使用固定长度, 如果是, 则使用 <see cref="Length"/> 的值, 反之则尝试取得名为 <see cref="PropertyName"/> 的属性的值
        /// </summary>
        public bool UseFixedLength { get; }
        public uint Length { get; }
        public string PropertyName { get; } 
    }
}
