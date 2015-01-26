using avt.ActionForm;
using avt.ActionForm.Core.Actions;
using avt.ActionForm.Core.DnnSf;
using avt.ActionForm.Core.Form;
using avt.ActionForm.Core.Form.Result;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Journal;
using System.Linq;

namespace ActionForm.DnnJournal
{
    public class PostToJournal : IAction
    {
        public string Message { get; set; }
        public string UserId { get; set; }


        public void Init(ActionInfo actionInfo, SettingsDictionary settings)
        {
            Message = settings.GetValue("Message", "");
            UserId = settings.GetValue("UserId", "");
        }

        public IFormEventResult Execute(ActionFormSettings settings, FormData data, eActionContext context)
        {
            var message = data.ApplyAllTokens(Message);
            if (string.IsNullOrEmpty(message))
                return null;

            var strUserId = data.ApplyAllTokens(UserId);
            var userId = UserController.GetCurrentUserInfo().UserID;
            if (!string.IsNullOrEmpty(strUserId))
                int.TryParse(strUserId, out userId);

            string objectKey = null;// Constants.ContentTypeName + "_" + Constants.JournalVoteTypeName + "_" + string.Format("{0}:{1}", objPost.ModuleID, voteId);
            var ji = JournalController.Instance.GetJournalItemByKey(settings.PortalId, objectKey);
            if ((ji != null)) 
                JournalController.Instance.DeleteJournalItemByKey(settings.PortalId, objectKey);
            

            var colJournalTypes = (from t in JournalController.Instance.GetJournalTypes(settings.PortalId)
                                   where t.JournalType == "status"
                                   select t.JournalTypeId).SingleOrDefault();

            ji = new JournalItem {
                PortalId = settings.PortalId,
                ProfileId = userId,
                UserId = userId,
                ContentItemId = -1,
                Title = "",
                ItemData = null, //new ItemData { Url = url },
                Summary = message,
                Body = null,
                JournalTypeId = colJournalTypes,
                ObjectKey = objectKey,
                SecuritySet = "E,"
            };

            JournalController.Instance.SaveJournalItem(ji, settings.TabId);

            return null;
        }

    }
}
