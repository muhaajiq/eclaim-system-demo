using MHA.ECLAIM.Entities.ViewModel.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MHA.ECLAIM.Entities.ViewModel.Home
{
    [Serializable]
    [DataContract]
    public class ViewModelMyPendingTask
    {
        public List<MyPendingTask> MyTaskList { get; set; } = new();

        public bool HasTask { get; set; }

        #region Paging
        public int CurrentPage { get; set; }

        //public int PageGroupCount { get; set; }

        public int TotalPage { get; set; }

        public int TotalCount { get; set; }
        public int RowsPerPage { get; set; }

        #endregion

        //public ViewModelMyPendingTask()
        //{
        //    MyTaskList = new List<MyPendingTask>();
        //    HasTask = false;
        //}
    }
}
