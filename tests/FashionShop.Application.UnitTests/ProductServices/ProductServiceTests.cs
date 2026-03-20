using Bogus;
using FashionShop.Application.Interfaces;
using FashionShop.Application.ProductServices;
using FashionShop.Application.ProductServices.DTO;
using FashionShop.Domain.Entities;
using Moq;

namespace FashionShop.Application.UnitTests.ProductServices;

public class ProductServiceTests
{
    public static IEnumerable<object[]> GetAllProductTheoryData()
    {
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.BasePrice, f => f.Random.Bool() ? f.Finance.Amount(1, 999999) : null)
            .RuleFor(p => p.ThumbnailUrl, f => f.Image.PicsumUrl())
            .RuleFor(p => p.CategoryId, _ => Guid.NewGuid());

        yield return new object[] { new List<Product>() };
        yield return new object[] { productFaker.Generate(1) };
        yield return new object[] { productFaker.Generate(3) };
        yield return new object[] { productFaker.Generate(5) };
    }

    public static IEnumerable<object[]> GetProductByIdTheoryData()
    {
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.BasePrice, f => f.Random.Bool() ? f.Finance.Amount(1, 999999) : null)
            .RuleFor(p => p.ThumbnailUrl, f => f.Image.PicsumUrl())
            .RuleFor(p => p.CategoryId, _ => Guid.NewGuid());

        var hasPriceProduct = productFaker.Generate();
        hasPriceProduct.BasePrice = new Faker().Finance.Amount(1, 999999);

        var noPriceProduct = productFaker.Generate();
        noPriceProduct.BasePrice = null;

        yield return new object[] { hasPriceProduct };
        yield return new object[] { noPriceProduct };
        yield return new object[] { productFaker.Generate() };
    }

    public static IEnumerable<object[]> UpdateProductTheoryData()
    {
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.BasePrice, f => f.Finance.Amount(1, 999999))
            .RuleFor(p => p.ThumbnailUrl, f => f.Image.PicsumUrl())
            .RuleFor(p => p.CategoryId, _ => Guid.NewGuid());

        var updateDtoFaker = new Faker<UpdateDetailsProductDTO>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.ThumbnailUrl, f => f.Image.PicsumUrl())
            .RuleFor(p => p.Price, f => f.Finance.Amount(1, 999999));

        var existing1 = productFaker.Generate();
        var update1 = updateDtoFaker.Generate();

        var existing2 = productFaker.Generate();
        var update2 = updateDtoFaker.Generate();
        update2.Name = null;
        update2.Price = null;

        var existing3 = productFaker.Generate();
        var update3 = updateDtoFaker.Generate();
        update3.Description = null;
        update3.ThumbnailUrl = null;

        yield return new object[] { existing1, update1 };
        yield return new object[] { existing2, update2 };
        yield return new object[] { existing3, update3 };
    }

    [Theory]
    [MemberData(nameof(GetAllProductTheoryData))]
    public async Task GetAllProductAsync_Theory_ShouldMapAllFields(List<Product> products)
    {
        // Arrange
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await service.GetAllProductAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var mapped = result.Value.ToList();
        Assert.Equal(products.Count, mapped.Count);

        for (var i = 0; i < products.Count; i++)
        {
            Assert.Equal(products[i].Name, mapped[i].Name);
            Assert.Equal(products[i].Description, mapped[i].Description);
            Assert.Equal(products[i].BasePrice ?? 0, mapped[i].Price);
            Assert.Equal(products[i].ThumbnailUrl, mapped[i].ThumbnailUrl);
            Assert.Equal(products[i].CategoryId, mapped[i].CategoryId);
        }
    }

    [Theory]
    [MemberData(nameof(GetProductByIdTheoryData))]
    public async Task GetProductByIdAsync_Theory_ShouldMapAllFields(Product product)
    {
        // Arrange
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await service.GetProductByIdAsync(product.Id, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(product.Name, result.Value.Name);
        Assert.Equal(product.Description, result.Value.Description);
        Assert.Equal(product.BasePrice ?? 0, result.Value.Price);
        Assert.Equal(product.ThumbnailUrl, result.Value.ThumbnailUrl);
        Assert.Equal(product.CategoryId, result.Value.CategoryId);
    }

    [Theory]
    [MemberData(nameof(UpdateProductTheoryData))]
    public async Task UpdateProductAsync_Theory_ShouldUpdateByDomainRules(Product existingProduct, UpdateDetailsProductDTO updateDto)
    {
        // Arrange
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var expectedName = string.IsNullOrEmpty(updateDto.Name) ? existingProduct.Name : updateDto.Name;
        var expectedDescription = string.IsNullOrEmpty(updateDto.Description) ? existingProduct.Description : updateDto.Description;
        var expectedThumbnailUrl = string.IsNullOrEmpty(updateDto.ThumbnailUrl) ? existingProduct.ThumbnailUrl : updateDto.ThumbnailUrl;
        var expectedPrice = updateDto.Price ?? existingProduct.BasePrice ?? 0;

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(existingProduct.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await service.UpdateProductAsync(existingProduct.Id, updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedName, result.Value.Name);
        Assert.Equal(expectedDescription, result.Value.Description);
        Assert.Equal(expectedThumbnailUrl, result.Value.ThumbnailUrl);
        Assert.Equal(expectedPrice, result.Value.Price);
        Assert.Equal(existingProduct.CategoryId, result.Value.CategoryId);

        productRepositoryMock.Verify(r => r.GetByIdAsync(existingProduct.Id, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Create")]
    [InlineData("Delete")]
    [InlineData("Update")]
    public async Task WriteMethods_ShouldPassCancellationToken_ToUnitOfWorkCommit(string operation)
    {
        // Arrange
        var token = new CancellationTokenSource().Token;
        var id = Guid.NewGuid();
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, token))
            .ReturnsAsync(new Product { Id = id, Name = "Old", Description = "Desc", ThumbnailUrl = "old.png", BasePrice = 100000, CategoryId = Guid.NewGuid() });

        unitOfWorkMock
            .Setup(u => u.CommitAsync(token))
            .ReturnsAsync(1);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        switch (operation)
        {
            case "Create":
                await service.CreateProductAsync(new CreateProductDTO
                {
                    Name = "Áo len",
                    Description = "Warm",
                    BasePrice = 450000,
                    ThumbnailUrl = "thumb",
                    CategoryId = Guid.NewGuid()
                }, token);
                break;
            case "Delete":
                await service.DeleteProductAsync(id, token);
                break;
            case "Update":
                await service.UpdateProductAsync(id, new UpdateDetailsProductDTO { Name = "New" }, token);
                break;
        }

        // Assert
        unitOfWorkMock.Verify(u => u.CommitAsync(token), Times.Once);
    }

    [Theory]
    [InlineData("GetAll")]
    [InlineData("GetById")]
    [InlineData("Delete")]
    [InlineData("Update")]
    public async Task ReadMethods_ShouldPassCancellationToken_ToRepository(string operation)
    {
        // Arrange
        var token = new CancellationTokenSource().Token;
        var id = Guid.NewGuid();
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetAllAsync(token))
            .ReturnsAsync(new List<Product>());

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, token))
            .ReturnsAsync(new Product { Id = id, Name = "Old", Description = "Desc", ThumbnailUrl = "old.png", BasePrice = 100000, CategoryId = Guid.NewGuid() });

        unitOfWorkMock
            .Setup(u => u.CommitAsync(token))
            .ReturnsAsync(1);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        switch (operation)
        {
            case "GetAll":
                await service.GetAllProductAsync(token);
                break;
            case "GetById":
                await service.GetProductByIdAsync(id, token);
                break;
            case "Delete":
                await service.DeleteProductAsync(id, token);
                break;
            case "Update":
                await service.UpdateProductAsync(id, new UpdateDetailsProductDTO { Name = "New" }, token);
                break;
        }

        // Assert
        if (operation == "GetAll")
        {
            productRepositoryMock.Verify(r => r.GetAllAsync(token), Times.Once);
            return;
        }

        productRepositoryMock.Verify(r => r.GetByIdAsync(id, token), Times.Once);
    }

    [Theory]
    [InlineData("GetById")]
    [InlineData("Delete")]
    [InlineData("Update")]
    public async Task GetByIdBasedMethods_ShouldThrow_WhenRepositoryGetByIdThrows(string operation)
    {
        // Arrange
        var id = Guid.NewGuid();
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Read error"));

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        Func<Task> act = operation switch
        {
            "GetById" => () => service.GetProductByIdAsync(id, CancellationToken.None),
            "Delete" => () => service.DeleteProductAsync(id, CancellationToken.None),
            "Update" => () => service.UpdateProductAsync(id, new UpdateDetailsProductDTO { Name = "New" }, CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);

        if (operation == "Delete")
        {
            productRepositoryMock.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
        }
    }

    [Theory]
    [InlineData("Create")]
    [InlineData("Delete")]
    [InlineData("Update")]
    public async Task WriteMethods_ShouldThrow_WhenCommitThrows(string operation)
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = id,
            Name = "Old",
            Description = "Desc",
            ThumbnailUrl = "old.png",
            BasePrice = 100000,
            CategoryId = Guid.NewGuid()
        };

        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Commit failed"));

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        Func<Task> act = operation switch
        {
            "Create" => () => service.CreateProductAsync(new CreateProductDTO
            {
                Name = "Váy",
                Description = "Summer dress",
                BasePrice = 399000,
                ThumbnailUrl = "thumb",
                CategoryId = Guid.NewGuid()
            }, CancellationToken.None),
            "Delete" => () => service.DeleteProductAsync(id, CancellationToken.None),
            "Update" => () => service.UpdateProductAsync(id, new UpdateDetailsProductDTO { Name = "New" }, CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

        // Assert
        await Assert.ThrowsAsync<Exception>(act);

        if (operation == "Create")
        {
            productRepositoryMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        }

        if (operation == "Delete")
        {
            productRepositoryMock.Verify(r => r.Remove(It.IsAny<Product>()), Times.Once);
        }

        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldAddProduct_CommitAndReturnMappedDto_WhenInputIsValid()
    {
        // Arrange
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var cancellationToken = new CancellationTokenSource().Token;

        Product? addedProduct = null;
        productRepositoryMock
            .Setup(r => r.Add(It.IsAny<Product>()))
            .Callback<Product>(p => addedProduct = p);

        unitOfWorkMock
            .Setup(u => u.CommitAsync(cancellationToken))
            .ReturnsAsync(1);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        var request = new CreateProductDTO
        {
            Name = "Áo sơ mi",
            Description = "Chất liệu cotton",
            BasePrice = 299000,
            ThumbnailUrl = "https://cdn.example.com/aoso.png",
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = await service.CreateProductAsync(request, cancellationToken);

        // Assert
        Assert.NotNull(addedProduct);
        Assert.Equal(request.Name, addedProduct!.Name);
        Assert.Equal(request.Description, addedProduct.Description);
        Assert.Equal(request.BasePrice, addedProduct.BasePrice);
        Assert.Equal(request.ThumbnailUrl, addedProduct.ThumbnailUrl);
        Assert.Equal(request.CategoryId, addedProduct.CategoryId);

        Assert.Equal(request.Name, result.Value.Name);
        Assert.Equal(request.Description, result.Value.Description);
        Assert.Equal(request.BasePrice, result.Value.Price);
        Assert.Equal(request.ThumbnailUrl, result.Value.ThumbnailUrl);
        Assert.Equal(request.CategoryId, result.Value.CategoryId);

        productRepositoryMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldReturnPriceZero_WhenInputBasePriceIsZero()
    {
        // Arrange
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var cancellationToken = CancellationToken.None;

        unitOfWorkMock
            .Setup(u => u.CommitAsync(cancellationToken))
            .ReturnsAsync(1);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        var request = new CreateProductDTO
        {
            Name = "Áo thun",
            Description = "Basic",
            BasePrice = 0,
            ThumbnailUrl = "thumb",
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = await service.CreateProductAsync(request, cancellationToken);

        // Assert
        Assert.Equal(0, result.Value.Price);
        productRepositoryMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldThrow_WhenRepositoryAddThrows()
    {
        // Arrange
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.Add(It.IsAny<Product>()))
            .Throws(new InvalidOperationException("Cannot add product"));

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        var request = new CreateProductDTO
        {
            Name = "Quần jean",
            Description = "Slim fit",
            BasePrice = 499000,
            ThumbnailUrl = "thumb",
            CategoryId = Guid.NewGuid()
        };

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateProductAsync(request, CancellationToken.None));

        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldStillReturnDto_WhenCommitResultIsZero()
    {
        // Arrange
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        var request = new CreateProductDTO
        {
            Name = "Áo khoác",
            Description = "Windbreaker",
            BasePrice = 650000,
            ThumbnailUrl = "thumb",
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = await service.CreateProductAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(request.Name, result.Value.Name);
        Assert.Equal(request.Description, result.Value.Description);
        Assert.Equal(request.BasePrice, result.Value.Price);
        Assert.Equal(request.CategoryId, result.Value.CategoryId);

        productRepositoryMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnOkAndDelete_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var product = new Product
        {
            Id = productId,
            Name = "Áo sơ mi",
            Description = "Cotton",
            BasePrice = 299000,
            ThumbnailUrl = "thumb",
            CategoryId = Guid.NewGuid()
        };

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await service.DeleteProductAsync(productId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        productRepositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        productRepositoryMock.Verify(r => r.Remove(product), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnFail_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await service.DeleteProductAsync(productId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal($"Product with id {productId} not found.", result.Errors[0].Message);
        productRepositoryMock.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldThrow_WhenRemoveThrows()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId };

        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        productRepositoryMock
            .Setup(r => r.Remove(product))
            .Throws(new Exception("Delete error"));

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<Exception>(() =>
            service.DeleteProductAsync(productId, CancellationToken.None));

        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllProductAsync_ShouldReturnFail_WhenRepositoryThrows()
    {
        // Arrange
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database read failed"));

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await service.GetAllProductAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains("Failed to retrieve products:", result.Errors[0].Message);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnFail_WhenProductNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await service.GetProductByIdAsync(productId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal($"Product with id {productId} not found.", result.Errors[0].Message);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnFail_WhenProductNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act
        var result = await service.UpdateProductAsync(productId, new UpdateDetailsProductDTO(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Product not found.", result.Errors[0].Message);
        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldThrow_WhenPriceIsNegative()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productRepositoryMock = new Mock<IProductRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product
            {
                Id = productId,
                Name = "Áo",
                Description = "Desc",
                ThumbnailUrl = "thumb",
                BasePrice = 120000,
                CategoryId = Guid.NewGuid()
            });

        var service = new ProductService(productRepositoryMock.Object, unitOfWorkMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateProductAsync(productId, new UpdateDetailsProductDTO { Price = -1 }, CancellationToken.None));

        unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

}
