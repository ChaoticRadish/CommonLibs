using Common_Wpf.Converter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Common_Wpf.SettingPackage
{
    [TypeConverter(typeof(BaseBoxSettingCollectionConverter))]
    public struct BaseBoxSetting
    {
        public string Name { get; set; }

        public Brush? BackColor { get; set; }
        public Brush? ForeColor { get; set; }
        public Brush? BorderColor { get; set; }

        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is BaseBoxSetting other)
            {
                return Name == other.Name
                    && BackColor == other.BackColor
                    && ForeColor == other.ForeColor
                    && BorderColor == other.BorderColor;
            }
            return base.Equals(obj);
        }

        public static bool operator ==(BaseBoxSetting left, BaseBoxSetting right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BaseBoxSetting left, BaseBoxSetting right)
        {
            return !(left == right);
        }

        public override readonly int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
    [TypeConverter(typeof(BaseBoxSettingCollectionConverter))]
    public class BaseBoxSettingCollection : IEnumerable<BaseBoxSetting>
    {

        public BaseBoxSetting[] Array { get; private set; } = [];

        public IEnumerator<BaseBoxSetting> GetEnumerator() => ((IEnumerable<BaseBoxSetting>)Array).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Array.GetEnumerator();



        public static implicit operator BaseBoxSettingCollection(BaseBoxSetting[] array)
        {
            return new() { Array = array };
        }

        public static implicit operator BaseBoxSetting[](BaseBoxSettingCollection collection)
        {
            return collection.Array;
        }
    }
}
