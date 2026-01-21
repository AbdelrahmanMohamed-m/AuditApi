using AuditApi.DTOs;
using AuditApi.mappers;
using Microsoft.AspNetCore.Mvc;

namespace AuditApi.Controllers;

[ApiController]
[Route("api/activities")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityRepository _repository;

    public ActivitiesController(IActivityRepository repository)
    {
        _repository = repository;
    }

    [HttpGet ("task/{taskId}")]
    public async Task<IEnumerable<ActivityDto>> GetTaskHistory(string taskId)
    {
        var activities = await _repository.GetTaskHistory(taskId);
        return activities.Select(ActvityMapper.ToDto);
    }

    [HttpGet]
    public async Task<IEnumerable<ActivityDto>> GetTasksHistory()
    {
        var activities = await _repository.GetTasksHistory();
        return activities.Select(ActvityMapper.ToDto);
    }

    [HttpPost]
    public async Task<IActionResult> AddActivity(ActivityDto dto)
    {
        var activity = ActvityMapper.ToModel(dto);
        activity.Timestamp = DateTime.UtcNow;
        await _repository.AddActivity(activity);
        return Ok();
    }

}
