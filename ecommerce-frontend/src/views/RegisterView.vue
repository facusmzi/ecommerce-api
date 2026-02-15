<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authService } from '@/services/auth.service'

const router = useRouter()

const step = ref<'register' | 'verify'>('register')
const email = ref('')
const password = ref('')
const code = ref('')
const error = ref('')
const loading = ref(false)

async function register() {
  error.value = ''
  loading.value = true
  try {
    await authService.register(email.value, password.value)
    step.value = 'verify'
  } catch (e: any) {
    error.value = e.response?.data?.message ?? 'Error al registrarse'
  } finally {
    loading.value = false
  }
}

async function verify() {
  error.value = ''
  loading.value = true
  try {
    await authService.verifyEmail(email.value, code.value)
    router.push('/login')
  } catch (e: any) {
    error.value = e.response?.data?.message ?? 'Código incorrecto'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="max-w-md mx-auto mt-16">
    <div class="bg-white rounded-xl border border-gray-200 p-8">
      <!-- Paso 1: Registro -->
      <template v-if="step === 'register'">
        <h1 class="text-2xl font-bold text-gray-900 mb-6">Crear cuenta</h1>

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
              @keyup.enter="register"
              class="w-full border border-gray-300 rounded-lg px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Mínimo 8 caracteres"
            />
          </div>

          <button
            @click="register"
            :disabled="loading"
            class="w-full bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
          >
            {{ loading ? 'Creando cuenta...' : 'Crear cuenta' }}
          </button>
        </div>

        <p class="text-sm text-gray-500 text-center mt-6">
          ¿Ya tenés cuenta?
          <RouterLink to="/login" class="text-blue-600 hover:underline">Iniciá sesión</RouterLink>
        </p>
      </template>

      <!-- Paso 2: Verificación de email -->
      <template v-else>
        <h1 class="text-2xl font-bold text-gray-900 mb-2">Verificá tu email</h1>
        <p class="text-sm text-gray-500 mb-6">
          Te enviamos un código a <strong>{{ email }}</strong
          >. En desarrollo podés usar <strong>123456</strong>.
        </p>

        <div v-if="error" class="bg-red-50 text-red-600 text-sm rounded-lg px-4 py-3 mb-4">
          {{ error }}
        </div>

        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1"
              >Código de verificación</label
            >
            <input
              v-model="code"
              type="text"
              maxlength="6"
              @keyup.enter="verify"
              class="w-full border border-gray-300 rounded-lg px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 text-center tracking-widest text-lg"
              placeholder="123456"
            />
          </div>

          <button
            @click="verify"
            :disabled="loading"
            class="w-full bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
          >
            {{ loading ? 'Verificando...' : 'Verificar email' }}
          </button>
        </div>
      </template>
    </div>
  </div>
</template>
