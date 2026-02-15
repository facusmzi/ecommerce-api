<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { orderService } from '@/services/order.service'
import type { Order } from '@/types'

const route = useRoute()
const router = useRouter()
const order = ref<Order | null>(null)
const loading = ref(true)
const creatingPayment = ref(false)

const statusLabel: Record<string, string> = {
  Pending: 'Pendiente de pago',
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

async function fetchOrder() {
  try {
    order.value = await orderService.getOrderById(route.params.id as string)
  } finally {
    loading.value = false
  }
}

async function retryPayment() {
  creatingPayment.value = true
  try {
    const payment = await orderService.createPayment(order.value!.id)
    window.location.href = payment.initPoint
  } catch (e: any) {
    alert(e.response?.data?.message ?? 'Error al procesar el pago')
  } finally {
    creatingPayment.value = false
  }
}

onMounted(fetchOrder)
</script>

<template>
  <div class="max-w-2xl mx-auto">
    <button
      @click="router.push('/orders')"
      class="text-sm text-gray-500 hover:text-gray-900 mb-6 flex items-center gap-1"
    >
      ← Mis órdenes
    </button>

    <div v-if="loading" class="text-center py-16 text-gray-500">Cargando...</div>

    <div v-else-if="!order" class="text-center py-16 text-gray-500">Orden no encontrada.</div>

    <div v-else class="space-y-6">
      <!-- Header -->
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <div class="flex items-center justify-between mb-4">
          <div>
            <p class="text-xs text-gray-400 mb-1">Número de orden</p>
            <p class="font-mono font-medium text-gray-900">
              {{ order.id.slice(0, 8).toUpperCase() }}
            </p>
          </div>
          <span
            :class="statusColor[order.status]"
            class="text-sm font-medium px-3 py-1 rounded-full"
          >
            {{ statusLabel[order.status] ?? order.status }}
          </span>
        </div>

        <div class="grid grid-cols-2 gap-4 text-sm">
          <div>
            <p class="text-gray-400 mb-1">Fecha</p>
            <p class="text-gray-900">{{ new Date(order.createdAt).toLocaleDateString('es-AR') }}</p>
          </div>
          <div>
            <p class="text-gray-400 mb-1">Estado del pago</p>
            <p class="text-gray-900">{{ order.paymentStatus ?? 'Sin pago iniciado' }}</p>
          </div>
        </div>
      </div>

      <!-- Items -->
      <div class="bg-white rounded-xl border border-gray-200 divide-y divide-gray-100">
        <div
          v-for="item in order.items"
          :key="item.id"
          class="flex items-center justify-between p-4"
        >
          <div>
            <p class="font-medium text-gray-900 text-sm">{{ item.productName }}</p>
            <p class="text-gray-500 text-xs">
              {{ item.quantity }} x ${{ item.unitPrice.toLocaleString('es-AR') }}
            </p>
          </div>
          <p class="font-semibold text-gray-900 text-sm">
            ${{ item.subtotal.toLocaleString('es-AR') }}
          </p>
        </div>
      </div>

      <!-- Total -->
      <div class="bg-white rounded-xl border border-gray-200 p-6">
        <div class="flex justify-between items-center">
          <span class="text-gray-600 font-medium">Total</span>
          <span class="text-2xl font-bold text-gray-900"
            >${{ order.total.toLocaleString('es-AR') }}</span
          >
        </div>

        <button
          v-if="order.status === 'Pending'"
          @click="retryPayment"
          :disabled="creatingPayment"
          class="w-full mt-4 bg-blue-600 text-white rounded-lg py-3 font-medium hover:bg-blue-700 disabled:opacity-50"
        >
          {{ creatingPayment ? 'Procesando...' : 'Pagar con MercadoPago' }}
        </button>
      </div>
    </div>
  </div>
</template>
