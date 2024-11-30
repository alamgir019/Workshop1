namespace SimpleWS_Server.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SimpleWS_Server.DataContext;
    using SimpleWS_Server.Model;
    using SimpleWS_Server.Service;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly DbContextClass _dbContext;
        private readonly ICacheService _cacheService;
        public ProductController(DbContextClass dbContext, ICacheService cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
        }

        [HttpHead("private/products")]
        [HttpGet("private/products")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client, NoStore = false)]
        public IEnumerable<Product> Get()
        {
            // get data from redis cache
            var cacheData = _cacheService.GetData<IEnumerable<Product>>("product");
            if (cacheData != null)
            {
                return cacheData;
            }
            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
            // get data from mysql database
            cacheData = _dbContext.Products.ToList();
            // save data to redis cache
            _cacheService.SetData<IEnumerable<Product>>("product", cacheData, expirationTime);
            return cacheData;
        }

        [HttpHead("public/products")]
        [HttpGet("public/products")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public IEnumerable<Product> GetPublic()
        {
            var cacheData = _cacheService.GetData<IEnumerable<Product>>("product");
            if (cacheData != null)
            {
                return cacheData;
            }
            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
            cacheData = _dbContext.Products.ToList();
            _cacheService.SetData<IEnumerable<Product>>("product", cacheData, expirationTime);
            return cacheData;
        }

        [HttpHead("no-cache/products")]
        [HttpGet("no-cache/products")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.None)]
        public IEnumerable<Product> GetNoCache()
        {
            var cacheData = _cacheService.GetData<IEnumerable<Product>>("product");
            if (cacheData != null)
            {
                return cacheData;
            }
            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
            cacheData = _dbContext.Products.ToList();
            _cacheService.SetData<IEnumerable<Product>>("product", cacheData, expirationTime);
            return cacheData;
        }

        [HttpHead("no-store/products")]
        [HttpGet("no-store/products")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = true)]
        public IEnumerable<Product> GetNoStore()
        {
            var cacheData = _cacheService.GetData<IEnumerable<Product>>("product");
            if (cacheData != null)
            {
                return cacheData;
            }
            var expirationTime = DateTimeOffset.Now.AddMinutes(5.0);
            cacheData = _dbContext.Products.ToList();
            _cacheService.SetData<IEnumerable<Product>>("product", cacheData, expirationTime);
            return cacheData;
        }

        [HttpGet("product")]
        public Product Get(int id)
        {
            Product filteredData;
            var cacheData = _cacheService.GetData<IEnumerable<Product>>("product");
            if (cacheData != null)
            {
                filteredData = cacheData.Where(x => x.ProductId == id).FirstOrDefault();
                return filteredData;
            }
            filteredData = _dbContext.Products.Where(x => x.ProductId == id).FirstOrDefault();
            return filteredData;
        }
        [HttpPost("addproduct")]
        public async Task<Product> Post(Product value)
        {
            var obj = await _dbContext.Products.AddAsync(value);
            _cacheService.RemoveData("product");
            _dbContext.SaveChanges();
            return obj.Entity;
        }
        [HttpPut("updateproduct")]
        public void Put(Product product)
        {
            _dbContext.Products.Update(product);
            _cacheService.RemoveData("product");
            _dbContext.SaveChanges();
        }
        [HttpDelete("deleteproduct")]
        public void Delete(int Id)
        {
            var filteredData = _dbContext.Products.Where(x => x.ProductId == Id).FirstOrDefault();
            _dbContext.Remove(filteredData!);
            _cacheService.RemoveData("product");
            _dbContext.SaveChanges();
        }

        [HttpDelete("delete-cache/product")]
        public void DeleteCache()
        {
            _cacheService.RemoveData("product");
        }
    }
}
