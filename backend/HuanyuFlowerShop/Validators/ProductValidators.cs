using FluentValidation;
using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Validators;

/// <summary>
/// 创建产品DTO验证器
/// </summary>
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("产品名称不能为空")
            .MaximumLength(100).WithMessage("产品名称不能超过100个字符")
            .MinimumLength(2).WithMessage("产品名称至少需要2个字符");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("产品描述不能超过500个字符");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("产品价格必须大于0")
            .LessThan(10000).WithMessage("产品价格不能超过10000元");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("库存不能为负数")
            .LessThan(10000).WithMessage("库存数量不能超过9999");

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("图片URL格式不正确");

        RuleFor(x => x.Size)
            .MaximumLength(50).WithMessage("尺寸规格不能超过50个字符");

        RuleFor(x => x.Material)
            .MaximumLength(100).WithMessage("材质描述不能超过100个字符");

        RuleFor(x => x.Occasion)
            .MaximumLength(100).WithMessage("适用场合不能超过100个字符");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).When(x => x.CategoryId.HasValue)
            .WithMessage("分类ID必须大于0");
    }

    /// <summary>
    /// 验证URL格式
    /// </summary>
    private bool BeAValidUrl(string? url)
    {
        return string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

/// <summary>
/// 更新产品DTO验证器
/// </summary>
public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().When(x => x.Name != null)
            .WithMessage("产品名称不能为空")
            .MaximumLength(100).When(x => x.Name != null)
            .WithMessage("产品名称不能超过100个字符")
            .MinimumLength(2).When(x => x.Name != null)
            .WithMessage("产品名称至少需要2个字符");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description != null)
            .WithMessage("产品描述不能超过500个字符");

        RuleFor(x => x.Price)
            .GreaterThan(0).When(x => x.Price.HasValue)
            .WithMessage("产品价格必须大于0")
            .LessThan(10000).When(x => x.Price.HasValue)
            .WithMessage("产品价格不能超过10000元");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).When(x => x.Stock.HasValue)
            .WithMessage("库存不能为负数")
            .LessThan(10000).When(x => x.Stock.HasValue)
            .WithMessage("库存数量不能超过9999");

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("图片URL格式不正确");

        RuleFor(x => x.Size)
            .MaximumLength(50).When(x => x.Size != null)
            .WithMessage("尺寸规格不能超过50个字符");

        RuleFor(x => x.Material)
            .MaximumLength(100).When(x => x.Material != null)
            .WithMessage("材质描述不能超过100个字符");

        RuleFor(x => x.Occasion)
            .MaximumLength(100).When(x => x.Occasion != null)
            .WithMessage("适用场合不能超过100个字符");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).When(x => x.CategoryId.HasValue)
            .WithMessage("分类ID必须大于0");
    }

    /// <summary>
    /// 验证URL格式
    /// </summary>
    private bool BeAValidUrl(string? url)
    {
        return string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}