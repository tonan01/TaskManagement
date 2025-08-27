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
    public class CategoriesController : BaseApiController
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetCategories()
        {
            var categories = await _mediator.Send(new GetAllCategoriesQuery());
            return Success(categories, "Categories retrieved successfully");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
        {
            var category = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
            if (category == null)
                return NotFound(ApiResponse<CategoryDto>.FailureResult("Category not found"));

            return Success(category, "Category retrieved successfully");
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<int>>> CreateCategory(CreateCategoryCommand command)
        {
            var categoryId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetCategory), new { id = categoryId },
                ApiResponse<int>.SuccessResult(categoryId, "Category created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> UpdateCategory(int id, UpdateCategoryCommand command)
        {
            command.Id = id;
            await _mediator.Send(command);
            return Success("Category updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteCategory(int id)
        {
            await _mediator.Send(new DeleteCategoryCommand { Id = id });
            return Success("Category deleted successfully");
        }
    }
}
