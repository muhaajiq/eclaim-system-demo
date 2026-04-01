using MHA.Framework.Core.SP;
using MHA.Framework.Core.Workflow.BL;
using MHA.Framework.Core.Workflow.BO;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using Microsoft.SharePoint.Client;
using System.Data;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class WorkflowHelper
    {
        #region Workflow Actioner
        public static Actioner ConstructActioner(User user)
        {
            Actioner actioner = new Actioner(user.LoginName, user.Title, user.Email);
            return actioner;
        }

        public static Actioner ConstructActioner(PeoplePickerUser PPUser)
        {
            if (PPUser.Email == null)
            {
                PPUser.Email = "";
            }
            Actioner actioner = new Actioner(PPUser.Login, PPUser.Name, PPUser.Email);
            return actioner;
        }

        public static List<Actioner> ContructActionerList(List<PeoplePickerUser> peoplePickerList)
        {
            List<Actioner> ListActioner = new List<Actioner>();
            foreach (PeoplePickerUser pplPickerUser in peoplePickerList)
            {
                Actioner actioner = new Actioner(pplPickerUser.Login, pplPickerUser.Name, pplPickerUser.Email);
                ListActioner.Add(actioner);
            }
            return ListActioner;
        }

        public static void AddStageActionerFromPeoplePickerList(ref StageActioners stageActioners, List<PeoplePickerUser> userList)
        {
            foreach (PeoplePickerUser stageUser in userList)
            {
                Actioner actioner = ConstructActioner(stageUser);
                stageActioners.AddActioner(actioner);
            }
        }

        public static void AddStage(ref StageActioners stageActioners, string stageName, int dueDays, List<PeoplePickerUser> users)
        {
            if (users != null && users.Count > 0)
            {
                stageActioners.NextStage(stageName, dueDays);
                WorkflowHelper.AddStageActionerFromPeoplePickerList(ref stageActioners, users);
            }
        }
        #endregion

        #region Workflow Status
        public static bool IsWorkflowRunning(string refNo)
        {
            // 1. use wfbl.GetWorkflowInstanceByReferenceKey
            String WFConnString = ConnectionStringHelper.GetGenericWFConnString();
            MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(WFConnString);

            DataTable dtWFInstance = wfBL.GetWorkflowInstancesByReferenceKey(refNo);

            // 2. then wfbl.GetPendingTasksByProcessID
            if (dtWFInstance.Rows.Count > 0)
            {
                foreach (DataRow dr in dtWFInstance.Rows)
                {
                    int processID = Convert.ToInt32(dr[ConstantHelper.WFSQLTableFields.d_tblProcess.ProcessID]);
                    DataTable dtPendingTaskForCurrentMDR = wfBL.GetPendingTasksByProcessID(processID);
                    if (dtPendingTaskForCurrentMDR.Rows.Count > 0)
                    {
                        return true;
                    }
                }

            }

            return false;
        }

        public static bool IsWorkflowRunning(int processID)
        {
            string ConnString = ConnectionStringHelper.GetGenericWFConnString();
            WorkflowBL wfBL = new WorkflowBL(ConnString);
            string curStep = wfBL.GetCurrentStepName(processID);
            //TODO
            if (curStep.Equals(ConstantHelper.WorkflowStatus.APPROVED) || curStep.Equals(ConstantHelper.WorkflowStatus.COMPLETED) || curStep.Equals(ConstantHelper.WorkflowStatus.REJECTED) || curStep.Equals(ConstantHelper.WorkflowStatus.TERMINATED))
                return false;
            else
                return true;
        }
        #endregion

        #region General Helper
        public static PeoplePickerUser GetTaskOwnerByTaskID(int taskID, WorkflowBL wfBL)
        {
            PeoplePickerUser user = new PeoplePickerUser();
            TaskInstance taskInstance = wfBL.GetTask(taskID);
            user.Login = taskInstance.Assignee.LoginName;
            user.Name = taskInstance.Assignee.Name;
            return user;
        }

        public static void SetFormState(MainClaimHeaderVM vm)
        {
            bool showApproveButton = false;
            bool showRejectButton = false;
            bool showCompleteButton = false;
            bool showRequireAmendment = false;
            bool showResubmitButton = false;

            //switch (vm.WorkflowStage)
            //{
            //    case ConstantHelper.WorkflowStatus.PendingReportingLineVerifier:
            //        showCompleteButton = true;
            //        showRejectButton = true;
            //        showRequireAmendment = true;
            //        break;
            //    case ConstantHelper.WorkflowStatus.PendingReportingLineApprover:
            //        showApproveButton = true;
            //        showRejectButton = true;
            //        showRequireAmendment = true;
            //        break;
            //    case ConstantHelper.WorkflowStatus.PendingOriginatorResubmission:
            //        showResubmitButton = true;
            //        break;
            //    default:
            //        break;
            //}

            //vm.ShowApproveButton = showApproveButton;
            //vm.ShowRejectButton = showRejectButton;
            //vm.ShowCompleteButton = showCompleteButton;
            //vm.ShowRequireAmendment = showRequireAmendment;
            //vm.ShowResubmitButton = showResubmitButton;
        }
        #endregion
    }
}
