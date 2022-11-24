﻿using Microsoft.EntityFrameworkCore;
using Multitenant.Core.Entities;
using Multitenant.Core.Interfaces;
using Multitenant.Infrastructure.Persistence;

namespace Multitenant.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        
        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Product> CreateAsync(string name, string description, int rate)
        {
            var product = new Product(name, description, rate);
            
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            
            return product;
        }
        
        public async Task<IReadOnlyList<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }
        
        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }
    }
}
