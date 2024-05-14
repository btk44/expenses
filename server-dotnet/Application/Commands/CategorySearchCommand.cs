using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Models;

namespace TransactionService.Application.Commands;

public class CategorySearchCommand {
    public int OwnerId { get; set; }
    public string Name { get; set; }
    public int? Id { get; set; }
    public int? ParentId { get; set; }
    public bool? Active { get; set; }
}

public class CategorySearchCommandHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _categoryMapper;

    public CategorySearchCommandHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _categoryMapper = mapper;
    }

    public async Task<List<CategoryDto>> Handle(CategorySearchCommand command)
    {
        var categoryQuery = _dbContext.Categories.Where(x => x.OwnerId == command.OwnerId);

        if(command.Active.HasValue)
            categoryQuery = categoryQuery.Where(x => x.Active == command.Active);

        if(!string.IsNullOrEmpty(command.Name)){
            var categoryName = command.Name.ToLower();
            categoryQuery = categoryQuery.Where(x => x.Name.ToLower().Contains(categoryName));
        }

        if(command.Id.HasValue)
            categoryQuery = categoryQuery.Where(x => x.Id == command.Id);

        if(command.ParentId.HasValue)
            categoryQuery = categoryQuery.Where(x => x.ParentId == command.ParentId);

        var categories = await categoryQuery.Select(x => _categoryMapper.Map<CategoryDto>(x)).ToListAsync();

        // consider movig it elswhere:
        // var categoriesIds = categories.Select(x => x.Id);
        // var visualProps = await _dbContext.VisualProperties.Where(x => x.ObjectName == "Category" && categoriesIds.Contains(x.ObjectId)).ToDictionaryAsync(x => x.ObjectId);
        // categories.ForEach(x => x.Color = visualProps[x.Id].Color);
        //----

        return categories;
    }
}