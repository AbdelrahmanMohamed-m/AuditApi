using AuditApi.models;

public interface IActivityRepository
{
    Task<Activity> AddActivity(Activity a);
    Task<IEnumerable<Activity>> GetTaskHistory(string taskId);
    Task<IEnumerable<Activity>> GetTasksHistory();


}