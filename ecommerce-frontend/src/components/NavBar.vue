<script setup lang="ts">
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { useCartStore } from '@/stores/cart.store'

const auth = useAuthStore()
const cart = useCartStore()
const router = useRouter()

async function logout() {
  await auth.logout()
  router.push('/login')
}
</script>

<template>
  <nav class="bg-white border-b border-gray-200 px-6 py-4">
    <div class="max-w-6xl mx-auto flex items-center justify-between">
      <RouterLink to="/" class="text-xl font-bold text-gray-900"> 🛒 ECommerce </RouterLink>

      <div class="flex items-center gap-6">
        <RouterLink to="/" class="text-gray-600 hover:text-gray-900 text-sm">
          Productos
        </RouterLink>

        <template v-if="auth.isAuthenticated">
          <RouterLink to="/cart" class="relative text-gray-600 hover:text-gray-900 text-sm">
            Carrito
            <span
              v-if="cart.itemCount > 0"
              class="absolute -top-2 -right-3 bg-blue-600 text-white text-xs rounded-full w-4 h-4 flex items-center justify-center"
            >
              {{ cart.itemCount }}
            </span>
          </RouterLink>

          <RouterLink to="/orders" class="text-gray-600 hover:text-gray-900 text-sm">
            Mis órdenes
          </RouterLink>

          <button @click="logout" class="text-sm text-gray-600 hover:text-gray-900">Salir</button>
        </template>

        <template v-else>
          <RouterLink to="/login" class="text-sm text-gray-600 hover:text-gray-900">
            Iniciar sesión
          </RouterLink>
          <RouterLink
            to="/register"
            class="text-sm bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
          >
            Registrarse
          </RouterLink>
        </template>
      </div>
    </div>
  </nav>
</template>
