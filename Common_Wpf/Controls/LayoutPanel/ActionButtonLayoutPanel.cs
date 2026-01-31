using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Common_Wpf.Controls.LayoutPanel
{
    /// <summary>
    /// 底部有一个按钮列表的布局面板
    /// </summary>
    public class ActionButtonLayoutPanel : MbContentControl01
    {
        static ActionButtonLayoutPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ActionButtonLayoutPanel),
                new FrameworkPropertyMetadata(typeof(ActionButtonLayoutPanel)));
        }


        public Guid Id { get; } = Guid.NewGuid();
        private ObservableCollection<ActionButtonOption> _internalButtons = [];

        public ActionButtonLayoutPanel()
        {
            RawButtons = [];

            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetBinding(Button.ContentProperty, new Binding("Label"));
            factory.SetBinding(Button.CommandProperty, new Binding("Command"));
            factory.SetBinding(Button.CommandParameterProperty, new Binding("CommandParameter"));
            factory.SetBinding(Button.IsDefaultProperty, new Binding("IsDefault"));
            factory.SetBinding(Button.IsCancelProperty, new Binding("IsCancel"));
            factory.SetValue(Button.MinWidthProperty, 64.0);
            factory.SetValue(Button.HeightProperty, double.NaN);
            factory.SetValue(Button.MarginProperty, new Thickness(4, 0, 0, 0));
            factory.SetValue(Button.PaddingProperty, new Thickness(8, 4, 8, 4));


            ControlButtonTemplate = new DataTemplate { VisualTree = factory };
            Converter = ActionButtonOptionConverter.Shared;
        }


        /// <summary>
        /// 按钮配置转换器
        /// </summary>
        public ActionButtonOptionConverter Converter
        {
            get => (ActionButtonOptionConverter)GetValue(ConverterProperty);
            set => SetValue(ConverterProperty, value);
        }
        public static readonly DependencyProperty ConverterProperty =
            DependencyProperty.Register(
                nameof(Converter),
                typeof(ActionButtonOptionConverter),
                typeof(ActionButtonLayoutPanel),
                new PropertyMetadata(null, OnConverterChanged));
        private ActionButtonOption ConvertToButtonOption(object source)
        {
            var converter = this.Converter ?? ActionButtonOptionConverter.Shared;
            return converter.Convert(source);
        }
        private static void OnConverterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ActionButtonLayoutPanel panel) return;

            if (e.OldValue is ActionButtonOptionConverter converter)
            {
                converter.OnRegisterChanged -= panel.ConverterRegisterChanged;
            }
            if (e.NewValue is ActionButtonOptionConverter newConverter)
            {
                newConverter.OnRegisterChanged += panel.ConverterRegisterChanged;
            }
            panel.UpdateActionButtons();
        }
        private void ConverterRegisterChanged(object? sender, Type changedType)
        {
            UpdateActionButtons();
        }
        private void UpdateActionButtons()
        {
            _internalButtons.Clear();
            foreach (var item in RawButtons)
            {
                _internalButtons.Add(ConvertToButtonOption(item));
            }
        }

        /// <summary>
        /// 按钮列表, 使用 <see cref="Converter"/> 转换为 <see cref="ActionButtonOption"/>
        /// </summary>
        public ObservableCollection<object> RawButtons
        {
            get => (ObservableCollection<object>)GetValue(RawButtonsProperty);
            set => SetValue(RawButtonsProperty, value);
        }
        public static readonly DependencyProperty RawButtonsProperty =
            DependencyProperty.Register(nameof(RawButtons), typeof(ObservableCollection<object>), typeof(ActionButtonLayoutPanel), new PropertyMetadata(OnRawButtonsChanged));
        private static void OnRawButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ActionButtonLayoutPanel panel) return;

            if (e.OldValue is INotifyCollectionChanged oldCollection) oldCollection.CollectionChanged -= panel.OnSourceCollectionChanged;

            panel._internalButtons.Clear();

            if (e.NewValue is IList newCollection)
            {
                foreach (var item in newCollection)
                {
                    panel._internalButtons.Add(panel.ConvertToButtonOption(item));
                }
                if (newCollection is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += panel.OnSourceCollectionChanged;
                }
            }
        }
        private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _internalButtons.Clear();
                return;
            }
            if (e.NewItems != null)
            {
                int index = e.NewStartingIndex;
                foreach (var item in e.NewItems)
                {
                    var btn = ConvertToButtonOption(item);
                    if (index >= 0 && index < _internalButtons.Count)
                        _internalButtons.Insert(index, btn);
                    else
                        _internalButtons.Add(btn);
                    index++;
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (_internalButtons.Count > e.OldStartingIndex)
                        _internalButtons.RemoveAt(e.OldStartingIndex);
                }
            }
        }
        /// <summary>
        /// 实际应显示的按钮配置集合
        /// </summary>
        public ObservableCollection<ActionButtonOption> DisplayButtons => _internalButtons;


        /// <summary>
        /// 按钮的外观模板
        /// </summary>
        public DataTemplate ControlButtonTemplate
        {
            get => (DataTemplate)GetValue(ControlButtonTemplateProperty);
            set => SetValue(ControlButtonTemplateProperty, value);
        }
        public static readonly DependencyProperty ControlButtonTemplateProperty =
            DependencyProperty.Register(nameof(ControlButtonTemplate), typeof(DataTemplate), typeof(ActionButtonLayoutPanel));


        /// <summary>
        /// 按钮排列方式
        /// </summary>
        public HorizontalAlignment ButtonAlignment
        {
            get { return (HorizontalAlignment)GetValue(ButtonAlignmentProperty); }
            set { SetValue(ButtonAlignmentProperty, value); }
        }

        public static readonly DependencyProperty ButtonAlignmentProperty =
            DependencyProperty.Register(nameof(ButtonAlignment), typeof(HorizontalAlignment), typeof(ActionButtonLayoutPanel), new PropertyMetadata(HorizontalAlignment.Right));



        /// <summary>
        /// 按钮区域内的 Padding
        /// </summary>
        public Thickness ButtonAreaPadding
        {
            get { return (Thickness)GetValue(ButtonAreaPaddingProperty); }
            set { SetValue(ButtonAreaPaddingProperty, value); }
        }
        public static readonly DependencyProperty ButtonAreaPaddingProperty =
            DependencyProperty.Register(nameof(ButtonAreaPadding), typeof(Thickness), typeof(ActionButtonLayoutPanel), new PropertyMetadata(new Thickness(2)));




    }

    public class ActionButtonOption : DependencyObject
    {
        /// <summary>
        /// 显示文本
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(ActionButtonOption));

        /// <summary>
        /// 绑定的命令
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ActionButtonOption));

        /// <summary>
        /// 命令参数
        /// </summary>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ActionButtonOption));

        /// <summary>
        /// 是否为默认按钮 (响应回车)
        /// </summary>
        public bool IsDefault
        {
            get => (bool)GetValue(IsDefaultProperty);
            set => SetValue(IsDefaultProperty, value);
        }
        public static readonly DependencyProperty IsDefaultProperty =
            DependencyProperty.Register(nameof(IsDefault), typeof(bool), typeof(ActionButtonOption), new PropertyMetadata(false));

        /// <summary>
        /// 是否为取消按钮 (响应 Esc)
        /// </summary>
        public bool IsCancel
        {
            get => (bool)GetValue(IsCancelProperty);
            set => SetValue(IsCancelProperty, value);
        }
        public static readonly DependencyProperty IsCancelProperty =
            DependencyProperty.Register(nameof(IsCancel), typeof(bool), typeof(ActionButtonOption), new PropertyMetadata(false));

        /// <summary>
        /// 绑定原始对象
        /// </summary>
        public object? Source { get; init; }
        /// <summary>
        /// 是否源于未注册到转换器 <see cref="ActionButtonLayoutPanel.Converter"/> 的占位实例
        /// </summary>
        public bool IsUnregistered { get; init; }
    }

    public class ActionButtonOptionConverter
    {
        private static readonly ActionButtonOptionConverter _shared = new ActionButtonOptionConverter();
        public static ActionButtonOptionConverter Shared => _shared;

        private readonly Dictionary<Type, Func<object, ActionButtonOption>> _converters = [];

        public ActionButtonOptionConverter() 
        {
            Register<ActionButtonOption>(btn => btn);
        }

        /// <summary>
        /// 注册事件 (新增注册项或注册内容发生变更时触发
        /// </summary>
        public event EventHandler<Type>? OnRegisterChanged;

        /// <summary>
        /// 注册转换逻辑
        /// </summary>
        public void Register<TSource>(Func<TSource, ActionButtonOption> converter)
        {
            ArgumentNullException.ThrowIfNull(converter);

            _converters[typeof(TSource)] = (obj) => converter((TSource)obj);

            if (OnRegisterChanged != null)
            {
                OnRegisterChanged?.Invoke(this, typeof(TSource));
            }
        }

        /// <summary>
        /// 执行转换
        /// </summary>
        [return: NotNullIfNotNull(nameof(obj))]
        public ActionButtonOption? Convert(object? obj)
        {
            if (obj == null) return null;

            if (obj is ActionButtonOption btn) return btn;

            Type sourceType = obj.GetType();

            if (_converters.TryGetValue(sourceType, out var converter))
            {
                return converter(obj);
            }

            return new()
            {
                Label = obj.ToString() ?? string.Empty,
                IsUnregistered = true,
                Source = obj,
            };
            // throw new InvalidOperationException($"类型 '{sourceType.FullName}' 未注册转换器。");
        }
    }
}
