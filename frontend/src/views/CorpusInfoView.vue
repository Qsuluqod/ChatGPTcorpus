<template>
  <div class="max-w-2xl mx-auto px-4 py-12 flex-1 flex flex-col justify-center w-full">
    <div class="space-y-8">
      <div class="text-center">
        <h1 class="text-3xl font-bold text-gray-900 mb-2">Corpus Statistics</h1>
        <p class="text-gray-500">Live overview of the ChatGPT Corpus</p>
      </div>
      <div v-if="loading" class="text-gray-500 text-lg py-8 text-center">Loading...</div>
      <div v-else-if="!statsLoaded" class="text-gray-400 text-lg py-8 text-center">No data available.</div>
      <div v-else class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div class="card p-6 bg-white rounded-xl shadow-sm border border-gray-100 flex flex-col items-center">
          <span class="stat-number">{{ stats.conversations || 0 }}</span>
          <span class="stat-label">Conversations</span>
        </div>
        <div class="card p-6 bg-white rounded-xl shadow-sm border border-gray-100 flex flex-col items-center">
          <span class="stat-number">{{ stats.messages || 0 }}</span>
          <span class="stat-label">Messages</span>
        </div>
        <div class="card p-6 bg-white rounded-xl shadow-sm border border-gray-100 flex flex-col items-center">
          <span class="stat-number">{{ stats.uploads || 0 }}</span>
          <span class="stat-label">Uploads</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { fetchCorpusStats } from '../services/corpusInfoService'

const stats = ref({
  conversations: 0,
  messages: 0,
  uploads: 0
})
const loading = ref(true)
const statsLoaded = ref(false)

onMounted(async () => {
  loading.value = true
  const result = await fetchCorpusStats();
  if (result.success) {
    console.log('Received stats:', result.stats); // Debug log
    stats.value = result.stats;
    statsLoaded.value = true;
  } else {
    console.error('Failed to fetch stats:', result.error);
    statsLoaded.value = false;
  }
  loading.value = false
})
</script>

<style scoped>
.stat-number {
  font-size: 2.5rem;
  font-weight: 700;
  color: #111827;
  margin-bottom: 0.25rem;
}
.stat-label {
  font-size: 1rem;
  color: #6b7280;
  font-weight: 500;
}
.card {
  transition: box-shadow 0.2s;
}
.card:hover {
  box-shadow: 0 4px 24px rgba(0,0,0,0.08);
}
</style> 
