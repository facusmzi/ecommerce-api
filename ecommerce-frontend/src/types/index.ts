export interface User {
  id: string
  email: string
  isEmailVerified: boolean
  isAdmin: boolean
}

export interface Category {
  id: string
  name: string
  description: string | null
  createdAt: string
}

export interface Product {
  id: string
  name: string
  description: string | null
  price: number
  stock: number
  imageUrl: string | null
  isActive: boolean
  categoryId: string
  categoryName: string
  createdAt: string
  updatedAt: string
}

export interface PagedResponse<T> {
  items: T[]
  totalItems: number
  page: number
  pageSize: number
  totalPages: number
}

export interface CartItem {
  id: string
  productId: string
  productName: string
  productImageUrl: string | null
  unitPrice: number
  quantity: number
  subtotal: number
}

export interface Cart {
  id: string
  items: CartItem[]
  total: number
  itemCount: number
}

export interface OrderItem {
  id: string
  productId: string
  productName: string
  quantity: number
  unitPrice: number
  subtotal: number
}

export interface Order {
  id: string
  status: string
  total: number
  items: OrderItem[]
  paymentStatus: string | null
  createdAt: string
  updatedAt: string
}

export interface LoginResponse {
  token: string
  expiresAt: string
  user: User
}
