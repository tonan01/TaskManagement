using AutoMapper;
using TaskManagement.Application.Features.Tasks.Commands;
using TaskManagement.Application.Features.Tasks.Queries;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Task mappings
            CreateMap<TaskItem, TaskDto>()
                .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name));

            CreateMap<CreateTaskCommand, TaskItem>();
            CreateMap<UpdateTaskCommand, TaskItem>()
                .ForMember(d => d.Id, opt => opt.Ignore()); // Don't map Id from command to entity

            // Category mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(d => d.TaskCount, opt => opt.MapFrom(s => s.Tasks.Count(t => !t.IsDeleted)));

            CreateMap<CreateCategoryCommand, Category>();
            CreateMap<UpdateCategoryCommand, Category>()
                .ForMember(d => d.Id, opt => opt.Ignore()); // Don't map Id from command to entity
        }
    }
}
