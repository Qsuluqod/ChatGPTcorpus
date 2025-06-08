<template>
  <div class="max-w-6xl mx-auto px-4 py-8">
    <div class="space-y-6">
      <div class="flex gap-4 w-[800px] mx-auto">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search conversations..."
          class="input w-[700px] bg-white/80 backdrop-blur border-2 border-primary-from focus:border-primary-to shadow-[0_0_8px_rgba(168,85,247,0.08)]"
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

      <!-- Filter checkboxes -->
      <div class="flex gap-8 items-center w-[800px] mx-auto mb-2" style="color-scheme: light;">
        <label class="flex items-center gap-1 cursor-pointer text-sm">
          <input type="checkbox" v-model="showAiMessages" class="accent-primary-from bg-white border border-gray-300 w-3 h-3" style="background-color: #fff;" />
          <span>AI messages</span>
        </label>
        <label class="flex items-center gap-1 cursor-pointer text-sm">
          <input type="checkbox" v-model="showUserMessages" class="accent-primary-from bg-white border border-gray-300 w-3 h-3" style="background-color: #fff;" />
          <span>User messages</span>
        </label>
        <label class="flex items-center gap-1 cursor-pointer text-sm">
          <input type="checkbox" v-model="showTableView" class="accent-primary-from bg-white border border-gray-300 w-3 h-3" style="background-color: #fff;" />
          <span>Table view</span>
        </label>
      </div>

      <div v-if="error" class="bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-lg">
        {{ error }}
      </div>

      <div v-if="searchStats" class="bg-white/80 backdrop-blur border-2 border-primary-from rounded-lg p-4 mb-6 w-[800px] mx-auto">
        <div class="flex flex-wrap justify-between">
          <div class="text-center">
            <div class="text-2xl font-bold text-primary-from">{{ searchStats.totalMatches }}</div>
            <div class="text-sm text-gray-600">Total Matches</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-primary-from">{{ searchStats.agentMatches }}</div>
            <div class="text-sm text-gray-600">Agent Messages</div>
            <div class="text-xs text-gray-500">{{ Math.round((searchStats.agentMatches / searchStats.totalMatches) * 100) }}% of matches</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-primary-from">{{ searchStats.userMatches }}</div>
            <div class="text-sm text-gray-600">User Messages</div>
            <div class="text-xs text-gray-500">{{ Math.round((searchStats.userMatches / searchStats.totalMatches) * 100) }}% of matches</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-primary-from">{{ searchStats.uniqueMessages }}</div>
            <div class="text-sm text-gray-600">Unique Messages</div>
            <div class="text-xs text-gray-500">{{ Math.round((searchStats.uniqueMessages / searchStats.totalMessagesInCorpus) * 100) }}% of all messages</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-primary-from">{{ searchStats.wordOccurrences }}</div>
            <div class="text-sm text-gray-600">Word Occurrences</div>
            <div class="text-xs text-gray-500">{{ (searchStats.wordOccurrences / searchStats.uniqueMessages).toFixed(1) }} per message</div>
          </div>
        </div>
      </div>

      <div v-if="isLoading" class="text-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-from mx-auto"></div>
      </div>

      <div v-else-if="filteredResults.length > 0">
        <!-- Table View -->
        <div v-if="showTableView" class="w-full max-w-[1200px] mx-auto">
          <table class="w-full border-collapse">
            <thead>
              <tr class="bg-white/80 backdrop-blur border-b-2 border-primary-from">
                <th 
                  v-for="(column, index) in columns" 
                  :key="index"
                  class="px-4 py-3 text-left text-sm font-semibold text-gray-600 cursor-pointer hover:text-primary-from"
                  @click="sortBy(column.key)"
                >
                  <div class="flex items-center gap-1">
                    {{ column.label }}
                    <span v-if="sortKey === column.key" class="text-primary-from">
                      {{ sortOrder === 'asc' ? '↑' : '↓' }}
                    </span>
                  </div>
                </th>
              </tr>
            </thead>
            <tbody>
              <tr 
                v-for="(result, index) in sortedResults" 
                :key="`${result.messageId}-${index}`"
                class="border-b border-gray-200 hover:bg-gray-50 transition-colors cursor-pointer"
                :class="{ 'bg-gray-50/50': index % 2 === 0 }"
                @click="navigateToConversation(result)"
              >
                <td class="px-4 py-3 text-sm text-gray-600">{{ result.author }}</td>
                <td class="px-4 py-3 text-sm text-gray-600">{{ formatDate(result.createTime) }}</td>
                <td class="px-4 py-3 text-sm text-gray-600">{{ result.conversationTitle }}</td>
                <td class="px-4 py-3 text-sm text-gray-700">
                  <span v-html="getContextText(result.content)"></span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <!-- Grid View -->
        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div
            v-for="result in filteredResults"
            :key="result.messageId"
            class="card p-6 flex flex-col gap-2 relative group hover:scale-[1.02] transition-transform duration-200 cursor-pointer"
            @click="navigateToConversation(result)"
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
            <div class="mt-1 text-gray-700 text-sm whitespace-pre-line break-words text-left">
              <span v-html="highlightQuery(result.content)"></span>
            </div>
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
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { searchConversations } from '../services/searchService'

