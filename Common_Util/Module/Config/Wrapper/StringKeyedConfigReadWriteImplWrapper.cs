using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config.Wrapper
{
    public readonly struct StringKeyedConfigReadWriteImplWrapper<TCompatibleKey> : IKeyedConfigReadWriteImpl<TCompatibleKey>
	{
        #region 静态
        /// <summary>
        /// 如果传入实例是 <see cref="StringKeyedConfigReadWriteImplWrapper"/> 包装器, 则解包装
        /// </summary>
        /// <param name="impl"></param>
        /// <returns></returns>
        public static IConfigReadWriteImpl UnWrapper(IConfigReadWriteImpl impl) => impl is StringKeyedConfigReadWriteImplWrapper<TCompatibleKey> wrapper ? wrapper.InEffect : impl;

        #endregion

        /// <summary>
        /// 当前生效中的配置读写实现
        /// </summary>
        public IConfigReadWriteImpl InEffect 
        { 
            get => useMode switch
            {
                ModeEnum.Compatible => compatibleKeyedImpl!,
                ModeEnum.String => stringKeyedImpl!,
                _ => throw new InvalidOperationException("未配置可用的配置读写实现")
            };
        }
        private readonly IKeyedConfigReadWriteImpl<TCompatibleKey>? compatibleKeyedImpl;
        private readonly Func<TCompatibleKey, string>? convert2StrFunc;
        private readonly Func<string, TCompatibleKey>? convert2CompatibleFunc;
        private readonly IKeyedConfigReadWriteImpl<string>? stringKeyedImpl;
        /// <summary>
        /// 模式: 0:未配置; 1:兼容模式; 2:字符串;
        /// </summary>
        private readonly ModeEnum useMode;
        private enum ModeEnum
        {
            Compatible, 
            String,
        }

        public StringKeyedConfigReadWriteImplWrapper(IKeyedConfigReadWriteImpl<TCompatibleKey> keyedImpl, Func<TCompatibleKey, string> convert2StrFunc, Func<string, TCompatibleKey> convert2CompatibleFunc)
        {
            if (keyedImpl is StringKeyedConfigReadWriteImplWrapper<TCompatibleKey>) throw new NotSupportedException("不支持包装器嵌套! ");
            ArgumentNullException.ThrowIfNull(convert2StrFunc);
            ArgumentNullException.ThrowIfNull(convert2CompatibleFunc);
            this.compatibleKeyedImpl = keyedImpl;
            this.convert2StrFunc = convert2StrFunc;
            this.convert2CompatibleFunc = convert2CompatibleFunc;
            stringKeyedImpl = null;
            useMode = ModeEnum.Compatible;

        } 
        public StringKeyedConfigReadWriteImplWrapper(IKeyedConfigReadWriteImpl<string> stringKeyedImpl)
		{
			this.stringKeyedImpl = stringKeyedImpl;
			compatibleKeyedImpl = null;
            convert2StrFunc = null;
            convert2CompatibleFunc = null;
            useMode = ModeEnum.String;
        }

        #region 接口转换
        /// <summary>
        /// 取得使用本包装器使用字符串作为键值的接口包装
        /// </summary>
        /// <returns></returns>
        public readonly IKeyedConfigReadWriteImpl<string> AsStringKey()
        {
            return new StringKeyedInterfaceWrapper(this);
        }
        private readonly struct StringKeyedInterfaceWrapper(StringKeyedConfigReadWriteImplWrapper<TCompatibleKey> outside) : IKeyedConfigReadWriteImpl<string>
        {
            private readonly StringKeyedConfigReadWriteImplWrapper<TCompatibleKey> outside = outside;

            public bool SaveConfig(string key, object config)
            {
                return outside.SaveConfig(key, config);
            }

            public bool TryLoadConfig(string key, [NotNullWhen(true)] out object? config)
            {
                return outside.TryLoadConfig(key, out config);
            }
        }
        #endregion

        #region 接口实现

        public bool TryLoadConfig(TCompatibleKey key, [NotNullWhen(true)] out object? config)
        {
            return useMode switch
            {
                ModeEnum.Compatible => compatibleKeyedImpl!.TryLoadConfig(key, out config),
                ModeEnum.String => stringKeyedImpl!.TryLoadConfig(convert2StrFunc!(key), out config),
                _ => throw InvalidMode()
            };
        }

        public bool SaveConfig(TCompatibleKey key, object config)
        {
            return useMode switch
            {
                ModeEnum.Compatible => compatibleKeyedImpl!.SaveConfig(key, config),
                ModeEnum.String => stringKeyedImpl!.SaveConfig(convert2StrFunc!(key), config),
                _ => throw InvalidMode()
            };
        }

        public bool TryLoadConfig(string key, [NotNullWhen(true)] out object? config)
        {
            return useMode switch
            {
                ModeEnum.Compatible => compatibleKeyedImpl!.TryLoadConfig(convert2CompatibleFunc!(key), out config),
                ModeEnum.String => stringKeyedImpl!.TryLoadConfig(key, out config),
                _ => throw InvalidMode()
            };
        }

        public bool SaveConfig(string key, object config)
        {
            return useMode switch
            {
                ModeEnum.Compatible => compatibleKeyedImpl!.SaveConfig(convert2CompatibleFunc!(key), config),
                ModeEnum.String => stringKeyedImpl!.SaveConfig(key, config),
                _ => throw InvalidMode()
            };
        }

        #endregion

        #region 异常
        private readonly Exception InvalidMode() => new InvalidOperationException("当前使用模式无效") { Data = { ["Mode"] = useMode } };
        #endregion
    }
}
