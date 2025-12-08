using System;

namespace HuanyuFlowerShop.Exceptions
{
    /// <summary>
    /// 实体未找到异常
    /// 当尝试访问不存在的实体时抛出
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        /// <summary>
        /// 初始化新的实体未找到异常实例
        /// </summary>
        public EntityNotFoundException() { }

        /// <summary>
        /// 使用指定的错误消息初始化新的实体未找到异常实例
        /// </summary>
        /// <param name="message">描述错误的消息</param>
        public EntityNotFoundException(string message) : base(message) { }

        /// <summary>
        /// 使用指定的错误消息和对导致此异常的内部异常的引用初始化新的实体未找到异常实例
        /// </summary>
        /// <param name="message">描述错误的消息</param>
        /// <param name="innerException">导致当前异常的异常</param>
        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }
        
        /// <summary>
        /// 使用指定的实体类型和ID初始化新的实体未找到异常实例
        /// </summary>
        /// <param name="entityType">实体类型名称</param>
        /// <param name="id">实体ID</param>
        public EntityNotFoundException(string entityType, object id) : 
            base($"未找到类型为 '{entityType}' 且ID为 '{id}' 的实体") { }
    }
}