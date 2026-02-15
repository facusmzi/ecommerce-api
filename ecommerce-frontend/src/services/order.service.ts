import api from './api'
import type { Order } from '@/types'

export const orderService = {
  async getOrders(): Promise<Order[]> {
    const { data } = await api.get<Order[]>('/orders')
    return data
  },

  async getOrderById(id: string): Promise<Order> {
    const { data } = await api.get<Order>(`/orders/${id}`)
    return data
  },

  async createOrder(): Promise<Order> {
    const { data } = await api.post<Order>('/orders')
    return data
  },

  async createPayment(orderId: string): Promise<{ preferenceId: string; initPoint: string }> {
    const { data } = await api.post(`/orders/${orderId}/payment`)
    return data
  },
}
