using System;
using System.Collections.Generic;
using System.Text;

namespace WatchDog
{
    public enum DogStatus
    {
        GOOD,
        BAD,
        STILL_BAD
    }

    public interface IWatchDogFactory
    {
        void Init(string dbconn_str, string watchdog_tablename);

        /// <summary>
        /// 由工厂创建WatchDog
        /// </summary>
        /// <param name="dbconn_str"></param>
        /// <param name="host">WatchDog伺服的主部件名称</param>
        /// <param name="component">WatchDog伺服的子部件名称。host+component进程内全局唯一</param>
        /// <param name="refresh_interval">毫秒为单位的时间间隔，表示Host将多长时间更新一次WatchDog时间戳</param>
        /// <param name="timeout">周期数。如果超出周期数*refresh_interval的时间未喂狗，则狗状态不正常</param>
        /// <returns></returns>
        IWatchDog CreateWatchDog(string host, string component, int refresh_interval, int timeout);

        int CreateWatchDogsFromFile(string filename);
        DogStatus Check(string host);
        void Update(string host);

        IWatchDog GetWatchDog(string host, string component);
    }

    public interface IWatchDog : IComparable<IWatchDog>
    {
        /// <summary>
        /// 喂狗
        /// </summary>
        void Feed();

        /// <summary>
        /// 检查狗状态
        /// </summary>
        /// <returns>true:狗正常</returns>
        DogStatus Check();

        DogStatus Status
        {
            get;
        }

        string Host
        {
            get;
        }

        string Component
        {
            get;
        }
    }
}
