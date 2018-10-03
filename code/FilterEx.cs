using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Data;

namespace TK_AlarmManagement
{
	public class FilterEx
	{
		public FilterEx()
		{
		}

		public FilterEx(string name)
		{
			Name = name;
		}

		#region 过滤表达式
		public bool isStringMatched(string sample, string token)
		{
            sample = sample.ToLower();
            token = token.ToLower();

			if (token == "")
				return true;

			if (sample == "")
				return false;

			token = token.Trim();

			bool startwith_wildcard = false;
			bool endwith_wildcard = false;
			int start = 0;
			int len = token.Length;
			if (token[0] == '*' || token[0] == '%')
			{
				startwith_wildcard = true;
				start = 1;
				--len;
			}

			if (token[token.Length - 1] == '*' || token[token.Length - 1] == '%')
			{
				endwith_wildcard = true;
				--len;
			}

			if (startwith_wildcard && endwith_wildcard)
				return (sample.IndexOf(token.Substring(start, len)) != -1);
			else if (startwith_wildcard)
				return sample.EndsWith(token.Substring(start, len));
			else if (endwith_wildcard)
				return sample.StartsWith(token.Substring(start, len));
			else
				return sample == token;

		}

		public bool isMatched(ref TKAlarm alarm)
		{
            bool bMatched = false;
			if (BSCes.Count > 0)
			{
				foreach (object o in BSCes)
				{
					bMatched |= isStringMatched(alarm.NeName, (string)o);
					if (bMatched)
						break;
				}
				if (!bMatched)
					return false;
			}

			bMatched = false;
			if (Redefinition.Count != 0)
			{
				foreach (object o in Redefinition)
				{
					bMatched |= isStringMatched(alarm.Redefinition, (string)o);
					if (bMatched)
						break;
				}
				if (!bMatched)
					return false;
			}

			bMatched = false;
			try
			{
				DateTime t = Convert.ToDateTime(alarm.OccurTime);
				bMatched = (t >= StartTime && t <= EndTime);

				if (!bMatched)
					return false;
			}
			catch
			{
				return false;
			}

			alarm.ProjectEndTime = this.EndTime.ToString();
			alarm.ProjectInfo = this.AddInfo;

			return true;
		}

		#endregion

		private ArrayList m_Cities = new ArrayList();
		private ArrayList m_Manufacturers = new ArrayList();
		private ArrayList m_BSCes = new ArrayList();
		private ArrayList m_BTSes = new ArrayList();
		private ArrayList m_ObjTypes = new ArrayList();
		private ArrayList m_Redefinition = new ArrayList();
		private ArrayList m_Categories = new ArrayList();
		private ArrayList m_Severities = new ArrayList();
		private ArrayList m_Operators = new ArrayList();
		private DateTime m_StartTime, m_EndTime;
		private bool m_TimeFilterChecked;
		private bool m_DBMode = false;
		private string m_AddInfo = "";

		private string m_Name = "";
		private string m_GrpName = "";

		#region 各种过滤条件的外部接口
		public ArrayList Cities
		{
			get
			{
				return m_Cities;
			}
			set
			{
				if (value != null)
					m_Cities = (ArrayList)value.Clone();
			}
		}

		public ArrayList Manufacturers
		{
			get
			{
				return m_Manufacturers;
			}
			set
			{
				if (value != null)
					this.m_Manufacturers = (ArrayList)value.Clone();
			}
		}

		public ArrayList BSCes
		{
			get
			{
				return m_BSCes;
			}
			set
			{
				if (value != null)
					m_BSCes = (ArrayList)value.Clone();
			}
		}
		
		public ArrayList BTSes
		{
			get
			{
				return m_BTSes;
			}
			set
			{
				if (value != null)
					m_BTSes = (ArrayList)value.Clone();
			}
		}

		public ArrayList ObjTypes
		{
			get
			{
				return m_ObjTypes;
			}
			set
			{
				if (value != null)
					m_ObjTypes = (ArrayList)value.Clone();
			}
		}

		public ArrayList Redefinition
		{
			get
			{
				return this.m_Redefinition;
			}
			set
			{
				if (value != null)
					this.m_Redefinition = (ArrayList)value.Clone();
			}
		}

		public ArrayList Categories
		{
			get
			{
				return this.m_Categories;
			}
			set
			{
				if (value != null)
					this.m_Categories = (ArrayList)value.Clone();
			}
		}

		public ArrayList Severities
		{
			get
			{
				return m_Severities;
			}
			set
			{
				if (value != null)
					this.m_Severities = (ArrayList)value.Clone();
			}
		}

