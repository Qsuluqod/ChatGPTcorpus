<template>
  <div class="max-w-6xl mx-auto px-4 py-8">
    <div class="space-y-6">
      <!-- Header with back button -->
      <div class="fixed top-24 left-6 z-30 flex items-center gap-4" style="min-width: 260px;">
        <button
          @click="goBack"
          class="btn-primary flex items-center gap-2"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M9.707 16.707a1 1 0 01-1.414 0l-6-6a1 1 0 010-1.414l6-6a1 1 0 011.414 1.414L5.414 9H17a1 1 0 110 2H5.414l4.293 4.293a1 1 0 010 1.414z" clip-rule="evenodd" />
          </svg>
          <span>Back to Search Results</span>
        </button>
        <span class="sr-only">Conversation title: {{ conversation.title }}</span>
      </div>

      <!-- Messages -->
      <div class="space-y-4">
        <div
          v-for="message in conversation.messages"
          :key="message.id"
          class="flex"
          :class="{
            'justify-end': isUser(message),
            'justify-start': !isUser(message)
          }"
        >
          <div
            class="max-w-[70%] card p-6 flex flex-col gap-2 relative group"
            :class="[
              isUser(message)
                ? 'bg-primary-from/10 text-primary-from rounded-br-2xl rounded-tl-2xl rounded-bl-2xl ml-8'
                : 'bg-white/80 text-gray-800 rounded-bl-2xl rounded-tr-2xl rounded-br-2xl mr-8',
              message.id === highlightedMessageId ? 'ring-2 ring-primary-from' : '',
              'shadow-sm'
            ]"
          >
            <div class="flex items-center gap-3 mb-2">
              <div class="w-8 h-8 rounded-full bg-gradient-to-br from-primary-from to-primary-to flex items-center justify-center text-white font-bold text-base shadow-md">
                {{ message.author ? message.author.charAt(0).toUpperCase() : '?' }}
              </div>
              <div>
                <p class="text-xs text-gray-500">{{ message.author }} • {{ formatDate(message.createTime) }}</p>
              </div>
            </div>
            <div
              class="mt-1 text-gray-700 text-sm whitespace-pre-line break-words text-left"
            >
              <span v-html="highlightSearchTerm(message.content)"></span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { getConversation } from '../services/conversationService'

const router = useRouter()
const route = useRoute()

const conversation = ref({
  title: '',
  messages: []
})
const highlightedMessageId = ref(null)

onMounted(async () => {
  const conversationId = route.params.conversationId
  const messageId = route.params.messageId
  
  try {
    const result = await getConversation(conversationId)
    if (result.success) {
      conversation.value = result.conversation
      highlightedMessageId.value = messageId
      
      // Scroll to the highlighted message
      setTimeout(() => {
        const element = document.querySelector(`[data-message-id="${messageId}"]`)
        if (element) {
          element.scrollIntoView({ behavior: 'smooth', block: 'center' })
        }
      }, 100)
    }
  } catch (error) {
    console.error('Failed to load conversation:', error)
  }
})

const goBack = () => {
  router.back()
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

const highlightSearchTerm = (text) => {
  const searchTerm = route.query.term
  if (!searchTerm) return text
  
  const escaped = searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
  const regex = new RegExp(`(${escaped})`, 'gi')
  return text.replace(regex, '<mark class="bg-primary-from/20 text-primary-from font-semibold rounded px-1">$1</mark>')
}

// Helper to check if a message is from the user
const isUser = (message) => message.author && message.author.toLowerCase() === 'user'
</script> 