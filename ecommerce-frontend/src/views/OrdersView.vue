<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { orderService } from '@/services/order.service'
import type { Order } from '@/types'

const router = useRouter()
const orders = ref<Order[]>([])
const loading = ref(true)

const statusLabel: Record<string, string> = {
  Pending: 'Pendiente',
  Paid: 'Pagado',
  Shipped: 'Enviado',
  Delivered: 'Entregado',
  Cancelled: 'Cancelado',
}

const statusColor: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-700',
  Paid: 'bg-green-100 text-green-700',
  Shipped: 'bg-blue-100 text-blue-700',
  Delivered: 'bg-gray-100 text-gray-700',
  Cancelled: 'bg-red-100 text-red-600',
}

onMounted(async () => {
  try {
    orders.value = await orderService.getOrders()
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold text-gray-900 mb-8">Mis órdenes</h1>

    <div v-if="loading" class="text-center py-16 text-gray-500">Cargando...</div>

    <div v-else-if="orders.length === 0" class="text-center py-16">
      <p class="text-gray-500 mb-4">Todavía no hiciste ningún pedido.</p>
      <RouterLink to="/" class="text-blue-600 hover:underline text-sm">Ver productos</RouterLink>
    </div>

    <div v-else class="space-y-4">
      <div
        v-for="order in orders"
        :key="order.id"
        @click="router.push(`/orders/${order.id}`)"
        class="bg-white rounded-xl border border-gray-200 p-6 cursor-pointer hover:shadow-md transition-shadow"
      >
        <div class="flex items-center justify-between mb-3">
          <p class="text-sm text-gray-500 font-mono">{{ order.id.slice(0, 8).toUpperCase() }}</p>
          <span
            :class="statusColor[order.status]"
            class="text-xs font-medium px-2 py-1 rounded-full"
          >
            {{ statusLabel[order.status] ?? order.status }}
          </span>
        </div>
        <div class="flex items-center justify-between">
          <p class="text-sm text-gray-600">
            {{ new Date(order.createdAt).toLocaleDateString('es-AR') }}
          </p>
          <p class="font-bold text-gray-900">${{ order.total.toLocaleString('es-AR') }}</p>
        </div>
      </div>
    </div>
  </div>
</template>
