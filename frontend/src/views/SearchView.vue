<template>
  <div class="max-w-6xl mx-auto px-4 py-8">
    <div class="space-y-6">
      <div class="flex gap-4">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search conversations..."
          class="input flex-1 bg-white/80 backdrop-blur border-2 border-primary-from focus:border-primary-to shadow-[0_0_8px_rgba(168,85,247,0.08)]"
          @keyup.enter="performSearch"
        />
        <button
          @click="performSearch"
          class="btn-primary whitespace-nowrap"
          :disabled="isLoading"
        >
          {{ isLoading ? 'Searching...' : 'Search' }}
        </button>
      </div>

      <div v-if="error" class="bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-lg">
        {{ error }}
      </div>

      <div v-if="isLoading" class="text-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-from mx-auto"></div>
      </div>

      <div v-else-if="searchResults.length > 0" class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div
          v-for="result in searchResults"
          :key="result.messageId"
          class="card p-6 flex flex-col gap-2 relative group hover:scale-[1.02] transition-transform duration-200"
        >
          <div class="flex items-center gap-3 mb-2">
            <div class="w-10 h-10 rounded-full bg-gradient-to-br from-primary-from to-primary-to flex items-center justify-center text-white font-bold text-lg shadow-md">
              {{ result.author ? result.author.charAt(0).toUpperCase() : '?' }}
            </div>
            <div>
              <h3 class="text-base font-semibold text-gray-900">{{ result.conversationTitle }}</h3>
              <p class="text-xs text-gray-500">{{ result.author }} • {{ formatDate(result.createTime) }}</p>
            </div>
          </div>
          <div class="mt-1 text-gray-700 text-sm whitespace-pre-line break-words">
            <span v-html="highlightQuery(result.content)"></span>
          </div>
        </div>
      </div>

      <div v-else-if="hasSearched" class="text-center py-8 text-gray-500">
        No results found
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { searchConversations } from '../services/searchService'

const searchQuery = ref('')
const searchResults = ref([])
const isLoading = ref(false)
const hasSearched = ref(false)
const error = ref(null)

const performSearch = async () => {
  if (!searchQuery.value.trim()) return

  isLoading.value = true
  hasSearched.value = true
  error.value = null

  const result = await searchConversations(searchQuery.value)
  
  if (result.success) {
    searchResults.value = result.results
  } else {
    error.value = result.error
    searchResults.value = []
  }

  isLoading.value = false
}

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function highlightQuery(text) {
  if (!searchQuery.value) return text
  const escaped = searchQuery.value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
  const regex = new RegExp(`(${escaped})`, 'gi')
  return text.replace(regex, '<mark class="bg-primary-from/20 text-primary-from font-semibold rounded px-1">$1</mark>')
}
</script> 