const router = useRouter()
const searchQuery = ref('')
const searchResults = ref([])
const searchStats = ref(null)
const isLoading = ref(false)
const hasSearched = ref(false)
const error = ref(null)

// Filter checkboxes
const showAiMessages = ref(true)
const showUserMessages = ref(true)
const showTableView = ref(false)

// Table sorting
const sortKey = ref('createTime')
const sortOrder = ref('desc')

const columns = [
  { key: 'author', label: 'Author' },
  { key: 'createTime', label: 'Date & Time' },
  { key: 'conversationTitle', label: 'Chat Name' },
  { key: 'content', label: 'Context' }
]

const sortBy = (key) => {
  if (sortKey.value === key) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortKey.value = key
    sortOrder.value = 'asc'
  }
}

const filteredResults = computed(() => {
  return searchResults.value.filter(result => {
    if (showAiMessages.value && showUserMessages.value) return true
    if (showAiMessages.value && result.author?.toLowerCase() === 'assistant') return true
    if (showUserMessages.value && result.author?.toLowerCase() === 'user') return true
    return false
  })
})

const sortedResults = computed(() => {
  const results = filteredResults.value.flatMap(result => {
    const matches = result.content.match(new RegExp(searchQuery.value, 'gi'))
    if (!matches) return [result]
    return matches.map(() => ({ ...result }))
  })

  return results.sort((a, b) => {
    const aValue = a[sortKey.value]
    const bValue = b[sortKey.value]
    
    if (sortOrder.value === 'asc') {
      return aValue > bValue ? 1 : -1
    } else {
      return aValue < bValue ? 1 : -1
    }
  })
})

const performSearch = async () => {
  if (!searchQuery.value.trim()) return

  isLoading.value = true
  hasSearched.value = true
  error.value = null

  const result = await searchConversations(searchQuery.value)
  
  if (result.success) {
    searchResults.value = result.results
    searchStats.value = result.stats
  } else {
    error.value = result.error
    searchResults.value = []
    searchStats.value = null
  }

  isLoading.value = false
  
  // Scroll to top of the page
  window.scrollTo({ top: 0, behavior: 'smooth' })
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

function getContextText(text) {
  if (!searchQuery.value) return text
  
  const escaped = searchQuery.value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
  const regex = new RegExp(`(${escaped})`, 'gi')
  const match = text.match(regex)
  
  if (!match) return text
  
  const words = text.split(/\s+/)
  const matchIndex = words.findIndex(word => 
    word.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
  
  if (matchIndex === -1) return text
  
  const start = Math.max(0, matchIndex - 10)
  const end = Math.min(words.length, matchIndex + 11)
  const context = words.slice(start, end).join(' ')
  
  return context.replace(regex, '<mark class="bg-primary-from/20 text-primary-from font-semibold rounded px-1">$1</mark>')
}

const navigateToConversation = (result) => {
  router.push({
    name: 'conversation',
    params: {
      conversationId: result.conversationId,
      messageId: result.messageId
    },
    query: {
      term: searchQuery.value
    }
  })
}
</script>

<style scoped>
input[type="checkbox"] {
  width: 1.1em;
  height: 1.1em;
  border-radius: 0.25em;
  border: 2px solid #a855f7;
  background: #fff;
  transition: background 0.2s, box-shadow 0.2s;
}
input[type="checkbox"]:checked {
  background: theme('colors.primary.from');
  box-shadow: 0 0 0 2px #a855f7;
}
</style> 