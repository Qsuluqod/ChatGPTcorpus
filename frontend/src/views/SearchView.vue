<template>
  <div class="max-w-6xl mx-auto px-4 py-8 flex-1 w-full flex flex-col">
    <div class="space-y-6 flex-1 flex flex-col">
      <div class="flex gap-4 w-[800px] mx-auto">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search conversations..."
          class="input flex-1 bg-white/80 backdrop-blur border-2 border-primary-from focus:border-primary-to shadow-[0_0_8px_rgba(168,85,247,0.08)]"
          @keyup.enter="performSearch()"
        />
        <div class="flex gap-4">
          <button
            @click="performSearch()"
            class="btn-primary whitespace-nowrap"
            :disabled="isLoading"
          >
            {{ isLoading ? 'Searching...' : 'Search' }}
          </button>
          <button
            @click="performSearchAll"
            class="btn-primary whitespace-nowrap"
            :disabled="isLoading"
          >
            {{ isLoading ? 'Retrieving...' : 'Browse All' }}
          </button>
        </div>
      </div>

      <div class="flex gap-4 w-[800px] mx-auto mt-2">
        <input
          v-model="sequenceFrom"
          type="number"
          min="1"
          placeholder="From message #"
          class="input w-32 bg-white/80 backdrop-blur border-2 border-primary-from focus:border-primary-to shadow-[0_0_8px_rgba(168,85,247,0.08)]"
          @keyup.enter="performSearch()"
        />
        <input
          v-model="sequenceTo"
          type="number"
          min="1"
          placeholder="To message #"
          class="input w-32 bg-white/80 backdrop-blur border-2 border-primary-from focus:border-primary-to shadow-[0_0_8px_rgba(168,85,247,0.08)]"
          @keyup.enter="performSearch()"
        />
        <input
          v-model="maxPerUpload"
          type="number"
          min="1"
          placeholder="Max per upload"
          class="input w-36 bg-white/80 backdrop-blur border-2 border-primary-from focus:border-primary-to shadow-[0_0_8px_rgba(168,85,247,0.08)]"
          @keyup.enter="performSearch()"
        />
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
          <input type="checkbox" v-model="showOtherMessages" class="accent-primary-from bg-white border border-gray-300 w-3 h-3" style="background-color: #fff;" />
          <span>Other messages</span>
        </label>
        <label class="flex items-center gap-1 cursor-pointer text-sm">
          <input type="checkbox" v-model="showTableView" class="accent-primary-from bg-white border border-gray-300 w-3 h-3" style="background-color: #fff;" />
          <span>Table view</span>
        </label>
      </div>

      <div v-if="error" class="bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-lg">
        {{ error }}
      </div>

      <div v-if="searchStats" class="bg-white/80 backdrop-blur border-2 border-primary-from rounded-lg p-4 mb-3 w-[800px] mx-auto">
        <div class="flex flex-wrap justify-between">
          <div class="text-center">
            <div class="text-2xl font-bold text-primary-from">{{ searchStats.totalMatches }}</div>
            <div class="text-sm text-gray-600">Total Matches</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-primary-from">{{ searchStats.agentMatches }}</div>
            <div class="text-sm text-gray-600">Agent Messages</div>
            <div class="text-xs text-gray-500">{{ percentageOfTotal(searchStats.agentMatches) }}% of matches</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-primary-from">{{ searchStats.userMatches }}</div>
            <div class="text-sm text-gray-600">User Messages</div>
            <div class="text-xs text-gray-500">{{ percentageOfTotal(searchStats.userMatches) }}% of matches</div>
          </div>
          <div class="text-center">
            <div class="text-2xl font-bold text-primary-from">{{ searchStats.otherMatches }}</div>
            <div class="text-sm text-gray-600">Other Messages</div>
            <div class="text-xs text-gray-500">{{ percentageOfTotal(searchStats.otherMatches) }}% of matches</div>
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

      <div v-if="filteredResults.length" class="w-[800px] mx-auto -mt-2 flex justify-end">
        <button
          @click="exportCsv"
          class="btn-primary"
          :disabled="isLoading"
        >
          Export CSV
        </button>
      </div>

      <div v-if="isLoading" class="flex-1 flex items-center justify-center text-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-from mx-auto"></div>
      </div>

      <div v-else-if="filteredResults.length > 0" class="flex-1 flex flex-col">
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
                <td class="px-4 py-3 text-sm text-gray-600">{{ result.sequence }}</td>
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
                <p class="text-xs text-primary-from font-semibold">Turn {{ result.sequence }}</p>
              </div>
            </div>
            <div class="mt-1 text-gray-700 text-sm whitespace-pre-line break-words text-left">
              <span v-html="highlightQuery(result.content)"></span>
            </div>
          </div>
        </div>
      </div>

      <div v-else-if="hasSearched" class="flex-1 flex items-center justify-center text-center py-8 text-gray-500">
        No results found
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { searchConversations } from '../services/searchService'

