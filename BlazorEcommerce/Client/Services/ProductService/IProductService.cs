﻿namespace BlazorEcommerce.Client.Services.ProductService
{
    public interface IProductService
    {
        event Action ProductsChanged;

        List<Product> Products { get; set; }

        // message for when searching for products
        string Message { get; set; }

        int CurrentPage { get; set; }

        int PageCount { get; set; }

        string LastSearchText { get; set; }

        Task GetProducts(string? categoryUrl = null);

        Task<ServiceResponse<Product>> GetProduct(int productId);

        Task SearchProducts(string searchText, int page);

        Task<List<string>> GetProductSuggestions(string searchText);
    }

    public class ProductService : IProductService
    {
        private readonly HttpClient _http;

        public ProductService(HttpClient http)
        {
            _http = http;
        }

        public event Action ProductsChanged;

        public List<Product> Products { get; set; } = new List<Product>();

        public string Message { get; set; } = "Loading products...";

        public int CurrentPage { get; set; } = 1;

        public int PageCount { get; set; } = 0;

        public string LastSearchText { get; set; } = string.Empty;

        public async Task GetProducts(string? categoryUrl)
        {

            var result = categoryUrl == null ?
                await _http.GetFromJsonAsync<ServiceResponse<List<Product>>>("api/product/featured") :
                await _http.GetFromJsonAsync<ServiceResponse<List<Product>>>($"api/product/category/{categoryUrl}");

            if (result != null && result.Data != null) Products = result.Data;

            CurrentPage = 1;
            PageCount = 0;

            if (Products.Count == 0) Message = "No products found.";

            ProductsChanged.Invoke();
        }

        public async Task<ServiceResponse<Product>> GetProduct(int productId)
        {
            var result = await _http.GetFromJsonAsync<ServiceResponse<Product>>($"api/product/{productId}");

            return result;
        }

        public async Task SearchProducts(string searchText, int page)
        {
            LastSearchText = searchText;

            var result = await _http.GetFromJsonAsync<ServiceResponse<ProductSearchResult>>($"api/product/search/{searchText}/{page}");

            if (result != null && result.Data != null)
            {
                Products = result.Data.Products;
                CurrentPage = result.Data.CurrentPage;
                PageCount = result.Data.PageCount;
            }

            if (Products.Count == 0) Message = "No products found.";

            ProductsChanged.Invoke();
        }

        public async Task<List<string>> GetProductSuggestions(string searchText)
        {
            var result = await _http.GetFromJsonAsync<ServiceResponse<List<string>>>($"api/product/searchsuggestions/{searchText}");

            return result.Data;
        }
    }
}