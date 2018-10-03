using System;
namespace DefLib
{
    public interface IOperationAtom
    {
        /// <summary>
        /// 操作器的名称
        /// </summary>
        string OperationName { get;set; }

        /// <summary>
        /// 是否正在进行状态改变，如正在启动或正在停止，处于状态改变的操作原子的IsRunning状态应为改变前的状态
        /// </summary>
        bool IsPending { get; }

        /// <summary>
        /// 是否在运行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 启动
        /// </summary>
        void Start();

        /// <summary>
        /// 停止
        /// </summary>
        void Stop();
    }
}