const SEARCH_STATE_KEY = 'corpus_search_state'

const router = useRouter()
const searchQuery = ref('')
const sequenceFrom = ref('')
const sequenceTo = ref('')
const maxPerUpload = ref('')
const searchResults = ref([])
const searchStats = ref(null)
const isLoading = ref(false)
const hasSearched = ref(false)
const error = ref(null)

// Filter checkboxes
const showAiMessages = ref(true)
const showUserMessages = ref(true)
const showOtherMessages = ref(false)
const showTableView = ref(false)

// Table sorting
const sortKey = ref('createTime')
const sortOrder = ref('desc')

const columns = [
  { key: 'author', label: 'Author' },
  { key: 'sequence', label: 'Turn #' },
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
    const role = (result.author || '').toLowerCase()
    const isAssistant = role === 'assistant'
    const isUser = role === 'user'
    const isOther = !isAssistant && !isUser

    const assistantAllowed = showAiMessages.value && isAssistant
    const userAllowed = showUserMessages.value && isUser
    const otherAllowed = showOtherMessages.value && isOther

    if (!showAiMessages.value && !showUserMessages.value && !showOtherMessages.value) {
      return false
    }

    return assistantAllowed || userAllowed || otherAllowed
  })
})

const sortedResults = computed(() => {
  const query = searchQuery.value

  const results = !query
    ? [...filteredResults.value]
    : (() => {
        const escaped = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
        const regex = new RegExp(escaped, 'gi')
        return filteredResults.value.flatMap(result => {
          const matches = result.content.match(regex)
          if (!matches) return [result]
          return matches.map(() => ({ ...result }))
        })
      })()

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

const percentageOfTotal = (count) => {
  const total = searchStats.value?.totalMatches || 0
  if (!total || typeof count !== 'number') {
    return 0
  }
  return Math.round((count / total) * 100)
}

const saveSearchState = () => {
  const state = {
    searchQuery: searchQuery.value,
    sequenceFrom: sequenceFrom.value,
    sequenceTo: sequenceTo.value,
    maxPerUpload: maxPerUpload.value,
    showAiMessages: showAiMessages.value,
    showUserMessages: showUserMessages.value,
    showOtherMessages: showOtherMessages.value,
    showTableView: showTableView.value,
    searchResults: searchResults.value,
    searchStats: searchStats.value,
    hasSearched: hasSearched.value
  }

  sessionStorage.setItem(SEARCH_STATE_KEY, JSON.stringify(state))
}

const restoreSearchState = () => {
  const saved = sessionStorage.getItem(SEARCH_STATE_KEY)
  if (!saved) return

  try {
    const state = JSON.parse(saved)
    searchQuery.value = state.searchQuery ?? ''
    sequenceFrom.value = state.sequenceFrom ?? ''
    sequenceTo.value = state.sequenceTo ?? ''
    maxPerUpload.value = state.maxPerUpload ?? ''
    showAiMessages.value = state.showAiMessages ?? true
    showUserMessages.value = state.showUserMessages ?? true
    showOtherMessages.value = state.showOtherMessages ?? false
    showTableView.value = state.showTableView ?? false
    searchResults.value = Array.isArray(state.searchResults) ? state.searchResults : []
    searchStats.value = state.searchStats ?? null
    hasSearched.value = state.hasSearched ?? false
  } catch (error) {
    console.error('Failed to restore search state:', error)
  }
}

const performSearch = async (searchAll = false) => {
  if (!searchAll && !searchQuery.value.trim()) return

  error.value = null

  const options = {}

  if (searchAll) {
    searchQuery.value = ''
    options.includeAll = true
  }

  const parseSequence = (value) => {
    if (value === '' || value === null || value === undefined) {
      return null
    }
    const parsed = Number(value)
    return Number.isFinite(parsed) ? parsed : NaN
  }

  const fromValue = parseSequence(sequenceFrom.value)
  const toValue = parseSequence(sequenceTo.value)

  if ((fromValue !== null && (Number.isNaN(fromValue) || fromValue <= 0 || !Number.isInteger(fromValue))) ||
      (toValue !== null && (Number.isNaN(toValue) || toValue <= 0 || !Number.isInteger(toValue)))) {
    error.value = 'Message numbers must be positive integers.'
    return
  }

  if (fromValue !== null && toValue !== null && fromValue > toValue) {
    error.value = 'The "from" message number cannot be greater than the "to" message number.'
    return
  }

  if (fromValue !== null) {
    options.messageSequenceMin = fromValue
  }

  if (toValue !== null) {
    options.messageSequenceMax = toValue
  }

  const maxPerUploadValue = parseSequence(maxPerUpload.value)
  if (maxPerUpload.value !== '' && (maxPerUploadValue === null || Number.isNaN(maxPerUploadValue) || maxPerUploadValue <= 0 || !Number.isInteger(maxPerUploadValue))) {
    error.value = 'Max per upload must be a positive integer.'
    return
  }

  if (maxPerUploadValue !== null) {
    options.maxPerImportBatch = maxPerUploadValue
  }

  isLoading.value = true
  hasSearched.value = true

  const result = await searchConversations(searchQuery.value, options)
  
  if (result.success) {
    searchResults.value = result.results
    searchStats.value = result.stats
    saveSearchState()
  } else {
    error.value = result.error
    searchResults.value = []
    searchStats.value = null
  }

  isLoading.value = false
  
  // Scroll to top of the page
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const performSearchAll = () => {
  performSearch(true)
}

const exportCsv = () => {
  if (!hasSearched.value || !filteredResults.value.length) {
    error.value = 'No results to export. Run a search first.'
    return
  }

  const headers = ['Author', 'Turn #', 'Date & Time', 'Chat Name', 'Message']
  const escapeCsv = (value) => {
    const stringValue = value === null || value === undefined ? '' : String(value)
    return /[",\n]/.test(stringValue)
      ? `"${stringValue.replace(/"/g, '""')}"`
      : stringValue
  }

  const rows = filteredResults.value.map(result => [
    result.author || '',
    result.sequence ?? '',
    formatDate(result.createTime),
    result.conversationTitle || '',
    result.content || ''
  ])

  const csvContent = [headers, ...rows]
    .map(row => row.map(escapeCsv).join(','))
    .join('\n')

  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.setAttribute(
    'download',
    `search-results-${new Date().toISOString().slice(0, 19).replace(/[-:T]/g, '')}.csv`
  )
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

function formatDate(dateString) {
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
  saveSearchState()
  router.push({
    name: 'conversation',
    params: {
      conversationId: result.conversationId,
      messageId: result.messageId
    },
    query: {
      term: searchQuery.value,
      sequence: result.sequence
    }
  })
}

onMounted(() => {
  restoreSearchState()
})
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

.btn-secondary {
  @apply bg-white text-primary-from border-2 border-primary-from font-semibold py-2 px-4 rounded transition-colors duration-150;
}

.btn-secondary:hover {
  @apply bg-primary-from text-white;
}
</style> 
