using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // product controller api
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _productService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) => Ok(await _productService.GetByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            await _productService.AddAsync(dto);
            return Ok();
        }
        [HttpPut]
        public async Task<IActionResult> Update(UpdateProducDto dto)
        {
            await _productService.UpdateAsync(dto);
            return Ok();
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok();
        }

    }
}
