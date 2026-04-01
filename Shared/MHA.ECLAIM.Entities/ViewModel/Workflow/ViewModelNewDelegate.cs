using MHA.ECLAIM.Entities.ViewModel.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class ViewModelNewDelegate
    {
        public List<DropDownListItem> ProcessTemplateList { get; set; } = new();

        [Required(ErrorMessage = "Please select a process.")]
        public string ddlProcessTemplate { get; set; }

        public bool isAdmin { get; set; }
        public string curUserName { get; set; }
        public PeoplePickerUser txtFromUser { get; set; }
        //TODO: Can have multiple delegatees at a time
        [Required(ErrorMessage = "Please select a Delegatee (Delegation To).")]
        public PeoplePickerUser txtTouser { get; set; }
        [Required(ErrorMessage = "Please select Delegation Start Date.")]
        public DateTime dateFrom { get; set; } = DateTime.Now.Date;
        [Required(ErrorMessage = "Please select Delegation End Date.")]
        public DateTime dateTo { get; set; } = DateTime.Now.Date;

        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }
}
