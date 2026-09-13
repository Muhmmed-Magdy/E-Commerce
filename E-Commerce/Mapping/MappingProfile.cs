using AutoMapper;
using E_Commerce.Models;
using E_Commerce.ViewModels;

namespace E_Commerce.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductViewModel>();
        CreateMap<ProductViewModel, Product>();

        // Category mappings
        CreateMap<Category, CategoryViewModel>();
        CreateMap<CategoryViewModel, Category>();
    }
}