using System;
using System.Collections.Generic;

abstract public class EventManager<D>
{
	public delegate void EventCallback();

	public delegate void EventCallback<Arg1Type>(Arg1Type arg1);

	public delegate void EventCallback<Arg1Type,Arg2Type>(Arg1Type arg1,Arg2Type arg2);

	public delegate void EventCallback<Arg1Type,Arg2Type,Arg3Type>(Arg1Type arg1,Arg2Type arg2,Arg3Type arg3);

	static private Dictionary<int,Delegate> event_list_ = new Dictionary<int, Delegate>();
	//添加时检查委托的类型，如果事件列表里已有该事件，且事件已有处理函数，则抛出异常返回false.
	static private bool AddCheck(int eventID, Delegate callback)
	{
		if (!event_list_.ContainsKey(eventID))
		{
			event_list_.Add(eventID, null);
		}
		Delegate d = event_list_ [eventID];
		if (d != null && d.GetType() != callback.GetType())
		{
//			throw new Exception(string.Format("Event \"{0}\" has type \"{1}\". But handle being added has type \"{2}\".", eventID, d.GetType().Name, callback.GetType().Name));
			return false;
		}
		return true;

	}

	/// <summary>
	/// 添加无参数事件处理方法.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool AddEventCallback(int eventID, EventCallback callback)
	{
		if (AddCheck(eventID, callback))
		{
			event_list_ [eventID] = (EventCallback)event_list_ [eventID] + callback;
			return true;
		}
		return false;
	}
	/// <summary>
	/// 添加一参数事件处理方法.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool AddEventCallback<Arg1Type>(int eventID, EventCallback<Arg1Type> callback)
	{
		if (AddCheck(eventID, callback))
		{
			event_list_ [eventID] = (EventCallback<Arg1Type>)event_list_ [eventID] + callback;
			return true;
		}
		return false;
	}
	/// <summary>
	/// 添加二参数事件处理方法.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool AddEventCallback<Arg1Type,Arg2Type>(int eventID, EventCallback<Arg1Type,Arg2Type> callback)
	{
		if (AddCheck(eventID, callback))
		{
			event_list_ [eventID] = (EventCallback<Arg1Type,Arg2Type>)event_list_ [eventID] + callback;
			return true;
		}
		return false;
	}
	/// <summary>
	/// 添加三参数事件处理方法.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool AddEventCallback<Arg1Type,Arg2Type,Arg3Type>(int eventID, EventCallback<Arg1Type,Arg2Type,Arg3Type> callback)
	{
		if (AddCheck(eventID, callback))
		{
			event_list_ [eventID] = (EventCallback<Arg1Type,Arg2Type,Arg3Type>)event_list_ [eventID] + callback;
			return true;
		}
		return false;
	}
	//移除检查，如果事件列表中没有要移除的事件类型抛出异常，要移除的事件没有处理函数抛出异常，处理函数与事件类型不符抛出异常.
	static protected bool RemoveCheck(int eventID, Delegate callback)
	{
		if (event_list_.ContainsKey(eventID))
		{
			Delegate d = event_list_ [eventID];
			if (d == null)
			{
//				throw new Exception(string.Format("Event \"{0}\" is no handle.",eventID));
				return false;
			}
			if (d.GetType() != callback.GetType())
			{
//				throw new Exception(string.Format("Event \"{0}\" has type \"{1}\". But handle being removed has type \"{2}\".",eventID,d.GetType().Name,callback.GetType().Name));
				return false;
			}
			return true;
		} else
		{
//			throw new Exception(string.Format("There is no Event name \"{0}\".",eventID));
			return false;
		}
	}
	//移除没有处理方法的事件
	static private void RemoveNullEvent(int eventID)
	{
		if (event_list_ [eventID] == null)
		{
			event_list_.Remove(eventID);
		}
	}
	/// <summary>
	/// 移除无参数事件处理方法.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool RemoveEvent(int eventID, EventCallback callback)
	{
		if (RemoveCheck(eventID, callback))
		{
			event_list_ [eventID] = (EventCallback)event_list_ [eventID] - callback;
			RemoveNullEvent(eventID);
			return true;
		}
		return false;
	}
	/// <summary>
	/// 移除一参数事件处理方法.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool RemoveEvent<Arg1Type>(int eventID, EventCallback<Arg1Type> callback)
	{
		if (RemoveCheck(eventID, callback))
		{
			event_list_ [eventID] = (EventCallback<Arg1Type>)event_list_ [eventID] - callback;
			RemoveNullEvent(eventID);
			return true;
		}
		return false;
	}
	/// <summary>
	/// 移除二参数事件处理方法.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool RemoveEvent<Arg1Type,Arg2Type>(int eventID, EventCallback<Arg1Type,Arg2Type> callback)
	{
		if (RemoveCheck(eventID, callback))
		{
			event_list_ [eventID] = (EventCallback<Arg1Type,Arg2Type>)event_list_ [eventID] - callback;
			RemoveNullEvent(eventID);
			return true;
		}
		return false;
	}
	/// <summary>
	/// 移除三参数事件处理方法.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool RemoveEvent<Arg1Type,Arg2Type,Arg3Type>(int eventID, EventCallback<Arg1Type,Arg2Type,Arg3Type> callback)
	{
		if (RemoveCheck(eventID, callback))
		{
			event_list_ [eventID] = (EventCallback<Arg1Type,Arg2Type,Arg3Type>)event_list_ [eventID] - callback;
			RemoveNullEvent(eventID);
			return true;
		}
		return false;
	}
	/// <summary>
	/// 移除事件.
	/// </summary>
	/// <param name="eventID">事件ID.</param>
	static protected bool ClearEventCallback(int eventID)
	{
		return event_list_.Remove(eventID);
	}
	//触发检查.
	static private bool TriggerCheck(int eventID)
	{
		if (!event_list_.ContainsKey(eventID))
		{
//			throw new Exception(string.Format("There is no Event name \"{0}\".", eventID));
			return false;
		} else
			return true;
	}
	/// <summary>
	/// 触发无参数事件.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool TriggerEvent(int eventID)
	{
		if (TriggerCheck(eventID))
		{
			((EventCallback)event_list_ [eventID])();
			return true;
		}
		return false;
	}
	/// <summary>
	/// 触发一参数事件.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool TriggerEvent<Arg1Type>(int eventID, Arg1Type arg1)
	{
		if (TriggerCheck(eventID))
		{
			((EventCallback<Arg1Type>)(event_list_ [eventID]))(arg1);
			return true;
		}
		return false;
	}
	/// <summary>
	/// 触发二参数事件.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool TriggerEvent<Arg1Type,Arg2Type>(int eventID, Arg1Type arg1, Arg2Type arg2)
	{
		if (TriggerCheck(eventID))
		{
			((EventCallback<Arg1Type,Arg2Type>)event_list_ [eventID])(arg1, arg2);
			return true;
		}
		return false;
	}
	/// <summary>
	/// 触发三参数事件.
	/// </summary>
	/// <param name="eventID">事件名称.</param>
	/// <param name="handler">处理方法.</param>
	static protected bool TriggerEvent<Arg1Type,Arg2Type,Arg3Type>(int eventID, Arg1Type arg1, Arg2Type arg2, Arg3Type arg3)
	{
		if (TriggerCheck(eventID))
		{
			((EventCallback<Arg1Type,Arg2Type,Arg3Type>)event_list_ [eventID])(arg1, arg2, arg3);
			return true;
		}
		return false;
	}
}
