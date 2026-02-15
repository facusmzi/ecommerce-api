<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { productService } from '@/services/product.service'
import { useCartStore } from '@/stores/cart.store'
import { useAuthStore } from '@/stores/auth.store'
import type { Product } from '@/types'

const route = useRoute()
const router = useRouter()
const cart = useCartStore()
const auth = useAuthStore()

const product = ref<Product | null>(null)
const loading = ref(true)
const adding = ref(false)
const quantity = ref(1)
const success = ref(false)

async function fetchProduct() {
  try {
    product.value = await productService.getProductById(route.params.id as string)
  } finally {
    loading.value = false
  }
}

async function addToCart() {
  if (!auth.isAuthenticated) {
    router.push('/login')
    return
  }
  adding.value = true
  try {
    await cart.addItem(product.value!.id, quantity.value)
    success.value = true
    setTimeout(() => (success.value = false), 2000)
  } finally {
    adding.value = false
  }
}

onMounted(fetchProduct)
</script>

<template>
  <div>
    <button
      @click="router.back()"
      class="text-sm text-gray-500 hover:text-gray-900 mb-6 flex items-center gap-1"
    >
      ← Volver
    </button>

    <div v-if="loading" class="text-center py-16 text-gray-500">Cargando...</div>

    <div v-else-if="!product" class="text-center py-16 text-gray-500">Producto no encontrado.</div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-12">
      <!-- Imagen -->
      <div>
        <img
          v-if="product.imageUrl"
          :src="product.imageUrl"
          :alt="product.name"
          class="w-full rounded-xl object-cover"
        />
        <div
          v-else
          class="w-full h-80 bg-gray-100 rounded-xl flex items-center justify-center text-gray-400"
        >
          Sin imagen
        </div>
      </div>

      <!-- Info -->
      <div class="flex flex-col gap-4">
        <p class="text-sm text-blue-600 font-medium">{{ product.categoryName }}</p>
        <h1 class="text-3xl font-bold text-gray-900">{{ product.name }}</h1>
        <p v-if="product.description" class="text-gray-600 text-sm leading-relaxed">
          {{ product.description }}
        </p>
        <p class="text-3xl font-bold text-gray-900">${{ product.price.toLocaleString('es-AR') }}</p>

        <p
          :class="product.stock > 0 ? 'text-green-600' : 'text-red-500'"
          class="text-sm font-medium"
        >
          {{ product.stock > 0 ? `${product.stock} unidades disponibles` : 'Sin stock' }}
        </p>

        <div v-if="product.stock > 0" class="flex items-center gap-4">
          <div class="flex items-center border border-gray-300 rounded-lg">
            <button
              @click="quantity = Math.max(1, quantity - 1)"
              class="px-3 py-2 text-gray-600 hover:bg-gray-50 rounded-l-lg"
            >
              −
            </button>
            <span class="px-4 py-2 text-sm font-medium">{{ quantity }}</span>
            <button
              @click="quantity = Math.min(product.stock, quantity + 1)"
              class="px-3 py-2 text-gray-600 hover:bg-gray-50 rounded-r-lg"
            >
              +
            </button>
          </div>

          <button
            @click="addToCart"
            :disabled="adding"
            class="flex-1 bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
          >
            {{ adding ? 'Agregando...' : success ? '✓ Agregado' : 'Agregar al carrito' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
