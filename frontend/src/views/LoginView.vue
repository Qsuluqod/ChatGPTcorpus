<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-100">
    <form @submit.prevent="checkPassphrase" class="bg-white p-6 rounded shadow space-y-4 w-80">
      <h1 class="text-xl font-semibold text-center">Enter Passphrase</h1>
      <input
        v-model="input"
        type="password"
        placeholder="Passphrase"
        class="w-full border px-3 py-2 rounded"
      />
      <p v-if="error" class="text-red-600 text-sm">{{ error }}</p>
      <button type="submit" class="w-full btn-primary">Enter</button>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const input = ref('')
const error = ref('')
const router = useRouter()

const checkPassphrase = () => {
  const required = import.meta.env.VITE_ACCESS_PASSPHRASE
  if (input.value === required) {
    localStorage.setItem('authenticated', 'true')
    router.replace({ name: 'search' })
  } else {
    error.value = 'Invalid passphrase'
  }
}
</script>

<style scoped>
.btn-primary {
  @apply bg-purple-600 hover:bg-purple-700 text-white font-semibold py-2 px-4 rounded;
}
</style>
