using AutoMapper;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mappings cho Customer
        CreateMap<Customer, CustomerListDto>();
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();
        CreateMap<Customer, UpdateCustomerDto>();

        // Mappings cho Article
        CreateMap<Article, ArticleListDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));
        CreateMap<CreateArticleDto, Article>();
        CreateMap<UpdateArticleDto, Article>();
        CreateMap<Article, UpdateArticleDto>();

        // Mappings cho ArticleCategory
        CreateMap<ArticleCategory, ArticleCategoryListDto>()
            .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null))
            .ForMember(dest => dest.ArticleCount, opt => opt.MapFrom(src => src.Articles != null ? src.Articles.Count : 0));
        CreateMap<CreateArticleCategoryDto, ArticleCategory>();
        CreateMap<UpdateArticleCategoryDto, ArticleCategory>();
        CreateMap<ArticleCategory, UpdateArticleCategoryDto>();

        // Mappings cho ArticleComment
        CreateMap<ArticleComment, ArticleCommentListDto>()
            .ForMember(dest => dest.ArticleTitle, opt => opt.MapFrom(src => src.Article != null ? src.Article.Title : string.Empty));

        // Mappings cho Masterdata - DefaultTimeSlot
        CreateMap<DefaultTimeSlot, DefaultTimeSlotListDto>();
        CreateMap<CreateDefaultTimeSlotDto, DefaultTimeSlot>();
        CreateMap<UpdateDefaultTimeSlotDto, DefaultTimeSlot>();
        CreateMap<DefaultTimeSlot, UpdateDefaultTimeSlotDto>();

        // Mappings cho Masterdata - ServicePrice
        CreateMap<ServicePrice, ServicePriceListDto>();
        CreateMap<CreateServicePriceDto, ServicePrice>();
        CreateMap<UpdateServicePriceDto, ServicePrice>();
        CreateMap<ServicePrice, UpdateServicePriceDto>();

        // Mappings cho Masterdata - ProductCategory
        CreateMap<ProductCategory, ProductCategoryListDto>()
            .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null));
        CreateMap<CreateProductCategoryDto, ProductCategory>();
        CreateMap<UpdateProductCategoryDto, ProductCategory>();
        CreateMap<ProductCategory, UpdateProductCategoryDto>();

        // Mappings cho Masterdata - Unit
        CreateMap<Unit, UnitListDto>();
        CreateMap<CreateUnitDto, Unit>();
        CreateMap<UpdateUnitDto, Unit>();
        CreateMap<Unit, UpdateUnitDto>();

        // Mappings cho Promotion
        CreateMap<Promotion, PromotionListDto>();
        CreateMap<CreatePromotionDto, Promotion>();
        CreateMap<UpdatePromotionDto, Promotion>();
        CreateMap<Promotion, UpdatePromotionDto>();

        // Mappings cho Shop - Product
        CreateMap<Product, ProductListDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : string.Empty))
            .ForMember(dest => dest.UnitSymbol, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Symbol : string.Empty));
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
        CreateMap<Product, UpdateProductDto>();

        // Mappings cho Shop - Order
        CreateMap<Order, OrderListDto>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderItems != null ? src.OrderItems.Count : 0));

        // Mappings cho Shop - StockImport
        CreateMap<StockImport, StockImportListDto>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.ImportItems != null ? src.ImportItems.Count : 0));

        // Mappings cho Finance - CashTransaction
        CreateMap<CashTransaction, CashTransactionListDto>();
        CreateMap<CreateCashTransactionDto, CashTransaction>();
        CreateMap<UpdateCashTransactionDto, CashTransaction>();
        CreateMap<CashTransaction, UpdateCashTransactionDto>();
    }
}
