import api from './api'
import type { Cart } from '@/types'

export const cartService = {
  async getCart(): Promise<Cart> {
    const { data } = await api.get<Cart>('/cart')
    return data
  },

  async addItem(productId: string, quantity: number) {
    const { data } = await api.post('/cart/items', { productId, quantity })
    return data
  },

  async updateItem(itemId: string, quantity: number) {
    const { data } = await api.put(`/cart/items/${itemId}`, { quantity })
    return data
  },

  async removeItem(itemId: string) {
    await api.delete(`/cart/items/${itemId}`)
  },
}
