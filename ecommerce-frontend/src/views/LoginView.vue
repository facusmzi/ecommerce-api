<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { useCartStore } from '@/stores/cart.store'

const router = useRouter()
const auth = useAuthStore()
const cart = useCartStore()

const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function login() {
  error.value = ''
  loading.value = true
  try {
    await auth.login(email.value, password.value)
    await cart.fetchCart()
    router.push('/')
  } catch (e: any) {
    error.value = e.response?.data?.message ?? 'Credenciales incorrectas'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="max-w-md mx-auto mt-16">
    <div class="bg-white rounded-xl border border-gray-200 p-8">
      <h1 class="text-2xl font-bold text-gray-900 mb-6">Iniciar sesión</h1>

      <div v-if="error" class="bg-red-50 text-red-600 text-sm rounded-lg px-4 py-3 mb-4">
        {{ error }}
      </div>

      <div class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
          <input
            v-model="email"
            type="email"
            class="w-full border border-gray-300 rounded-lg px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="tu@email.com"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Contraseña</label>
          <input
            v-model="password"
            type="password"
            @keyup.enter="login"
            class="w-full border border-gray-300 rounded-lg px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="••••••••"
          />
        </div>

        <button
          @click="login"
          :disabled="loading"
          class="w-full bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
        >
          {{ loading ? 'Ingresando...' : 'Iniciar sesión' }}
        </button>
      </div>

      <p class="text-sm text-gray-500 text-center mt-6">
        ¿No tenés cuenta?
        <RouterLink to="/register" class="text-blue-600 hover:underline">Registrate</RouterLink>
      </p>
    </div>
  </div>
</template>
