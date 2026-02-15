import api from './api'
import type { Product, PagedResponse, Category } from '@/types'

export const productService = {
  async getProducts(params: {
    page?: number
    pageSize?: number
    categoryId?: string
    search?: string
  }): Promise<PagedResponse<Product>> {
    const { data } = await api.get<PagedResponse<Product>>('/products', { params })
    return data
  },

  async getProductById(id: string): Promise<Product> {
    const { data } = await api.get<Product>(`/products/${id}`)
    return data
  },

  async getCategories(): Promise<Category[]> {
    const { data } = await api.get<Category[]>('/categories')
    return data
  },
}
