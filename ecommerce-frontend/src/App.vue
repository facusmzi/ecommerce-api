<script setup lang="ts">
import { RouterView } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { useCartStore } from '@/stores/cart.store'
import { watch } from 'vue'
import NavBar from '@/components/NavBar.vue'

const auth = useAuthStore()
const cart = useCartStore()

// Cuando el usuario se loguea, cargamos el carrito automáticamente
watch(
  () => auth.isAuthenticated,
  (isAuth) => {
    if (isAuth) cart.fetchCart()
  },
  { immediate: true },
)
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <NavBar />
    <main class="max-w-6xl mx-auto px-6 py-8">
      <RouterView />
    </main>
  </div>
</template>
