<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { productService } from '@/services/product.service'
import type { Product, Category, PagedResponse } from '@/types'

const router = useRouter()

const products = ref<Product[]>([])
const categories = ref<Category[]>([])
const loading = ref(false)
const totalPages = ref(1)

const filters = ref({
  search: '',
  categoryId: '',
  page: 1,
  pageSize: 12,
})

async function fetchProducts() {
  loading.value = true
  try {
    const params: Record<string, any> = {
      page: filters.value.page,
      pageSize: filters.value.pageSize,
    }
    if (filters.value.search) params.search = filters.value.search
    if (filters.value.categoryId) params.categoryId = filters.value.categoryId

    const data: PagedResponse<Product> = await productService.getProducts(params)
    products.value = data.items
    totalPages.value = data.totalPages
  } finally {
    loading.value = false
  }
}

async function fetchCategories() {
  categories.value = await productService.getCategories()
}

function onSearchInput() {
  filters.value.page = 1
  fetchProducts()
}

function onCategoryChange() {
  filters.value.page = 1
  fetchProducts()
}

function goToPage(page: number) {
  filters.value.page = page
  fetchProducts()
}

onMounted(() => {
  fetchCategories()
  fetchProducts()
})
</script>

<template>
  <div>
    <!-- Filtros -->
    <div class="flex flex-col sm:flex-row gap-4 mb-8">
      <input
        v-model="filters.search"
        @input="onSearchInput"
        type="text"
        placeholder="Buscar productos..."
        class="flex-1 border border-gray-300 rounded-lg px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
      />
      <select
        v-model="filters.categoryId"
        @change="onCategoryChange"
        class="border border-gray-300 rounded-lg px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
      >
        <option value="">Todas las categorías</option>
        <option v-for="cat in categories" :key="cat.id" :value="cat.id">
          {{ cat.name }}
        </option>
      </select>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="text-center py-16 text-gray-500">Cargando productos...</div>

    <!-- Sin resultados -->
    <div v-else-if="products.length === 0" class="text-center py-16 text-gray-500">
      No se encontraron productos.
    </div>

    <!-- Grid de productos -->
    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      <div
        v-for="product in products"
        :key="product.id"
        @click="router.push(`/products/${product.id}`)"
        class="bg-white rounded-xl border border-gray-200 overflow-hidden cursor-pointer hover:shadow-md transition-shadow"
      >
        <img
          v-if="product.imageUrl"
          :src="product.imageUrl"
          :alt="product.name"
          class="w-full h-48 object-cover"
        />
        <div
          v-else
          class="w-full h-48 bg-gray-100 flex items-center justify-center text-gray-400 text-sm"
        >
          Sin imagen
        </div>

        <div class="p-4">
          <p class="text-xs text-blue-600 font-medium mb-1">{{ product.categoryName }}</p>
          <h3 class="font-semibold text-gray-900 text-sm mb-2 line-clamp-2">{{ product.name }}</h3>
          <div class="flex items-center justify-between">
            <span class="text-lg font-bold text-gray-900">
              ${{ product.price.toLocaleString('es-AR') }}
            </span>
            <span :class="product.stock > 0 ? 'text-green-600' : 'text-red-500'" class="text-xs">
              {{ product.stock > 0 ? `${product.stock} disponibles` : 'Sin stock' }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Paginación -->
    <div v-if="totalPages > 1" class="flex justify-center gap-2 mt-8">
      <button
        v-for="page in totalPages"
        :key="page"
        @click="goToPage(page)"
        :class="[
          'w-9 h-9 rounded-lg text-sm font-medium',
          filters.page === page
            ? 'bg-blue-600 text-white'
            : 'bg-white border border-gray-300 text-gray-700 hover:bg-gray-50',
        ]"
      >
        {{ page }}
      </button>
    </div>
  </div>
</template>
