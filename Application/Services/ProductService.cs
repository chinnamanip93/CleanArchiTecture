using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _mapper = mapper;
            _productRepository = productRepository;
        }
        public async Task AddAsync(CreateProductDto product)
        {
            await _productRepository.AddAsync(_mapper.Map<Product>(product));
        }

        public async Task DeleteAsync(int id)
        {
            await _productRepository.DeleteAsync(id);
        }

        public async Task<List<ProductDto>> GetAllAsync()
        {
            return _mapper.Map<List<ProductDto>>(await _productRepository.GetAllAsync());
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            return _mapper.Map<ProductDto>(await _productRepository.GetAsync(id));
        }

        public async Task UpdateAsync(UpdateProducDto product)
        {
            await _productRepository.UpdateAsync(_mapper.Map<Product>(product));
        }
    }
}
