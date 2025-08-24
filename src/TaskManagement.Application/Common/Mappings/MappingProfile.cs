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
            CreateMap<TaskItem, TaskDto>()
       .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name));

            CreateMap<CreateTaskCommand, TaskItem>();
            //CreateMap<UpdateTaskCommand, TaskItem>();

            //CreateMap<Category, CategoryDto>();
            //CreateMap<CreateCategoryCommand, Category>();
        }
    }
}
