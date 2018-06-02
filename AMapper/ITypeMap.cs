using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMapper
{
    /// <summary>
    /// 类型映射接口
    /// </summary>
    public interface ITypeMap
    {
        /// <summary>
        /// 包含单个类型转换的转换类
        /// </summary>
        Type ConvertType { get; }
        /// <summary>
        /// 包含多个类型转换的转换类
        /// </summary>
        Type CollectConvertType
        {
            get;
        }
    }
}
