<template>
  <div class="max-w-3xl mx-auto px-4 py-12 space-y-6">
    <div class="text-center space-y-4">
      <h1 class="text-4xl font-bold text-gray-900">Welcome to ChatGPT Corpus</h1>
      <p class="text-gray-700 text-lg">
        This project collects ChatGPT conversations for linguistic research. All uploaded
        files are automatically anonymized before they become part of the public corpus.
      </p>
      <p class="text-gray-700">
        Curious about the data? Head over to the
        <router-link to="/search" class="text-purple-600 hover:underline">search page</router-link>
        or help the project grow by
        <router-link to="/contribute" class="text-purple-600 hover:underline">contributing your own conversations</router-link>.
      </p>
    </div>
  </div>
</template>

<script setup>
// No logic needed
</script>
