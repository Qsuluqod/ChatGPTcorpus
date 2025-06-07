<template>
  <nav class="fixed top-0 left-0 right-0 z-50 bg-gradient-to-r from-primary-from to-primary-to animate-glow">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-16">
        <div class="flex items-center">
          <router-link to="/" class="text-white text-xl font-bold tracking-wide hover:text-white/90 transition-colors">
            ChatGPT Corpus
          </router-link>
        </div>
        <div class="flex space-x-4">
          <router-link
            to="/search"
            class="text-white/90 hover:text-white px-3 py-2 rounded-md text-sm font-medium transition-all duration-200 hover:bg-white/10"
            :class="{ 'bg-white/20 text-white shadow-[0_0_15px_rgba(255,255,255,0.5)]': $route.path === '/search' }"
          >
            Search
          </router-link>
          <router-link
            to="/contribute"
            class="text-white/90 hover:text-white px-3 py-2 rounded-md text-sm font-medium transition-all duration-200 hover:bg-white/10"
            :class="{ 'bg-white/20 text-white shadow-[0_0_15px_rgba(255,255,255,0.5)]': $route.path === '/contribute' }"
          >
            Contribute
          </router-link>
          <router-link
            to="/corpus-info"
            class="text-white/90 hover:text-white px-3 py-2 rounded-md text-sm font-medium transition-all duration-200 hover:bg-white/10"
            :class="{ 'bg-white/20 text-white shadow-[0_0_10px_rgba(255,255,255,0.3)]': $route.path === '/corpus-info' }"
          >
            Corpus Info
          </router-link>
        </div>
      </div>
    </div>
  </nav>
</template>

<script setup>
// No additional setup needed
</script>

<style scoped>
nav {
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
}
</style> 