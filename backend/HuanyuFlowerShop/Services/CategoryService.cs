using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Repositories;
using HuanyuFlowerShop.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace HuanyuFlowerShop.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto);
        Task<bool> DeleteCategoryAsync(int id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CategoryService> _logger;
        private const string ALL_CATEGORIES_CACHE_KEY = "all_categories";
        private const string CATEGORY_CACHE_KEY_PREFIX = "category_";

        public CategoryService(ICategoryRepository categoryRepository, ICacheService cacheService, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            try
            {
                // 尝试从缓存获取
                var cachedCategories = await _cacheService.GetAsync<IEnumerable<CategoryDto>>(ALL_CATEGORIES_CACHE_KEY);
                if (cachedCategories != null)
                {
                    _logger.LogInformation("从缓存获取所有分类");
                    return cachedCategories;
                }

                // 缓存不存在，从数据库获取
                var categories = await _categoryRepository.GetAllAsync();
                var categoryDtos = (categories ?? Enumerable.Empty<Category>()).Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name ?? string.Empty,
                    Description = c.Description ?? string.Empty,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                });

                // 设置缓存，缓存30分钟
                await _cacheService.SetAsync(ALL_CATEGORIES_CACHE_KEY, categoryDtos, TimeSpan.FromMinutes(30));
                _logger.LogInformation("获取并缓存所有分类，数量: {Count}", categories?.Count() ?? 0);
                
                return categoryDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有分类时发生错误");
                throw;
            }
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("无效的分类ID: {CategoryId}", id);
                    throw new ArgumentException("分类ID必须大于0", nameof(id));
                }

                // 尝试从缓存获取
                var cacheKey = $"{CATEGORY_CACHE_KEY_PREFIX}{id}";
                var cachedCategory = await _cacheService.GetAsync<CategoryDto>(cacheKey);
                if (cachedCategory != null)
                {
                    _logger.LogInformation("从缓存获取分类，ID: {CategoryId}", id);
                    return cachedCategory;
                }

                // 缓存不存在，从数据库获取
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("分类不存在，ID: {CategoryId}", id);
                    return null;
                }

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name ?? string.Empty,
                    Description = category.Description ?? string.Empty,
                    SortOrder = category.SortOrder,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt
                };

                // 设置缓存，缓存30分钟
                await _cacheService.SetAsync(cacheKey, categoryDto, TimeSpan.FromMinutes(30));
                _logger.LogInformation("获取并缓存分类，ID: {CategoryId}", id);
                
                return categoryDto;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类时发生错误，ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            try
            {
                var category = new Category
                {
                    Name = createCategoryDto.Name ?? string.Empty,
                    Description = createCategoryDto.Description ?? string.Empty,
                    SortOrder = createCategoryDto.SortOrder,
                    IsActive = createCategoryDto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                var createdCategory = await _categoryRepository.CreateAsync(category);

                // 清除相关缓存
                await ClearCategoryCaches();
                
                var categoryDto = new CategoryDto
                {
                    Id = createdCategory.Id,
                    Name = createdCategory.Name ?? string.Empty,
                    Description = createdCategory.Description ?? string.Empty,
                    SortOrder = createdCategory.SortOrder,
                    IsActive = createdCategory.IsActive,
                    CreatedAt = createdCategory.CreatedAt,
                    UpdatedAt = createdCategory.UpdatedAt
                };
                
                _logger.LogInformation("成功创建分类，ID: {CategoryId}, 名称: {CategoryName}", createdCategory.Id, createdCategory.Name);
                
                return categoryDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建分类时发生错误");
                throw;
            }
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("无效的分类ID: {CategoryId}", id);
                    throw new ArgumentException("分类ID必须大于0", nameof(id));
                }

                var category = new Category
                {
                    Name = updateCategoryDto.Name ?? string.Empty,
                    Description = updateCategoryDto.Description ?? string.Empty,
                    SortOrder = updateCategoryDto.SortOrder ?? 0,
                    IsActive = updateCategoryDto.IsActive ?? true
                };

                var updatedCategory = await _categoryRepository.UpdateAsync(id, category);
                if (updatedCategory == null)
                {
                    _logger.LogWarning("更新分类失败，分类不存在，ID: {CategoryId}", id);
                    return null;
                }

                // 清除相关缓存
                await ClearCategoryCaches();
                
                var categoryDto = new CategoryDto
                {
                    Id = updatedCategory.Id,
                    Name = updatedCategory.Name ?? string.Empty,
                    Description = updatedCategory.Description ?? string.Empty,
                    SortOrder = updatedCategory.SortOrder,
                    IsActive = updatedCategory.IsActive,
                    CreatedAt = updatedCategory.CreatedAt,
                    UpdatedAt = updatedCategory.UpdatedAt
                };
                
                _logger.LogInformation("成功更新分类，ID: {CategoryId}, 名称: {CategoryName}", updatedCategory.Id, updatedCategory.Name);
                
                return categoryDto;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新分类时发生错误，ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("无效的分类ID: {CategoryId}", id);
                    throw new ArgumentException("分类ID必须大于0", nameof(id));
                }

                var result = await _categoryRepository.DeleteAsync(id);
                
                if (result)
                {
                    // 清除相关缓存
                    await ClearCategoryCaches();
                    _logger.LogInformation("成功删除分类，ID: {CategoryId}", id);
                }
                else
                {
                    _logger.LogWarning("删除分类失败，分类不存在，ID: {CategoryId}", id);
                }
                
                return result;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除分类时发生错误，ID: {CategoryId}", id);
                throw;
            }
        }
        
        /// <summary>
        /// 清除分类相关缓存
        /// </summary>
        private async Task ClearCategoryCaches()
        {
            // 清除所有分类缓存
            await _cacheService.RemoveAsync(ALL_CATEGORIES_CACHE_KEY);
            
            // 清除所有单个分类缓存
            await _cacheService.RemoveByPatternAsync($"^{CATEGORY_CACHE_KEY_PREFIX}");
            
            _logger.LogInformation("已清除所有分类相关缓存");
        }
    }
}