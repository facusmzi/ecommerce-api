import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { cartService } from '@/services/cart.service'
import type { Cart } from '@/types'

export const useCartStore = defineStore('cart', () => {
  const cart = ref<Cart | null>(null)
  const loading = ref(false)

  const itemCount = computed(() => cart.value?.itemCount ?? 0)
  const total = computed(() => cart.value?.total ?? 0)
  const items = computed(() => cart.value?.items ?? [])

  async function fetchCart() {
    loading.value = true
    try {
      cart.value = await cartService.getCart()
    } finally {
      loading.value = false
    }
  }

  async function addItem(productId: string, quantity: number) {
    await cartService.addItem(productId, quantity)
    await fetchCart()
  }

  async function updateItem(itemId: string, quantity: number) {
    await cartService.updateItem(itemId, quantity)
    await fetchCart()
  }

  async function removeItem(itemId: string) {
    await cartService.removeItem(itemId)
    await fetchCart()
  }

  function clearCart() {
    cart.value = null
  }

  return {
    cart,
    loading,
    itemCount,
    total,
    items,
    fetchCart,
    addItem,
    updateItem,
    removeItem,
    clearCart,
  }
})
