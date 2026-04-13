namespace FashionShop.Application.Common.Interfaces
{

    public interface IRequestContext
    {
        Guid? UserId { get; set; }
        string SessionId { get; set; }
        bool IsAuthenticated { get; }
    }
    
}
