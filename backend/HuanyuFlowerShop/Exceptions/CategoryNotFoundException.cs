namespace HuanyuFlowerShop.Exceptions
{
    public class CategoryNotFoundException : BusinessException
    {
        public int CategoryId { get; }

        public CategoryNotFoundException(int categoryId) 
            : base($"分类未找到，ID: {categoryId}")
        {
            CategoryId = categoryId;
        }

        public CategoryNotFoundException(int categoryId, string message) 
            : base(message)
        {
            CategoryId = categoryId;
        }

        public CategoryNotFoundException(int categoryId, string message, Exception innerException) 
            : base(message, innerException)
        {
            CategoryId = categoryId;
        }
    }
}