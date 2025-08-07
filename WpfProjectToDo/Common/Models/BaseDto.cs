using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfProjectToDo.Common.Models
{
    //DataTransferObject简称DTO，数据传输对象
    public class BaseDto
    {
		private int id;

		public int Id
		{
			get { return id; }
			set { id = value; }
		}

		private DateTime creatDate;

		public DateTime CreatDate
        {
			get { return creatDate; }
			set { creatDate = value; }
		}

		private DateTime updateDate;

		public DateTime UpdateDate
        {
			get { return updateDate; }
			set { updateDate = value; }
		}

	}
}
