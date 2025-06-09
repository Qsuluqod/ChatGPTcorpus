<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-from to-primary-to p-4">
    <form
      @submit.prevent="checkPassphrase"
      class="bg-white/90 backdrop-blur-xl border-2 border-primary-from rounded-xl shadow-xl p-8 space-y-6 w-full max-w-md"
    >
      <h1 class="text-2xl font-bold text-center text-gray-900">Enter Passphrase</h1>
      <p class="text-center text-gray-700 text-sm">
        This project is temporarily open only to contributors. If you have contributed,
        please contact the author to obtain a valid passphrase.
      </p>
      <input
        v-model="input"
        type="password"
        placeholder="Passphrase"
        class="input bg-white"
      />
      <p v-if="error" class="text-red-600 text-sm text-center">{{ error }}</p>
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
