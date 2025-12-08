using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Common.Interfaces
{
    public interface IAIGenerationService
    {
        Task<string> AnalyzeTryOnAsync(string userImageBase64, string clothingImageBase64, string clothingType);


    }
}
