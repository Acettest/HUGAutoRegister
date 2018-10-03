using System;

namespace TK_AlarmManagement
{
	/// <summary>
	/// ICommandHandler 的摘要说明。
	/// </summary>
    /// 

	public interface ICommandHandler
	{
        void handleCommand(ICommunicationMessage message);
	}
}
