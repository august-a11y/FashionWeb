using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Common.Interfaces
{

    public interface IUserContext
    {
        Guid? UserId { get; set; }
        string SessionId { get; set; }
        bool IsAuthenticated { get; }
    }
    
}
