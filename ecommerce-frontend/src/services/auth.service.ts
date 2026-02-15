import api from './api'
import type { LoginResponse } from '@/types'

export const authService = {
  async register(email: string, password: string) {
    const { data } = await api.post('/auth/register', { email, password })
    return data
  },

  async verifyEmail(email: string, code: string) {
    const { data } = await api.post('/auth/verify-email', { email, code })
    return data
  },

  async login(email: string, password: string): Promise<LoginResponse> {
    const { data } = await api.post<LoginResponse>('/auth/login', { email, password })
    return data
  },

  async logout() {
    await api.post('/auth/logout')
  },
}
