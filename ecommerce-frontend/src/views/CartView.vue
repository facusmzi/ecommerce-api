<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCartStore } from '@/stores/cart.store'
import { orderService } from '@/services/order.service'
import { ref } from 'vue'

const cart = useCartStore()
const router = useRouter()
const checkingOut = ref(false)

async function checkout() {
  checkingOut.value = true
  try {
    const order = await orderService.createOrder()
    const payment = await orderService.createPayment(order.id)
    window.location.href = payment.initPoint
  } catch (e: any) {
    alert(e.response?.data?.message ?? 'Error al procesar el pedido')
  } finally {
    checkingOut.value = false
  }
}

onMounted(() => cart.fetchCart())
</script>

<template>
  <div class="max-w-2xl mx-auto">
    <h1 class="text-2xl font-bold text-gray-900 mb-8">Tu carrito</h1>

    <div v-if="cart.loading" class="text-center py-16 text-gray-500">Cargando...</div>

    <div v-else-if="cart.items.length === 0" class="text-center py-16">
      <p class="text-gray-500 mb-4">Tu carrito está vacío.</p>
      <RouterLink to="/" class="text-blue-600 hover:underline text-sm">Ver productos</RouterLink>
    </div>

    <div v-else>
      <!-- Items -->
      <div class="bg-white rounded-xl border border-gray-200 divide-y divide-gray-100 mb-6">
        <div v-for="item in cart.items" :key="item.id" class="flex items-center gap-4 p-4">
          <div class="flex-1">
            <p class="font-medium text-gray-900 text-sm">{{ item.productName }}</p>
            <p class="text-gray-500 text-sm">${{ item.unitPrice.toLocaleString('es-AR') }} c/u</p>
          </div>

          <div class="flex items-center border border-gray-300 rounded-lg">
            <button
              @click="cart.updateItem(item.id, item.quantity - 1)"
              :disabled="item.quantity <= 1"
              class="px-3 py-1 text-gray-600 hover:bg-gray-50 rounded-l-lg disabled:opacity-30"
            >
              −
            </button>
            <span class="px-3 py-1 text-sm">{{ item.quantity }}</span>
            <button
              @click="cart.updateItem(item.id, item.quantity + 1)"
              class="px-3 py-1 text-gray-600 hover:bg-gray-50 rounded-r-lg"
            >
              +
            </button>
          </div>

          <p class="font-semibold text-gray-900 text-sm w-24 text-right">
            ${{ item.subtotal.toLocaleString('es-AR') }}
          </p>

          <button @click="cart.removeItem(item.id)" class="text-red-400 hover:text-red-600 text-sm">
            ✕
          </button>
        </div>
      </div>

      <!-- Total y checkout -->
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <div class="flex justify-between items-center mb-4">
          <span class="text-gray-600">Total</span>
          <span class="text-2xl font-bold text-gray-900"
            >${{ cart.total.toLocaleString('es-AR') }}</span
          >
        </div>
        <button
          @click="checkout"
          :disabled="checkingOut"
          class="w-full bg-blue-600 text-white rounded-lg py-3 font-medium hover:bg-blue-700 disabled:opacity-50"
        >
          {{ checkingOut ? 'Procesando...' : 'Pagar con MercadoPago' }}
        </button>
      </div>
    </div>
  </div>
</template>
