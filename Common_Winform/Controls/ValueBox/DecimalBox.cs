using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Controls.ValueBox
{
    [DefaultEvent(nameof(ValueChanged))]
    public class DecimalBox : TextBox
    {
        #region 属性
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("默认值")]
        public decimal DefaultValue
        {
            get => defaultValue;
            set
            {
                defaultValue = IntoRange(value);
                Text = defaultValue.ToString();
                _value = defaultValue;
            }
        }
        private decimal defaultValue = 0;

        /// <summary>
        /// 小数精度
        /// </summary>
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("小数精度")]
        public int Precision
        {
            get => precision;
            set
            {
                precision = value;
                TrySetValue(Value);
            }
        }
        private int precision = -1;


        /// <summary>
        /// 数值范围,设置改值将会重新设置默认值
        /// </summary>
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("数值范围")]
        public NumberRangeEnum Range
        {
            get
            {
                return range;
            }
            set
            {
                range = value;
                DefaultValue = defaultValue;
            }
        }
        private NumberRangeEnum range = NumberRangeEnum.Arbitrarily;
        #endregion

        #region 数据
        [Category("CVII_自定义_数据"), Description("当前值")]
        public decimal Value
        {
            get => _value;
            set
            {
                TrySetValue(value);
            }
        }
        private decimal _value;
        #endregion

        public DecimalBox()
        {
            Text = defaultValue.ToString();
            _value = defaultValue;
            Leave += DecimalBox_Leave;
        }

        #region 控制
        /// <summary>
        /// 清空输入
        /// </summary>
        public void ClearInput()
        {
            TrySetValue(defaultValue);
        }
        /// <summary>
        /// 尝试设置值
        /// </summary>
        /// <param name="newValue"></param>
        public void TrySetValue(decimal newValue)
        {
            newValue = IntoRange(newValue);
            // 精度限制
            if (precision >= 0)
            {
                newValue = (decimal)Math.Round(newValue, precision);
            }

            decimal oldValue = Value;
            Text = newValue.ToString();
            _value = newValue;
            if (oldValue != newValue)
            {
                ValueChanged?.Invoke(this, oldValue, newValue);
            }
        }
        #endregion

        private void DecimalBox_Leave(object? sender, EventArgs e)
        {
            Leave -= DecimalBox_Leave;
            decimal newValue = ToValue(Text);
            TrySetValue(newValue);
            Leave += DecimalBox_Leave;
        }

        #region 事件
        /// <summary>
        /// 值变化委托
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public delegate void ValueChangedDelegate(DecimalBox inputBox, decimal oldValue, decimal newValue);
        /// <summary>
        /// 值变化事件
        /// </summary>
        [Category("CVII_自定义"), DisplayName("值变更事件")]
        public event ValueChangedDelegate? ValueChanged;
        #endregion

        #region 重载
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.KeyCode == Keys.Enter)
            {
                decimal newValue = ToValue(Text);
                TrySetValue(newValue);
            }
        }
        #endregion

        /// <summary>
        /// 将值转换为decimal
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private decimal ToValue(string str)
        {
            if (decimal.TryParse(str, out decimal output))
            {
                return output;
            }
            else
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// 将值转换到可用区间内
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private decimal IntoRange(decimal value)
        {
            decimal output = value;
            switch (range)
            {
                case NumberRangeEnum.Negative:
                    if (output >= 0)
                    {
                        output = -1;
                    }
                    break;
                case NumberRangeEnum.Nonnegative:
                    if (output < 0)
                    {
                        output = 0;
                    }
                    break;
                case NumberRangeEnum.Positive:
                    if (output <= 0)
                    {
                        output = 1;
                    }
                    break;
                case NumberRangeEnum.Arbitrarily:
                    break;
            }
            return output;
        }
    }
}
