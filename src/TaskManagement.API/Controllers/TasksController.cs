using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.Features.Tasks.Commands;
using TaskManagement.Application.Features.Tasks.Queries;

namespace TaskManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : BaseApiController
    {
        private readonly IMediator _mediator;

        public TasksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TaskDto>>>> GetTasks()
        {
            var tasks = await _mediator.Send(new GetAllTasksQuery());
            return Success(tasks, "Tasks retrieved successfully");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TaskDto>>> GetTask(int id)
        {
            var task = await _mediator.Send(new GetTaskByIdQuery { Id = id });
            if (task == null)
                return NotFound(ApiResponse<TaskDto>.FailureResult("Task not found"));

            return Success(task, "Task retrieved successfully");
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<int>>> CreateTask(CreateTaskCommand command)
        {
            var taskId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTask), new { id = taskId },
                ApiResponse<int>.SuccessResult(taskId, "Task created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> UpdateTask(int id, UpdateTaskCommand command)
        {
            command.Id = id;
            await _mediator.Send(command);
            return Success("Task updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteTask(int id)
        {
            await _mediator.Send(new DeleteTaskCommand { Id = id });
            return Success("Task deleted successfully");
        }
    }
}
