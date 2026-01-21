using AuditApi.models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

public class ActivityRepository : IActivityRepository
{
    private readonly IMongoCollection<Activity> _col;
    public ActivityRepository(IMongoDatabase db) => _col = db.GetCollection<Activity>("Activities");

    public async Task<Activity> AddActivity(Activity activity)
    {
        activity.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        await _col.InsertOneAsync(activity);
        return activity;
    }


    public async Task<IEnumerable<Activity>> GetTasksHistory()
    {
        var cursor = await _col.FindAsync(_ => true);
        return cursor.ToList();
    }

    public async Task<IEnumerable<Activity>> GetTaskHistory(string taskId)
    {
        var cursor = await _col.FindAsync(activity => activity.TaskId == Guid.Parse(taskId));
        return cursor.ToList();
    }

}