		public ArrayList Operators
		{
			get
			{
				return m_Operators;
			}
			set
			{
				if (value != null)
					m_Operators = (ArrayList)value.Clone();
			}
		}
		
		public bool TimeFilterEnabled
		{
			get
			{
				return this.m_TimeFilterChecked;
			}
			set
			{
				this.m_TimeFilterChecked = value;
			}
		}

		public DateTime StartTime
		{
			get
			{
				return m_StartTime;
			}
			set
			{
				m_StartTime = value;
			}
		}

		public DateTime EndTime
		{
			get
			{
				return m_EndTime;
			}
			set
			{
				m_EndTime = value;
			}
		}

		public bool DBMode
		{
			get
			{
				return m_DBMode;
			}
			set
			{
				this.m_DBMode = value;
			}
		}
		
		public string AddInfo
		{
			get
			{
				return m_AddInfo;
			}
			set
			{
				m_AddInfo = value;
			}
		}

		#endregion


		public string GroupName
		{
			get
			{
				return m_GrpName;
			}
			set
			{
				m_GrpName = value;
			}
		}

		public string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				m_Name = value;
			}
		}


	}

	public class FilterGroupEx
	{
		private SortedList m_FilterGroup = new SortedList();

		private bool m_DBMode = false;
		private string m_Name = "";

		/// <summary>
		/// 清空过滤器, 会生成一个新的自定义过滤器
		/// </summary>
		public void clearFilters()
		{
			m_FilterGroup.Clear();
		}

		public void addFilter(FilterEx filter)
		{
			lock(m_FilterGroup.SyncRoot)
			{
				this.m_FilterGroup.Add(filter.Name, filter);
			}
		}

		public FilterEx findFilter(string key)
		{
			if (this.m_FilterGroup.ContainsKey(key))
				return (FilterEx)this.m_FilterGroup[key];
			else
				return null;
		}

		// 改变自定义过滤器中的过滤器
		public void changeFilter(FilterEx filter)
		{
			if (this.m_FilterGroup.Contains(filter.Name))
			{
				lock (m_FilterGroup.SyncRoot)
				{
					this.m_FilterGroup.Remove(filter.Name);
					this.m_FilterGroup.Add(filter.Name, filter);			
				}			
			}
		}

		public void deleteFilter(string key)
		{
			if (this.m_FilterGroup.Contains(key))
			{
				lock(m_FilterGroup.SyncRoot)
				{
					this.m_FilterGroup.Remove(key);
				}
			}
		}

		public SortedList AllFilters
		{
			get
			{
				return this.m_FilterGroup;
			}
		}

		private FilterEx m_filterAllSee = new FilterEx();

		public FilterEx filterAllSee
		{
			get
			{
				return m_filterAllSee;
			}
		}

		public bool isMatched(ref TKAlarm alarm)
		{
			lock(AllFilters.SyncRoot)
			{
				if (AllFilters.Count == 0)
					return true;

				if (AllFilters.Count == 1)
				{
					FilterEx f = (FilterEx)AllFilters[AllFilters.GetKey(0)];
					m_filterAllSee = f;
					return f.isMatched(ref alarm);
				}

				bool bMatched = false;
				foreach (DictionaryEntry de in AllFilters)
				{
					bMatched = ((FilterEx)AllFilters[de.Key]).isMatched(ref alarm);
					if (bMatched)
						return true;
				}	
			}

			return false; // 不能直接返回false，可能未做任何比较
		}

		public string Name
		{
			get
			{
				return m_Name;
			}

			set
			{
				m_Name = value;
				foreach (DictionaryEntry de in this.AllFilters)
				{
					((FilterEx)de.Value).GroupName = value;
				}
			}
		}

		public bool DBMode
		{
			get
			{
				return this.m_DBMode;
			}
			set
			{
				this.m_DBMode = value;
				foreach (DictionaryEntry de in AllFilters)
				{
					FilterEx f = (FilterEx)de.Value;
					f.DBMode = value;
				}	
			}
		}

		public FilterGroupEx()
		{
		}

		public void loadFilters(DataTable dt)
		{
			foreach(DataRow dr in dt.Rows)
			{
				FilterEx filter = new FilterEx(dr["id"].ToString());
				filter.BSCes = Utilities.ConvertString2StrArr(dr["ne_name"].ToString());
				filter.Redefinition =Utilities. ConvertString2StrArr(dr["redefinition"].ToString());
                filter.Categories = Utilities.ConvertString2StrArr("");
				filter.StartTime = DateTime.Parse(dr["start_time"].ToString());
				filter.EndTime = DateTime.Parse(dr["end_time"].ToString());
				filter.AddInfo = dr["report_msg"].ToString();

				addFilter(filter);
			}
		}
	}
}
