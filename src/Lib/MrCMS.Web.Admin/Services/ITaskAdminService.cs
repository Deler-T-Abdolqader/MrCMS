using System.Collections.Generic;
using MrCMS.Tasks;
using MrCMS.Web.Admin.Models;
using X.PagedList;

namespace MrCMS.Web.Admin.Services
{
    public interface ITaskAdminService
    {
        List<TaskInfo> GetAllScheduledTasks();
        TaskUpdateData GetTaskUpdateData(string type);
        IPagedList<QueuedTask> GetQueuedTasks(QueuedTaskSearchQuery searchQuery);
        void Update(TaskUpdateData info);
        void Reset(TaskUpdateData info);
    }
}